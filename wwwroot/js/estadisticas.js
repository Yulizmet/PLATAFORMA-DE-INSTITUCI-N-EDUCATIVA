// Requiere jQuery (ya incluido en _Layout) y Chart.js (se carga en la vista)
(function () {
    // Obtener datos serializados desde la vista
    const raw = window.__estadisticasData || {};
    const students = (typeof raw.students === 'string') ? JSON.parse(raw.students) : (raw.students || []);
    const employees = (typeof raw.employees === 'string') ? JSON.parse(raw.employees) : (raw.employees || []);

    let pieChart, barChart, employeeActivityChart;

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
            options: { responsive: true, maintainAspectRatio: false }
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
            options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true, max: 10 } } }
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
            options: { responsive: true, maintainAspectRatio: false }
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
            options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true } } }
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
            options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true } } }
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
    });
})();