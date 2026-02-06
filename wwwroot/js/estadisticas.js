// Requiere jQuery (ya incluido en _Layout) y Chart.js (se carga en la vista)
(function () {
    const students = window.__estadisticasData || [];
    let pieChart, barChart;

    function computeStats(list) {
        const stats = { Total: list.length, Inscrito: 0, Cursando: 0, Aprobado: 0, Reprobado: 0 };
        list.forEach(s => {
            stats[s.Estado] = (stats[s.Estado] || 0) + 1;
        });
        return stats;
    }

    function updateStatCards(list) {
        const st = computeStats(list);
        $('#statTotal').text(st.Total);
        $('#statInscritos').text(st.Inscrito || 0);
        $('#statCursando').text(st.Cursando || 0);
        $('#statAprobados').text(st.Aprobado || 0);
    }

    function renderPie(list) {
        const pieEl = document.getElementById('pieChart');
        if (!pieEl) return;
        // asegurar tamaño del canvas según CSS de .chart-container
        pieEl.style.width = '100%';
        pieEl.style.height = '100%';

        const st = computeStats(list);
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

    function renderBar(list) {
        const barEl = document.getElementById('barChart');
        if (!barEl) return;
        barEl.style.width = '100%';
        barEl.style.height = '100%';

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
        const averages = labels.map(l => byCourse[l].count ? (byCourse[l].sum / byCourse[l].count).toFixed(2) : 0);
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

    function applyFilters() {
        const name = $('#filterName').val() ? $('#filterName').val().toLowerCase() : '';
        const status = $('#filterStatus').val();
        const rows = $('#studentsTable tbody tr');

        const filtered = [];
        rows.each(function () {
            const $tr = $(this);
            const tname = ($tr.data('name') || '').toString().toLowerCase();
            const tstatus = ($tr.data('status') || '').toString();
            const matchName = !name || tname.includes(name);
            const matchStatus = !status || tstatus === status;
            if (matchName && matchStatus) {
                $tr.show();
                filtered.push({
                    Id: $tr.find('td').eq(0).text(),
                    Nombre: $tr.find('td').eq(1).text(),
                    Curso: $tr.find('td').eq(2).text(),
                    Estado: tstatus,
                    Nota: parseFloat($tr.find('td').eq(4).text()) || 0
                });
            } else {
                $tr.hide();
            }
        });

        updateStatCards(filtered);
        renderPie(filtered);
        renderBar(filtered);
    }

    $(function () {
        // inicializar con todos los datos
        updateStatCards(students);
        renderPie(students);
        renderBar(students);

        $('#filterName').on('input', applyFilters);
        $('#filterStatus').on('change', applyFilters);
        $('#resetFilters').on('click', function () {
            $('#filterName').val('');
            $('#filterStatus').val('');
            applyFilters();
        });
    });
})();