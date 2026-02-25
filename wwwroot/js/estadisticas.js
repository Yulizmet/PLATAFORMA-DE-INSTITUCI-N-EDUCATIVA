// Requiere jQuery (ya incluido en _Layout) y Chart.js (se carga en la vista)
(function () {
    // Obtener datos serializados desde la vista
    const raw = window.__estadisticasData || {};
    const students = (typeof raw.students === 'string') ? JSON.parse(raw.students) : (raw.students || []);
    const employees = (typeof raw.employees === 'string') ? JSON.parse(raw.employees) : (raw.employees || []);

    let pieChart, barChart, employeeActivityChart;

    // Plugin para mostrar pequeñas etiquetas con los valores sobre/ dentro de los elementos
    const valueLabelPlugin = {
        id: 'valueLabelPlugin',
        afterDatasetsDraw(chart, args, pluginOptions) {
            const ctx = chart.ctx;
            ctx.save();
            const opts = pluginOptions || {};
            const font = opts.font || '12px Arial';
            ctx.font = font;
            const textColor = opts.color || '#000';
            const bgColor = opts.bgColor || 'rgba(108,117,125,0.9)'; // gris semitransparente por defecto
            const padding = (typeof opts.padding === 'number') ? opts.padding : 6;
            const borderColor = opts.borderColor || '#000';
            const borderWidth = (typeof opts.borderWidth === 'number') ? opts.borderWidth : 1;
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';

            chart.data.datasets.forEach((dataset, dsIndex) => {
                const meta = chart.getDatasetMeta(dsIndex);
                meta.data.forEach((elem, index) => {
                    let value = dataset.data && dataset.data[index] !== undefined ? dataset.data[index] : null;
                    if (value === null) return;
                    const label = (opts.formatter && typeof opts.formatter === 'function') ? opts.formatter(value, dataset, index) : String(value);

                    // Element provides tooltipPosition for many element types (bars, arcs)
                    if (typeof elem.tooltipPosition === 'function') {
                        const pos = elem.tooltipPosition();
                        // For bars show above, for arcs position will be near center
                        const yOffset = (elem.height) ? -8 : 0;
                            drawLabelBoxAndText(ctx, label, pos.x, pos.y + yOffset, font, textColor, bgColor, padding, borderColor, borderWidth);
                    } else if (elem.x !== undefined && elem.y !== undefined) {
                        // Fallback: for arc elements compute centroid
                        if (elem.startAngle !== undefined && elem.endAngle !== undefined) {
                            const mid = (elem.startAngle + elem.endAngle) / 2;
                            const r = ((elem.outerRadius || 0) + (elem.innerRadius || 0)) / 2 || (elem.outerRadius || 0) * 0.7;
                            const x = elem.x + Math.cos(mid) * r;
                            const y = elem.y + Math.sin(mid) * r;
                            drawLabelBoxAndText(ctx, label, x, y, font, textColor, bgColor, padding, borderColor, borderWidth);
                        } else {
                            drawLabelBoxAndText(ctx, label, elem.x, elem.y - 10, font, textColor, bgColor, padding, borderColor, borderWidth);
                        }
                    }
                });
            });

            // Helper: dibuja un recuadro y el texto centrado en (x,y)
            function drawLabelBoxAndText(ctx, text, x, y, font, color, bg, padding, borderColor, borderWidth) {
                ctx.save();
                ctx.font = font;
                // medir texto
                const metrics = ctx.measureText(text);
                const textWidth = metrics.width;
                // medir altura (fallback si no está disponible)
                const textHeight = (metrics.actualBoundingBoxAscent !== undefined && metrics.actualBoundingBoxDescent !== undefined)
                    ? (metrics.actualBoundingBoxAscent + metrics.actualBoundingBoxDescent)
                    : parseInt((font || '12px').replace('px', '')) || 12;

                const boxWidth = textWidth + padding * 2;
                const boxHeight = textHeight + padding * 2;
                const bx = x - boxWidth / 2;
                const by = y - boxHeight / 2;

                // dibujar rectángulo con esquinas ligeramente redondeadas y borde
                const radius = Math.min(6, boxHeight / 2);
                ctx.fillStyle = bg;
                ctx.strokeStyle = borderColor || '#000';
                ctx.lineWidth = borderWidth || 1;
                roundRect(ctx, bx, by, boxWidth, boxHeight, radius, true, true);

                // dibujar texto encima
                ctx.fillStyle = color;
                ctx.textAlign = 'center';
                ctx.textBaseline = 'middle';
                ctx.fillText(text, x, y);
                ctx.restore();
            }

            function roundRect(ctx, x, y, width, height, radius, fill, stroke) {
                if (typeof radius === 'undefined') radius = 5;
                if (typeof radius === 'number') {
                    radius = { tl: radius, tr: radius, br: radius, bl: radius };
                } else {
                    const defaultRadius = { tl: 0, tr: 0, br: 0, bl: 0 };
                    for (const side in defaultRadius) radius[side] = radius[side] || defaultRadius[side];
                }
                ctx.beginPath();
                ctx.moveTo(x + radius.tl, y);
                ctx.lineTo(x + width - radius.tr, y);
                ctx.quadraticCurveTo(x + width, y, x + width, y + radius.tr);
                ctx.lineTo(x + width, y + height - radius.br);
                ctx.quadraticCurveTo(x + width, y + height, x + width - radius.br, y + height);
                ctx.lineTo(x + radius.bl, y + height);
                ctx.quadraticCurveTo(x, y + height, x, y + height - radius.bl);
                ctx.lineTo(x, y + radius.tl);
                ctx.quadraticCurveTo(x, y, x + radius.tl, y);
                ctx.closePath();
                if (fill) ctx.fill();
                if (stroke) ctx.stroke();
            }

            ctx.restore();
        }
    };

    // Registrar el plugin globalmente si Chart está disponible
    if (window.Chart && typeof Chart.register === 'function') {
        try { Chart.register(valueLabelPlugin); } catch (e) { /* ya registrado u otra versión */ }
    }

    // --- Utilidades de gráficos y estadísitcas ---
    function computeStudentStats(list) {
        const stats = { Total: list.length, Inscrito: 0, Cursando: 0, Aprobado: 0, Reprobado: 0 };
        list.forEach(s => {
            stats[s.Estado] = (stats[s.Estado] || 0) + 1;
        });
        return stats;
    }

    function updateStudentCards(list) {
        const st = computeStudentStats(list);
        $('#statTotal').text(st.Total);
        $('#statInscritos').text(st.Inscrito || 0);
        $('#statCursando').text(st.Cursando || 0);
        $('#statAprobados').text(st.Aprobado || 0);
    }

    function renderStudentPie(list) {
        const pieEl = document.getElementById('pieChart');
        if (!pieEl) return;
        const st = computeStudentStats(list);
        const data = [
            st.Inscrito || 0,
            st.Cursando || 0,
            st.Aprobado || 0,
            st.Reprobado || 0
        ];
        const ctx = pieEl.getContext('2d');
        if (pieChart) pieChart.destroy();
        pieChart = new Chart(ctx, {
            type: 'pie',
            data: {
                labels: ['Inscrito', 'Cursando', 'Aprobado', 'Reprobado'],
                datasets: [{ data, backgroundColor: ['#6c757d', '#ffc107', '#198754', '#dc3545'] }]
            },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, plugins: { valueLabelPlugin: { color: '#fff', font: '12px Arial', formatter: function (v) { return v; } } } }
        });
    }

    function renderStudentBar(list) {
        const barEl = document.getElementById('barChart');
        if (!barEl) return;
        // promedio de nota por curso
        const byCourse = {};
        list.forEach(s => {
            if (!byCourse[s.Curso]) byCourse[s.Curso] = { sum: 0, count: 0 };
            if (s.Nota && s.Nota > 0) {
                byCourse[s.Curso].sum += s.Nota;
                byCourse[s.Curso].count += 1;
            }
        });
        const labels = Object.keys(byCourse);
        const averages = labels.map(l => byCourse[l].count ? +(byCourse[l].sum / byCourse[l].count).toFixed(2) : 0);
        const ctx = barEl.getContext('2d');
        if (barChart) barChart.destroy();
        barChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels,
                datasets: [{ label: 'Promedio nota', data: averages, backgroundColor: '#0d6efd' }]
            },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true, max: 10 } }, plugins: { valueLabelPlugin: { color: '#000', font: '12px Arial', formatter: function (v) { return v; } } } }
        });
    }

    // Gráficos para empleados
    function computeEmployeesByDepartment(list) {
        const byDept = {};
        list.forEach(e => {
            byDept[e.Departamento] = (byDept[e.Departamento] || 0) + 1;
        });
        return byDept;
    }

    function renderEmployeePie(list) {
        const pieEl = document.getElementById('pieChart');
        if (!pieEl) return;
        const byDept = computeEmployeesByDepartment(list);
        const labels = Object.keys(byDept);
        const data = labels.map(l => byDept[l]);
        const ctx = pieEl.getContext('2d');
        if (pieChart) pieChart.destroy();
        pieChart = new Chart(ctx, {
            type: 'pie',
            data: {
                labels,
                datasets: [{ data, backgroundColor: labels.map((_, i) => ['#0d6efd', '#198754', '#ffc107', '#dc3545', '#6c757d'][i % 5]) }]
            },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, plugins: { valueLabelPlugin: { color: '#fff', font: '12px Arial', formatter: function (v) { return v; } } } }
        });
    }

    function computeEmployeesByRole(list) {
        const byRole = {};
        list.forEach(e => {
            byRole[e.Rol] = (byRole[e.Rol] || 0) + 1;
        });
        return byRole;
    }

    function renderEmployeeBar(list) {
        const barEl = document.getElementById('barChart');
        if (!barEl) return;
        const byRole = computeEmployeesByRole(list);
        const labels = Object.keys(byRole);
        const data = labels.map(l => byRole[l]);
        const ctx = barEl.getContext('2d');
        if (barChart) barChart.destroy();
        barChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels,
                datasets: [{ label: 'Cantidad por rol', data, backgroundColor: '#0d6efd' }]
            },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true } }, plugins: { valueLabelPlugin: { color: '#000', font: '12px Arial', formatter: function (v) { return v; } } } }
        });
    }

    // Gráfica exclusiva: actividades por empleado (barra)
    function renderEmployeeActivityChart(list) {
        const el = document.getElementById('employeeActivityChart');
        if (!el) return;
        const labels = list.map(e => e.Nombre);
        const data = list.map(e => e.ActividadesHoy || 0);
        const ctx = el.getContext('2d');
        if (employeeActivityChart) employeeActivityChart.destroy();
        employeeActivityChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels,
                datasets: [{ label: 'Actividades hoy', data, backgroundColor: '#198754' }]
            },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true } }, plugins: { valueLabelPlugin: { color: '#fff', font: '12px Arial', formatter: function (v) { return v; } } } }
        });
    }

    // --- Filtrado y actualización de tablas ---
    function applyStudentFilters() {
        const name = ($('#filterName').val() || '').toString().toLowerCase();
        const status = $('#filterStatus').val();
        const genero = $('#filterGenero').val();
        const semestre = $('#filterSemestre').val();

        const rows = $('#studentsTable tbody tr');
        const filtered = [];

        rows.each(function () {
            const $tr = $(this);
            const tname = ($tr.data('name') || '').toString().toLowerCase();
            const tstatus = ($tr.data('status') || '').toString();
            const tgenero = ($tr.data('genero') || '').toString();
            const tsemes = ($tr.data('semestre') || '').toString();

            const matchName = !name || tname.includes(name);
            const matchStatus = !status || tstatus === status;
            const matchGenero = !genero || tgenero === genero;
            const matchSemestre = !semestre || tsemes === semestre;

            if (matchName && matchStatus && matchGenero && matchSemestre) {
                $tr.show();
                filtered.push({
                    Id: parseInt($tr.find('td').eq(0).text()) || 0,
                    Nombre: $tr.find('td').eq(1).text(),
                    Curso: $tr.find('td').eq(3).text(),
                    Estado: tstatus,
                    Nota: parseFloat($tr.find('td').eq(6).text()) || 0
                });
            } else {
                $tr.hide();
            }
        });

        updateStudentCards(filtered);
        renderStudentPie(filtered);
        renderStudentBar(filtered);
    }

    function resetStudentFilters() {
        $('#filterName').val('');
        $('#filterStatus').val('');
        $('#filterGenero').val('');
        $('#filterSemestre').val('');
        $('#studentsTable tbody tr').show();
        updateStudentCards(students);
        renderStudentPie(students);
        renderStudentBar(students);
    }

    function applyEmployeeFilters() {
        const name = ($('#filterEmployeeName').val() || '').toString().toLowerCase();
        const department = $('#filterDepartment').val();

        const rows = $('#employeesTable tbody tr');
        const filtered = [];

        rows.each(function () {
            const $tr = $(this);
            const tname = ($tr.data('name') || '').toString().toLowerCase();
            const tdept = ($tr.data('department') || '').toString();

            const matchName = !name || tname.includes(name);
            const matchDept = !department || tdept === department;

            if (matchName && matchDept) {
                $tr.show();
                filtered.push({
                    Id: parseInt($tr.find('td').eq(0).text()) || 0,
                    Nombre: $tr.find('td').eq(1).text(),
                    Departamento: $tr.find('td').eq(3).text(),
                    Rol: $tr.find('td').eq(4).text(),
                    ActividadesHoy: parseInt($tr.find('td').eq(5).text()) || 0
                });
            } else {
                $tr.hide();
            }
        });

        // Actualizar gráficas de empleados con filtered si hay resultados, si no con todos
        const toUse = filtered.length ? filtered : employees;
        renderEmployeePie(toUse);
        renderEmployeeBar(toUse);
        renderEmployeeActivityChart(toUse);
    }

    function resetEmployeeFilters() {
        $('#filterEmployeeName').val('');
        $('#filterDepartment').val('');
        $('#employeesTable tbody tr').show();
        renderEmployeePie(employees);
        renderEmployeeBar(employees);
        renderEmployeeActivityChart(employees);
    }

    // --- Exportar tabla visible a CSV (compatible con Excel) ---
    function exportVisibleTableToCSV(tableSelector, filename) {
        const $table = $(tableSelector);
        if (!$table.length) return;

        const rows = [];
        // Cabeceras
        const headers = [];
        $table.find('thead th').each(function () {
            headers.push(csvEscape($(this).text().trim()));
        });
        rows.push(headers.join(','));

        // Filas visibles
        $table.find('tbody tr:visible').each(function () {
            const cols = [];
            $(this).find('td').each(function () {
                cols.push(csvEscape($(this).text().trim()));
            });
            rows.push(cols.join(','));
        });

        const csvContent = '\uFEFF' + rows.join('\n'); // BOM para Excel
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        const url = URL.createObjectURL(blob);
        link.setAttribute('href', url);
        link.setAttribute('download', filename || 'export.csv');
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    }

    function csvEscape(text) {
        if (text == null) return '';
        // Si contiene comillas, comas o saltos de línea, encerrar entre comillas y duplicar comillas internas
        const needsQuotes = /[",\n\r,]/.test(text);
        let out = text.replace(/\"/g, '""');
        if (needsQuotes) out = '"' + out + '"';
        return out;
    }

    // --- Cambio de vista: se disparan desde los botones en la página;
    // añadimos listeners para mantener los gráficos sincronizados ---
    function onShowStudentsView() {
        // destruir gráfica de actividades si existe
        if (employeeActivityChart) {
            employeeActivityChart.destroy();
            employeeActivityChart = null;
        }
        // restaurar gráficos de estudiantes
        updateStudentCards(students);
        renderStudentPie(students);
        renderStudentBar(students);
    }

    function onShowEmployeesView() {
        // renderizar gráficas de empleados
        renderEmployeePie(employees);
        renderEmployeeBar(employees);
        renderEmployeeActivityChart(employees);
    }

    // --- Inicialización y eventos ---
    $(function () {
        // Inicializar con todos los estudiantes
        updateStudentCards(students);
        renderStudentPie(students);
        renderStudentBar(students);

        // Listeners para filtros estudiantes
        $('#filterName').on('input', applyStudentFilters);
        $('#filterStatus').on('change', applyStudentFilters);
        $('#filterGenero').on('change', applyStudentFilters);
        $('#filterSemestre').on('change', applyStudentFilters);
        $('#applyStudentFilters').on('click', applyStudentFilters);
        $('#resetFilters').on('click', resetStudentFilters);

        // Listeners para filtros empleados
        $('#filterEmployeeName').on('input', applyEmployeeFilters);
        $('#filterDepartment').on('change', applyEmployeeFilters);
        $('#applyEmployeeFilters').on('click', applyEmployeeFilters);
        $('#resetEmployeeFilters').on('click', resetEmployeeFilters);

        // Sincronizar con botones de pestañas (existen en la Razor page)
        $('#tabStudents').on('click', function () {
            onShowStudentsView();
        });
        $('#tabEmployees').on('click', function () {
            onShowEmployeesView();
        });

        // Botón de exportar tabla actual (respeta filtros porque exporta sólo filas visibles)
        $('#btnExportTable').on('click', function () {
            const now = new Date();
            const datePart = now.toISOString().slice(0,10);
            if ($('#viewStudents').is(':visible')) {
                exportVisibleTableToCSV('#studentsTable', `alumnos_${datePart}.csv`);
            } else {
                exportVisibleTableToCSV('#employeesTable', `empleados_${datePart}.csv`);
            }
        });
    });
})();