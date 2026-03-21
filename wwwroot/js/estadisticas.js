// estadisticas.js — Calificaciones, Servicios Sociales, Trámites, Bitácoras
(function () {
    // ── Datos ────────────────────────────────────────────────────────────────
    const raw = window.__estadisticasData || {};
    const students       = (typeof raw.students       === 'string') ? JSON.parse(raw.students)       : (raw.students       || []);
    const socialServices = (typeof raw.socialServices === 'string') ? JSON.parse(raw.socialServices) : (raw.socialServices || []);
    const procedures     = (typeof raw.procedures     === 'string') ? JSON.parse(raw.procedures)     : (raw.procedures     || []);
    const psychologyLogs = (typeof raw.psychologyLogs === 'string') ? JSON.parse(raw.psychologyLogs) : (raw.psychologyLogs || []);
    const medicalLogs    = (typeof raw.medicalLogs    === 'string') ? JSON.parse(raw.medicalLogs)    : (raw.medicalLogs    || []);

    // Estado de listas filtradas (para social y trámites que viven en memoria)
    let filteredSocialServices = [...socialServices];
    let filteredProcedures = [...procedures];

    // Paginación
    const PAGE_SIZE = 15;
    let currentPageSocial = 1;
    let currentPageTramites = 1;

    // Instancias de gráficas — se destruyen y recrean al filtrar
    let charts = {};

    // ── Plugin de etiquetas sobre barras / pie ───────────────────────────────
    const valueLabelPlugin = {
        id: 'valueLabelPlugin',
        afterDatasetsDraw(chart, _args, opts) {
            const ctx = chart.ctx;
            ctx.save();
            const font = (opts && opts.font) || '12px Arial';
            const textColor = (opts && opts.color) || '#000';
            const bg = (opts && opts.bgColor) || 'rgba(60,60,60,0.85)';
            const pad = (opts && typeof opts.padding === 'number') ? opts.padding : 5;
            const fmt = (opts && typeof opts.formatter === 'function') ? opts.formatter : v => String(v);

            chart.data.datasets.forEach((dataset, di) => {
                chart.getDatasetMeta(di).data.forEach((elem, i) => {
                    const value = dataset.data ? dataset.data[i] : null;
                    if (value === null || value === undefined || value === 0) return;
                    const label = fmt(value, dataset, i);

                    let x, y;
                    if (typeof elem.tooltipPosition === 'function') {
                        const pos = elem.tooltipPosition();
                        x = pos.x; y = pos.y + (elem.height ? -8 : 0);
                    } else if (elem.startAngle !== undefined) {
                        const mid = (elem.startAngle + elem.endAngle) / 2;
                        const r = ((elem.outerRadius || 0) + (elem.innerRadius || 0)) / 2 || (elem.outerRadius || 0) * 0.7;
                        x = elem.x + Math.cos(mid) * r;
                        y = elem.y + Math.sin(mid) * r;
                    } else {
                        x = elem.x; y = (elem.y || 0) - 10;
                    }

                    ctx.font = font;
                    const tw = ctx.measureText(label).width;
                    const th = parseInt(font) || 12;
                    const bw = tw + pad * 2, bh = th + pad * 2;
                    const bx = x - bw / 2, by = y - bh / 2;
                    const r2 = Math.min(5, bh / 2);

                    ctx.fillStyle = bg;
                    ctx.beginPath();
                    ctx.moveTo(bx + r2, by);
                    ctx.lineTo(bx + bw - r2, by);
                    ctx.quadraticCurveTo(bx + bw, by, bx + bw, by + r2);
                    ctx.lineTo(bx + bw, by + bh - r2);
                    ctx.quadraticCurveTo(bx + bw, by + bh, bx + bw - r2, by + bh);
                    ctx.lineTo(bx + r2, by + bh);
                    ctx.quadraticCurveTo(bx, by + bh, bx, by + bh - r2);
                    ctx.lineTo(bx, by + r2);
                    ctx.quadraticCurveTo(bx, by, bx + r2, by);
                    ctx.closePath();
                    ctx.fill();

                    ctx.fillStyle = textColor;
                    ctx.textAlign = 'center';
                    ctx.textBaseline = 'middle';
                    ctx.fillText(label, x, y);
                });
            });
            ctx.restore();
        }
    };

    if (window.Chart && typeof Chart.register === 'function') {
        try { Chart.register(valueLabelPlugin); } catch (_) { }
    }

    // ── Helpers genéricos ────────────────────────────────────────────────────
    function makeChart(id, config) {
        const el = document.getElementById(id);
        if (!el) return null;
        if (charts[id]) { charts[id].destroy(); }
        charts[id] = new Chart(el.getContext('2d'), config);
        return charts[id];
    }

    function emptyChart(id, cols) {
        makeChart(id, {
            type: 'bar',
            data: { labels: ['Sin datos'], datasets: [{ label: 'Sin datos', data: [1], backgroundColor: '#e9ecef' }] },
            options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } } }
        });
    }

    const pluginOpts = (color, bg) => ({
        valueLabelPlugin: { color: color || '#fff', font: '12px Arial', bgColor: bg || 'rgba(60,60,60,0.85)', formatter: v => v }
    });

    function baseBarOptions(extra) {
        return Object.assign({
            responsive: true,
            maintainAspectRatio: false,
            scales: { y: { beginAtZero: true, ticks: { precision: 0 } } },
            plugins: pluginOpts('#fff')
        }, extra || {});
    }

    function escapeHtml(s) {
        return String(s || '')
            .replace(/&/g, '&amp;').replace(/</g, '&lt;')
            .replace(/>/g, '&gt;').replace(/"/g, '&quot;');
    }
    function csvEscape(text) {
        if (text == null) return '';
        let out = String(text).replace(/\"/g, '""');
        if (/[",\n\r]/.test(out)) out = '"' + out + '"';
        return out;
    }

    // ════════════════════════════════════════════════════════════════════════
    // CALIFICACIONES — Gráficas
    // ════════════════════════════════════════════════════════════════════════
    function computeStudentStats(list) {
        const s = { Total: list.length, Inscrito: 0, Cursando: 0, Aprobado: 0, Reprobado: 0 };
        list.forEach(r => { s[r.Estado] = (s[r.Estado] || 0) + 1; });
        return s;
    }

    function updateStudentCards(list) {
        const s = computeStudentStats(list);
        $('#statTotal').text(s.Total);
        $('#statInscritos').text(s.Inscrito || 0);
        $('#statCursando').text(s.Cursando || 0);
        $('#statAprobados').text(s.Aprobado || 0);
    }

    function renderStudentPie(list) {
        const labels = [], data = [], colors = [];
        const map = { Inscrito: '#6c757d', Cursando: '#ffc107', Aprobado: '#198754', Reprobado: '#dc3545' };
        const st = computeStudentStats(list);
        Object.entries(map).forEach(([k, c]) => { if ((st[k] || 0) > 0) { labels.push(k); data.push(st[k]); colors.push(c); } });
        if (!data.length) { emptyChart('pieChart'); return; }
        makeChart('pieChart', {
            type: 'pie',
            data: { labels, datasets: [{ data, backgroundColor: colors }] },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, plugins: pluginOpts('#fff') }
        });
    }

    function renderStudentBar(list) {
        const byCourse = {};
        list.forEach(s => {
            if (!byCourse[s.Curso]) byCourse[s.Curso] = { sum: 0, count: 0 };
            if (s.Nota > 0) { byCourse[s.Curso].sum += s.Nota; byCourse[s.Curso].count++; }
        });
        const labels = Object.keys(byCourse).filter(l => byCourse[l].count > 0);
        if (!labels.length) { emptyChart('barChart'); return; }
        makeChart('barChart', {
            type: 'bar',
            data: { labels, datasets: [{ label: 'Promedio nota', data: labels.map(l => +(byCourse[l].sum / byCourse[l].count).toFixed(2)), backgroundColor: '#0d6efd' }] },
            plugins: [valueLabelPlugin],
            options: baseBarOptions({ scales: { y: { beginAtZero: true, max: 10 } }, plugins: pluginOpts('#fff') })
        });
    }

    function renderGradeHistogram(list) {
        const buckets = [0, 0, 0, 0, 0];
        list.forEach(s => {
            const n = Number(s.Nota); if (isNaN(n)) return;
            if (n < 2) buckets[0]++; else if (n < 4) buckets[1]++;
            else if (n < 6) buckets[2]++; else if (n < 8) buckets[3]++; else buckets[4]++;
        });
        const allLabels = ['0-2', '2-4', '4-6', '6-8', '8-10'];
        const fl = [], fb = [];
        buckets.forEach((b, i) => { if (b > 0) { fl.push(allLabels[i]); fb.push(b); } });
        if (!fb.length) { emptyChart('gradeHistogramChart'); return; }
        makeChart('gradeHistogramChart', {
            type: 'bar',
            data: { labels: fl, datasets: [{ label: 'Alumnos', data: fb, backgroundColor: '#6f42c1' }] },
            plugins: [valueLabelPlugin],
            options: baseBarOptions({ plugins: pluginOpts('#fff') })
        });
    }

    function renderCourseStatusStacked(list) {
        const byCourse = {};
        list.forEach(s => {
            const c = s.Curso || 'Sin curso';
            if (!byCourse[c]) byCourse[c] = { Inscrito: 0, Cursando: 0, Aprobado: 0, Reprobado: 0 };
            byCourse[c][s.Estado] = (byCourse[c][s.Estado] || 0) + 1;
        });
        const labels = Object.keys(byCourse).filter(c =>
            (byCourse[c].Inscrito || 0) + (byCourse[c].Cursando || 0) + (byCourse[c].Aprobado || 0) + (byCourse[c].Reprobado || 0) > 0
        );
        if (!labels.length) { emptyChart('courseStatusStacked'); return; }
        makeChart('courseStatusStacked', {
            type: 'bar',
            data: {
                labels, datasets: [
                    { label: 'Inscrito', data: labels.map(l => byCourse[l].Inscrito || 0), backgroundColor: '#6c757d' },
                    { label: 'Cursando', data: labels.map(l => byCourse[l].Cursando || 0), backgroundColor: '#ffc107' },
                    { label: 'Aprobado', data: labels.map(l => byCourse[l].Aprobado || 0), backgroundColor: '#198754' },
                    { label: 'Reprobado', data: labels.map(l => byCourse[l].Reprobado || 0), backgroundColor: '#dc3545' }
                ]
            },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, scales: { x: { stacked: true }, y: { stacked: true, beginAtZero: true } }, plugins: { legend: { position: 'top' }, ...pluginOpts('#000') } }
        });
    }

    function renderAllStudentCharts(list) {
        renderStudentPie(list);
        renderStudentBar(list);
        renderGradeHistogram(list);
        renderCourseStatusStacked(list);
    }

    // ════════════════════════════════════════════════════════════════════════
    // SERVICIOS SOCIALES — Tabla + Gráficas + Filtros
    // ════════════════════════════════════════════════════════════════════════
    function getStatusBadge(status) {
        const m = {
            'Completado': '<span class="badge bg-success">Completado</span>',
            'En progreso': '<span class="badge bg-warning text-dark">En progreso</span>',
            'Pendiente': '<span class="badge bg-secondary">Pendiente</span>'
        };
        return m[status] || `<span class="badge bg-secondary">${escapeHtml(status)}</span>`;
    }

    function populateSocialServicesTable(list) {
        const tbody = $('#socialServiceTable tbody');
        if (!tbody.length) return;
        tbody.empty();
        if (!list || !list.length) {
            tbody.html('<tr><td colspan="9" class="text-center text-muted py-4">Sin datos disponibles</td></tr>');
            updateSocialPagInfo(0, 0, 0); return;
        }
        const start = (currentPageSocial - 1) * PAGE_SIZE;
        const end = Math.min(start + PAGE_SIZE, list.length);
        list.slice(start, end).forEach(item => {
            const lu = item.LastUpdate ? new Date(item.LastUpdate).toLocaleDateString('es-MX') : 'N/A';
            tbody.append(`<tr>
                <td>${escapeHtml(item.StudentName || 'Sin nombre')}</td>
                <td>${escapeHtml(item.TeacherName || 'Sin asignar')}</td>
                <td>${escapeHtml(item.GroupName || 'Sin grupo')}</td>
                <td>${item.HoursPracticas || 0}</td>
                <td>${item.HoursServicioSocial || 0}</td>
                <td>${item.TotalHours || 0}</td>
                <td>${(item.AttendanceRate || 0).toFixed(2)}%</td>
                <td>${getStatusBadge(item.Status)}</td>
                <td>${lu}</td>
            </tr>`);
        });
        updateSocialPagInfo(start + 1, end, list.length);
    }

    function updateSocialPagInfo(from, to, total) {
        $('#pageInfoSocial').text(total > 0 ? `Mostrando ${from}-${to} de ${total}` : 'Sin resultados');
    }

    function updateSocialServiceCards(list) {
        if (!list || !list.length) {
            $('#statSocialTotal').text('0'); $('#statSocialHours').text('0');
            $('#statSocialPending').text('0'); $('#statSocialAttendance').text('0%'); return;
        }
        $('#statSocialTotal').text(list.length);
        $('#statSocialHours').text(list.reduce((s, i) => s + (i.TotalHours || 0), 0));
        $('#statSocialPending').text(list.filter(i => i.Status === 'Pendiente').length);
        $('#statSocialAttendance').text((list.reduce((s, i) => s + (i.AttendanceRate || 0), 0) / list.length).toFixed(1) + '%');
    }

    // Gráfica 1: Pie — estado del servicio social
    function renderSocialPie(list) {
        const cnt = { Completado: 0, 'En progreso': 0, Pendiente: 0 };
        list.forEach(i => { if (cnt[i.Status] !== undefined) cnt[i.Status]++; else cnt['Pendiente']++; });
        const labels = Object.keys(cnt).filter(k => cnt[k] > 0);
        if (!labels.length) { emptyChart('socialPieChart'); return; }
        makeChart('socialPieChart', {
            type: 'doughnut',
            data: { labels, datasets: [{ data: labels.map(l => cnt[l]), backgroundColor: ['#198754', '#ffc107', '#6c757d'] }] },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' }, ...pluginOpts('#fff') } }
        });
    }

    // Gráfica 2: Barras horizontales — Top 10 alumnos por total de horas
    function renderSocialHoursBar(list) {
        if (!list.length) { emptyChart('socialHoursBar'); return; }
        const sorted = [...list].sort((a, b) => (b.TotalHours || 0) - (a.TotalHours || 0)).slice(0, 10);
        makeChart('socialHoursBar', {
            type: 'bar',
            data: {
                labels: sorted.map(i => (i.StudentName || '?').split(' ').slice(0, 2).join(' ')),
                datasets: [{ label: 'Horas totales', data: sorted.map(i => i.TotalHours || 0), backgroundColor: '#0d6efd' }]
            },
            plugins: [valueLabelPlugin],
            options: {
                indexAxis: 'y',
                responsive: true, maintainAspectRatio: false,
                scales: { x: { beginAtZero: true } },
                plugins: { legend: { display: false }, ...pluginOpts('#fff') }
            }
        });
    }

    // Gráfica 3: Barras — asistencia promedio por grupo
    function renderSocialAttendance(list) {
        if (!list.length) { emptyChart('socialAttendanceBar'); return; }
        const byGroup = {};
        list.forEach(i => {
            const g = i.GroupName || 'Sin grupo';
            if (!byGroup[g]) byGroup[g] = { sum: 0, count: 0 };
            byGroup[g].sum += (i.AttendanceRate || 0); byGroup[g].count++;
        });
        const labels = Object.keys(byGroup);
        const data = labels.map(l => +(byGroup[l].sum / byGroup[l].count).toFixed(1));
        makeChart('socialAttendanceBar', {
            type: 'bar',
            data: { labels, datasets: [{ label: 'Asistencia %', data, backgroundColor: '#20c997' }] },
            plugins: [valueLabelPlugin],
            options: baseBarOptions({
                scales: { y: { beginAtZero: true, max: 100, ticks: { callback: v => v + '%' } } },
                plugins: { legend: { display: false }, ...pluginOpts('#fff') }
            })
        });
    }

    // Gráfica 4: Barras apiladas — horas prácticas vs servicio social por grupo
    function renderSocialHoursStacked(list) {
        if (!list.length) { emptyChart('socialHoursStacked'); return; }
        const byGroup = {};
        list.forEach(i => {
            const g = i.GroupName || 'Sin grupo';
            if (!byGroup[g]) byGroup[g] = { prac: 0, serv: 0 };
            byGroup[g].prac += (i.HoursPracticas || 0);
            byGroup[g].serv += (i.HoursServicioSocial || 0);
        });
        const labels = Object.keys(byGroup);
        makeChart('socialHoursStacked', {
            type: 'bar',
            data: {
                labels, datasets: [
                    { label: 'Horas prácticas', data: labels.map(l => byGroup[l].prac), backgroundColor: '#fd7e14' },
                    { label: 'Horas serv. social', data: labels.map(l => byGroup[l].serv), backgroundColor: '#6610f2' }
                ]
            },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, scales: { x: { stacked: true }, y: { stacked: true, beginAtZero: true } }, plugins: { legend: { position: 'top' }, ...pluginOpts('#fff') } }
        });
    }

    function renderAllSocialCharts(list) {
        renderSocialPie(list);
        renderSocialHoursBar(list);
        renderSocialAttendance(list);
        renderSocialHoursStacked(list);
    }

    function applySocialFilters() {
        const name = ($('#filterSocialName').val() || '').toLowerCase().trim();
        const teacher = ($('#filterSocialTeacher').val() || '').toLowerCase().trim();
        const status = $('#filterSocialStatus').val() || '';
        const group = ($('#filterSocialGroup').val() || '').toLowerCase().trim();

        filteredSocialServices = socialServices.filter(i =>
            (!name || (i.StudentName || '').toLowerCase().includes(name)) &&
            (!teacher || (i.TeacherName || '').toLowerCase().includes(teacher)) &&
            (!status || (i.Status || '') === status) &&
            (!group || (i.GroupName || '').toLowerCase().includes(group))
        );
        currentPageSocial = 1;
        populateSocialServicesTable(filteredSocialServices);
        updateSocialServiceCards(filteredSocialServices);
        renderAllSocialCharts(filteredSocialServices);
    }

    function resetSocialFilters() {
        $('#filterSocialName,#filterSocialTeacher,#filterSocialGroup').val('');
        $('#filterSocialStatus').val('');
        socialFilters = {};
        filteredSocialServices = [...socialServices];
        currentPageSocial = 1;
        populateSocialServicesTable(filteredSocialServices);
        updateSocialServiceCards(filteredSocialServices);
        renderAllSocialCharts(filteredSocialServices);
    }

    // ════════════════════════════════════════════════════════════════════════
    // TRÁMITES — Tabla + Gráficas + Filtros
    // ════════════════════════════════════════════════════════════════════════
    function getProcStatusBadge(code, name) {
        const m = {
            'APPROVED': '<span class="badge bg-success">Pagó inscripción</span>',
            'PENDING': '<span class="badge bg-warning text-dark">Pendiente</span>',
            'REJECTED': '<span class="badge bg-danger">No pagó</span>'
        };
        return m[code] || `<span class="badge bg-secondary">${escapeHtml(name || 'Desconocido')}</span>`;
    }

    function populateProceduresTable(list) {
        const tbody = $('#proceduresTable tbody');
        if (!tbody.length) return;
        tbody.empty();
        if (!list || !list.length) {
            tbody.html('<tr><td colspan="8" class="text-center text-muted py-4">Sin datos disponibles</td></tr>');
            updateTramitesPagInfo(0, 0, 0); return;
        }
        const start = (currentPageTramites - 1) * PAGE_SIZE;
        const end = Math.min(start + PAGE_SIZE, list.length);
        list.slice(start, end).forEach(item => {
            const dc = item.DateCreated ? new Date(item.DateCreated).toLocaleDateString('es-MX') : 'N/A';
            const du = item.DateUpdated ? new Date(item.DateUpdated).toLocaleDateString('es-MX') : 'N/A';
            tbody.append(`<tr>
                <td>${escapeHtml(item.Folio || 'Sin folio')}</td>
                <td>${escapeHtml(item.StudentName || 'Sin nombre')}</td>
                <td>${escapeHtml(item.ProcedureType || 'Sin tipo')}</td>
                <td>${escapeHtml(item.AreaName || 'Sin área')}</td>
                <td>${getProcStatusBadge(item.InternalCode, item.StatusName)}</td>
                <td>${dc}</td>
                <td>${du}</td>
                <td>${item.DaysElapsed || 0} días</td>
            </tr>`);
        });
        updateTramitesPagInfo(start + 1, end, list.length);
    }

    function updateTramitesPagInfo(from, to, total) {
        $('#pageInfoTramites').text(total > 0 ? `Mostrando ${from}-${to} de ${total}` : 'Sin resultados');
    }

    function updateProcedureCards(list) {
        if (!list || !list.length) {
            $('#statProcsTotal,#statProcsAction,#statProcsInProgress,#statProcsFinalized').text('0'); return;
        }
        $('#statProcsTotal').text(list.length);
        $('#statProcsAction').text(list.filter(i => i.InternalCode === 'PENDING').length);
        $('#statProcsInProgress').text(list.filter(i => i.InternalCode !== 'APPROVED' && i.InternalCode !== 'REJECTED' && i.InternalCode !== 'PENDING').length);
        $('#statProcsFinalized').text(list.filter(i => i.InternalCode === 'APPROVED').length);
    }

    // Gráfica 1: Pie — estado de trámites
    function renderTramitesPie(list) {
        const cnt = { APPROVED: 0, PENDING: 0, REJECTED: 0, OTRO: 0 };
        const labels2 = { APPROVED: 'Pagó inscripción', PENDING: 'Pendiente', REJECTED: 'No pagó', OTRO: 'Otro' };
        const colors2 = { APPROVED: '#198754', PENDING: '#ffc107', REJECTED: '#dc3545', OTRO: '#6c757d' };
        list.forEach(i => { const k = cnt[i.InternalCode] !== undefined ? i.InternalCode : 'OTRO'; cnt[k]++; });
        const keys = Object.keys(cnt).filter(k => cnt[k] > 0);
        if (!keys.length) { emptyChart('tramitesPieChart'); return; }
        makeChart('tramitesPieChart', {
            type: 'doughnut',
            data: { labels: keys.map(k => labels2[k]), datasets: [{ data: keys.map(k => cnt[k]), backgroundColor: keys.map(k => colors2[k]) }] },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' }, ...pluginOpts('#fff') } }
        });
    }

    // Gráfica 2: Barras — cantidad de trámites por área
    function renderTramitesArea(list) {
        if (!list.length) { emptyChart('tramitesAreaBar'); return; }
        const byArea = {};
        list.forEach(i => { const a = i.AreaName || 'Sin área'; byArea[a] = (byArea[a] || 0) + 1; });
        const labels = Object.keys(byArea).sort((a, b) => byArea[b] - byArea[a]);
        makeChart('tramitesAreaBar', {
            type: 'bar',
            data: { labels, datasets: [{ label: 'Trámites', data: labels.map(l => byArea[l]), backgroundColor: '#0d6efd' }] },
            plugins: [valueLabelPlugin],
            options: baseBarOptions({ plugins: { legend: { display: false }, ...pluginOpts('#fff') } })
        });
    }

    // Gráfica 3: Barras horizontales — tipos de trámite más solicitados
    function renderTramitesTipo(list) {
        if (!list.length) { emptyChart('tramitesTipoBar'); return; }
        const byTipo = {};
        list.forEach(i => { const t = i.ProcedureType || 'Sin tipo'; byTipo[t] = (byTipo[t] || 0) + 1; });
        const sorted = Object.entries(byTipo).sort((a, b) => b[1] - a[1]).slice(0, 10);
        makeChart('tramitesTipoBar', {
            type: 'bar',
            data: { labels: sorted.map(([l]) => l), datasets: [{ label: 'Cantidad', data: sorted.map(([, v]) => v), backgroundColor: '#fd7e14' }] },
            plugins: [valueLabelPlugin],
            options: {
                indexAxis: 'y',
                responsive: true, maintainAspectRatio: false,
                scales: { x: { beginAtZero: true } },
                plugins: { legend: { display: false }, ...pluginOpts('#fff') }
            }
        });
    }

    // Gráfica 4: Barras — días promedio de resolución por área
    function renderTramitesDias(list) {
        if (!list.length) { emptyChart('tramitesDiasBar'); return; }
        const byArea = {};
        list.forEach(i => {
            const a = i.AreaName || 'Sin área';
            if (!byArea[a]) byArea[a] = { sum: 0, count: 0 };
            byArea[a].sum += (i.DaysElapsed || 0); byArea[a].count++;
        });
        const labels = Object.keys(byArea);
        const data = labels.map(l => +(byArea[l].sum / byArea[l].count).toFixed(1));
        makeChart('tramitesDiasBar', {
            type: 'bar',
            data: { labels, datasets: [{ label: 'Días promedio', data, backgroundColor: '#20c997' }] },
            plugins: [valueLabelPlugin],
            options: baseBarOptions({ plugins: { legend: { display: false }, ...pluginOpts('#fff') } })
        });
    }

    function renderAllTramitesCharts(list) {
        renderTramitesPie(list);
        renderTramitesArea(list);
        renderTramitesTipo(list);
        renderTramitesDias(list);
    }

    function applyTramiteFilters() {
        const user = ($('#filterTramiteUser').val() || '').toLowerCase().trim();
        const folio = ($('#filterTramiteFolio').val() || '').toLowerCase().trim();
        const status = $('#filterTramiteStatus').val() || '';
        const area = ($('#filterTramiteArea').val() || '').toLowerCase().trim();

        filteredProcedures = procedures.filter(i =>
            (!user || (i.StudentName || '').toLowerCase().includes(user)) &&
            (!folio || String(i.Folio || '').toLowerCase().includes(folio)) &&
            (!status || (i.InternalCode || '') === status) &&
            (!area || (i.AreaName || '').toLowerCase().includes(area))
        );
        currentPageTramites = 1;
        populateProceduresTable(filteredProcedures);
        updateProcedureCards(filteredProcedures);
        renderAllTramitesCharts(filteredProcedures);
    }

    function resetTramiteFilters() {
        $('#filterTramiteUser,#filterTramiteFolio,#filterTramiteArea').val('');
        $('#filterTramiteStatus').val('');
        tramiteFilters = {};
        filteredProcedures = [...procedures];
        currentPageTramites = 1;
        populateProceduresTable(filteredProcedures);
        updateProcedureCards(filteredProcedures);
        renderAllTramitesCharts(filteredProcedures);
    }

    // ════════════════════════════════════════════════════════════════════════
    // CALIFICACIONES — Filtros del submenu
    // ════════════════════════════════════════════════════════════════════════
    function applyStudentFilters() {
        const name = ($('#filterName').val() || '').toLowerCase();
        const status = $('#filterStatus').val() || '';
        const genero = $('#filterGenero').val() || '';
        const semestre = $('#filterSemestre').val() || '';
        const rows = $('#studentsTable tbody tr');
        const filtered = [];
        rows.each(function () {
            const $tr = $(this);
            const ok = (!name || ($tr.data('name') || '').toString().toLowerCase().includes(name))
                && (!status || ($tr.data('status') || '') === status)
                && (!genero || ($tr.data('genero') || '') === genero)
                && (!semestre || ($tr.data('semestre') || '').toString() === semestre);
            $tr.toggle(ok);
            if (ok) filtered.push({
                Estado: $tr.data('status') || '',
                Curso: $tr.find('td').eq(3).text(),
                Nota: parseFloat($tr.find('td').eq(6).text()) || 0
            });
        });
        updateStudentCards(filtered);
        renderAllStudentCharts(filtered);
        studentFilters = {};
        currentPage = 1;
        updatePagination();
    }

    function resetStudentFilters() {
        $('#filterName').val(''); $('#filterStatus').val('');
        $('#filterGenero').val(''); $('#filterSemestre').val('');
        studentFilters = {};
        $('.excel-filter-popup').remove();
        $('#studentsTable tbody tr').show();
        currentPage = 1;
        updatePagination();
        updateStudentCards(students);
        renderAllStudentCharts(students);
    }

    // ════════════════════════════════════════════════════════════════════════
    // EXPORTAR CSV
    // ════════════════════════════════════════════════════════════════════════
    function exportVisibleTableToCSV(tableSelector, filename) {
        const $t = $(tableSelector);
        if (!$t.length) return;
        const rows = [];
        const h = []; $t.find('thead th').each(function () { h.push(csvEscape($(this).text().trim())); });
        rows.push(h.join(','));
        $t.find('tbody tr:visible').each(function () {
            const c = []; $(this).find('td').each(function () { c.push(csvEscape($(this).text().trim())); });
            rows.push(c.join(','));
        });
        downloadCSV(rows.join('\n'), filename);
    }

    function exportDataToCSV(data, headers, keys, filename) {
        if (!data || !data.length) return;
        const rows = [headers.map(csvEscape).join(',')];
        data.forEach(item => rows.push(keys.map(k => csvEscape(String(item[k] !== undefined ? item[k] : ''))).join(',')));
        downloadCSV(rows.join('\n'), filename);
    }

    function downloadCSV(content, filename) {
        const blob = new Blob(['\uFEFF' + content], { type: 'text/csv;charset=utf-8;' });
        const a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = filename || 'export.csv';
        document.body.appendChild(a); a.click();
        document.body.removeChild(a); URL.revokeObjectURL(a.href);
    }

    // ════════════════════════════════════════════════════════════════════════
    // BITÁCORAS — Psicología
    // ════════════════════════════════════════════════════════════════════════
    function populatePsychologyTable(list) {
        const tbody = $('#psychologyTable tbody');
        if (!tbody.length) return;
        tbody.empty();
        if (!list || !list.length) {
            tbody.html('<tr><td colspan="7" class="text-center text-muted py-4">Sin datos disponibles</td></tr>');
            return;
        }
        list.forEach(item => {
            const apptDate = item.AppointmentDate ? new Date(item.AppointmentDate).toLocaleString('es-MX') : 'N/A';
            const createdAt = item.CreatedAt ? new Date(item.CreatedAt).toLocaleDateString('es-MX') : 'N/A';
            tbody.append(`<tr>
                <td>${escapeHtml(item.Folio || 'Sin folio')}</td>
                <td>${escapeHtml(item.StudentName || 'Sin nombre')}</td>
                <td>${escapeHtml(item.EnrollmentOrMatricula || 'Sin matrícula')}</td>
                <td>${apptDate}</td>
                <td>${getPsyAttendanceBadge(item.AttendanceStatus)}</td>
                <td>${escapeHtml(item.Observations || '')}</td>
                <td>${createdAt}</td>
            </tr>`);
        });
    }

    function getPsyAttendanceBadge(status) {
        const m = {
            'Asistió': '<span class="badge bg-success">Asistió</span>',
            'No asistió': '<span class="badge bg-danger">No asistió</span>',
            'Justificado': '<span class="badge bg-warning text-dark">Justificado</span>'
        };
        return m[status] || `<span class="badge bg-secondary">${escapeHtml(status || 'Sin estado')}</span>`;
    }

    function updatePsychologyCards(list) {
        if (!list || !list.length) {
            $('#statPsyTotal,#statPsyAttended,#statPsyAbsent,#statPsyJustified').text('0'); return;
        }
        $('#statPsyTotal').text(list.length);
        $('#statPsyAttended').text(list.filter(i => (i.AttendanceStatus || '') === 'Asistió').length);
        $('#statPsyAbsent').text(list.filter(i => (i.AttendanceStatus || '') === 'No asistió').length);
        $('#statPsyJustified').text(list.filter(i => (i.AttendanceStatus || '') === 'Justificado').length);
    }

    // ════════════════════════════════════════════════════════════════════════
    // BITÁCORAS — Enfermería
    // ════════════════════════════════════════════════════════════════════════
    function populateMedicalTable(list) {
        const tbody = $('#medicalTable tbody');
        if (!tbody.length) return;
        tbody.empty();
        if (!list || !list.length) {
            tbody.html('<tr><td colspan="9" class="text-center text-muted py-4">Sin datos disponibles</td></tr>');
            return;
        }
        list.forEach(item => {
            const recDate = item.RecordDate ? new Date(item.RecordDate).toLocaleString('es-MX') : 'N/A';
            tbody.append(`<tr>
                <td>${escapeHtml(item.Folio || 'Sin folio')}</td>
                <td>${escapeHtml(item.StudentName || 'Sin nombre')}</td>
                <td>${escapeHtml(item.EnrollmentOrMatricula || 'Sin matrícula')}</td>
                <td>${recDate}</td>
                <td>${escapeHtml(item.ConsultationReason || '')}</td>
                <td>${escapeHtml(item.VitalSigns || '')}</td>
                <td>${escapeHtml(item.Observations || '')}</td>
                <td>${escapeHtml(item.TreatmentAction || '')}</td>
                <td>${getMedStatusBadge(item.Status)}</td>
            </tr>`);
        });
    }

    function getMedStatusBadge(status) {
        const m = {
            'Estable': '<span class="badge bg-success">Estable</span>',
            'Alta': '<span class="badge bg-success">Alta</span>',
            'Urgente': '<span class="badge bg-danger">Urgente</span>',
            'Critico': '<span class="badge bg-danger">Crítico</span>',
            'Observacion': '<span class="badge bg-warning text-dark">Observación</span>',
            'Pendiente': '<span class="badge bg-warning text-dark">Pendiente</span>'
        };
        return m[status] || `<span class="badge bg-secondary">${escapeHtml(status || 'Sin estado')}</span>`;
    }

    function updateMedicalCards(list) {
        if (!list || !list.length) {
            $('#statMedTotal,#statMedAttended,#statMedPending,#statMedOther').text('0'); return;
        }
        const attended = ['Estable', 'Alta'];
        const pending  = ['Urgente', 'Critico', 'Observacion', 'Pendiente'];
        $('#statMedTotal').text(list.length);
        $('#statMedAttended').text(list.filter(i => attended.includes(i.Status || '')).length);
        $('#statMedPending').text(list.filter(i => pending.includes(i.Status || '')).length);
        $('#statMedOther').text(list.filter(i => !attended.includes(i.Status || '') && !pending.includes(i.Status || '')).length);
    }

    // ════════════════════════════════════════════════════════════════════════
    // INIT
    // ════════════════════════════════════════════════════════════════════════
    $(function () {
        // Calificaciones
        updateStudentCards(students);
        renderAllStudentCharts(students);

        // Servicios Sociales
        filteredSocialServices = [...socialServices];
        populateSocialServicesTable(filteredSocialServices);
        updateSocialServiceCards(filteredSocialServices);
        renderAllSocialCharts(filteredSocialServices);

        // Trámites
        filteredProcedures = [...procedures];
        populateProceduresTable(filteredProcedures);
        updateProcedureCards(filteredProcedures);
        renderAllTramitesCharts(filteredProcedures);

        // Bitácoras — Psicología
        populatePsychologyTable(psychologyLogs);
        updatePsychologyCards(psychologyLogs);

        // Bitácoras — Enfermería
        populateMedicalTable(medicalLogs);
        updateMedicalCards(medicalLogs);

        // ── Listeners Calificaciones ──
        $('#filterName').on('input', applyStudentFilters);
        $('#filterStatus,#filterGenero,#filterSemestre').on('change', applyStudentFilters);
        $('#applyStudentFilters').on('click', applyStudentFilters);
        $('#resetFilters').on('click', resetStudentFilters);

        // ── Listeners Servicios Sociales ──
        $('#filterSocialName,#filterSocialTeacher,#filterSocialGroup').on('input', applySocialFilters);
        $('#filterSocialStatus').on('change', applySocialFilters);
        $('#applySocialFilters').on('click', applySocialFilters);
        $('#resetSocialFilters').on('click', resetSocialFilters);
        $('#firstPageSocial').on('click', () => { currentPageSocial = 1; populateSocialServicesTable(filteredSocialServices); });
        $('#prevPageSocial').on('click', () => { if (currentPageSocial > 1) { currentPageSocial--; populateSocialServicesTable(filteredSocialServices); } });
        $('#nextPageSocial').on('click', () => {
            const max = Math.ceil(filteredSocialServices.length / PAGE_SIZE);
            if (currentPageSocial < max) { currentPageSocial++; populateSocialServicesTable(filteredSocialServices); }
        });

        // ── Listeners Trámites ──
        $('#filterTramiteUser,#filterTramiteFolio,#filterTramiteArea').on('input', applyTramiteFilters);
        $('#filterTramiteStatus').on('change', applyTramiteFilters);
        $('#applyTramiteFilters').on('click', applyTramiteFilters);
        $('#resetTramiteFilters').on('click', resetTramiteFilters);
        $('#firstPageTramites').on('click', () => { currentPageTramites = 1; populateProceduresTable(filteredProcedures); });
        $('#prevPageTramites').on('click', () => { if (currentPageTramites > 1) { currentPageTramites--; populateProceduresTable(filteredProcedures); } });
        $('#nextPageTramites').on('click', () => {
            const max = Math.ceil(filteredProcedures.length / PAGE_SIZE);
            if (currentPageTramites < max) { currentPageTramites++; populateProceduresTable(filteredProcedures); }
        });

        // ── Exportar ──
        $('#btnExportTable').on('click', function () {
            const d = new Date().toISOString().slice(0, 10);
            if ($('#viewStudents').is(':visible'))
                exportVisibleTableToCSV('#studentsTable', `alumnos_${d}.csv`);
            else if ($('#viewSocialServices').is(':visible'))
                exportDataToCSV(filteredSocialServices,
                    ['Alumno', 'Maestro asesor', 'Grupo', 'Horas prácticas', 'Horas serv. social', 'Total horas', 'Asistencia', 'Estado', 'Última actualización'],
                    ['StudentName', 'TeacherName', 'GroupName', 'HoursPracticas', 'HoursServicioSocial', 'TotalHours', 'AttendanceRate', 'Status', 'LastUpdate'],
                    `servicios_sociales_${d}.csv`);
            else if ($('#viewProcedures').is(':visible'))
                exportDataToCSV(filteredProcedures,
                    ['Folio', 'Usuario', 'Tipo de trámite', 'Área', 'Estado', 'Fecha creación', 'Fecha actualización', 'Días transcurridos'],
                    ['Folio', 'StudentName', 'ProcedureType', 'AreaName', 'StatusName', 'DateCreated', 'DateUpdated', 'DaysElapsed'],
                    `tramites_${d}.csv`);
            else if ($('#subViewPsicologia').is(':visible') && $('#viewBitacoras').is(':visible'))
                exportDataToCSV(psychologyLogs,
                    ['Folio', 'Alumno', 'Matrícula', 'Fecha cita', 'Asistencia', 'Observaciones', 'Fecha creación'],
                    ['Folio', 'StudentName', 'EnrollmentOrMatricula', 'AppointmentDate', 'AttendanceStatus', 'Observations', 'CreatedAt'],
                    `psicologia_${d}.csv`);
            else if ($('#subViewEnfermeria').is(':visible') && $('#viewBitacoras').is(':visible'))
                exportDataToCSV(medicalLogs,
                    ['Folio', 'Alumno', 'Matrícula', 'Fecha registro', 'Motivo', 'Signos vitales', 'Observaciones', 'Acción', 'Estado'],
                    ['Folio', 'StudentName', 'EnrollmentOrMatricula', 'RecordDate', 'ConsultationReason', 'VitalSigns', 'Observations', 'TreatmentAction', 'Status'],
                    `enfermeria_${d}.csv`);
        });
    });
})(); // ── fin IIFE ──


// ════════════════════════════════════════════════════════════════════════════
// FILTROS TIPO EXCEL — aplica a las 3 tablas según data-table en el <th>
// ════════════════════════════════════════════════════════════════════════════
let studentFilters = {};
let socialFilters = {};
let tramiteFilters = {};

function getFiltersFor(tableId) {
    if (tableId === 'studentsTable') return studentFilters;
    if (tableId === 'socialServiceTable') return socialFilters;
    if (tableId === 'proceduresTable') return tramiteFilters;
    return {};
}
function setFiltersFor(tableId, obj) {
    if (tableId === 'studentsTable') { studentFilters = obj; return; }
    if (tableId === 'socialServiceTable') { socialFilters = obj; return; }
    if (tableId === 'proceduresTable') { tramiteFilters = obj; return; }
}

// Paginación Calificaciones
let currentPage = 1;
const ITEMS_PER_PAGE = 15;

function updatePagination() {
    const allRows = $('#studentsTable tbody tr');
    const visibleRows = allRows.filter(':visible');
    const total = visibleRows.length;
    const maxPage = Math.max(1, Math.ceil(total / ITEMS_PER_PAGE));
    if (currentPage > maxPage) currentPage = maxPage;
    const start = (currentPage - 1) * ITEMS_PER_PAGE;
    const end = start + ITEMS_PER_PAGE;
    allRows.hide();
    visibleRows.each(function (i) { if (i >= start && i < end) $(this).show(); });
    const from = total > 0 ? start + 1 : 0;
    $('#pageInfo').text(`Mostrando ${from}-${Math.min(end, total)} de ${total}`);
}

$(document).on('click', '.excel-header', function (e) {
    e.stopPropagation();
    $('.excel-filter-popup').remove();

    const col = $(this).data('col');
    const tableId = $(this).data('table');
    const $table = $('#' + tableId);
    const offset = $(this).offset();
    const curFilters = getFiltersFor(tableId);

    // Recolectar valores únicos de la columna
    const values = new Set();
    $table.find('tbody tr').each(function () {
        const txt = $(this).find('td').eq(col).text().trim();
        if (txt) values.add(txt);
    });

    const popup = $(`
        <div class="excel-filter-popup">
            <div style="display:flex;gap:6px;margin-bottom:8px;">
                <button class="sort-asc btn-ef">A → Z</button>
                <button class="sort-desc btn-ef">Z → A</button>
            </div>
            <input type="text" class="search-filter" placeholder="Buscar valor..."
                style="width:100%;padding:6px;border-radius:4px;border:1px solid rgba(255,255,255,0.12);
                       background:#2a2a2a;color:#fff;margin-bottom:4px;box-sizing:border-box;">
            <div class="filter-values"></div>
            <div style="display:flex;justify-content:flex-end;gap:6px;margin-top:8px;">
                <button class="select-all btn-ef" style="font-size:11px;">Todo</button>
                <button class="deselect-all btn-ef" style="font-size:11px;">Ninguno</button>
                <button class="apply-filter btn-ef"
                    style="background:#0d6efd;border-color:#0d6efd;padding:4px 14px;">OK</button>
            </div>
        </div>
    `);

    popup.css({
        top: Math.min(offset.top + $(this).outerHeight() + 4 - $(window).scrollTop(), window.innerHeight - 320),
        left: Math.min(offset.left, window.innerWidth - 260)
    });

    const container = popup.find('.filter-values');
    const active = curFilters[col] || null;

    values.forEach(v => {
        const checked = (!active || active.includes(v)) ? 'checked' : '';
        container.append(`<label><input type="checkbox" value="${v.replace(/"/g, '&quot;')}" ${checked}> ${v}</label>`);
    });

    $('body').append(popup);
    popup.on('click', ev => ev.stopPropagation());

    popup.find('.sort-asc').click(() => { sortTableGeneric(tableId, col, true); popup.remove(); });
    popup.find('.sort-desc').click(() => { sortTableGeneric(tableId, col, false); popup.remove(); });
    popup.find('.select-all').click(() => { container.find('input[type=checkbox]').prop('checked', true); });
    popup.find('.deselect-all').click(() => { container.find('input[type=checkbox]').prop('checked', false); });

    popup.find('.apply-filter').click(function () {
        const selected = [];
        popup.find('input[type=checkbox]:checked').each(function () { selected.push($(this).val()); });
        const f = getFiltersFor(tableId);
        f[col] = selected;
        setFiltersFor(tableId, f);
        applyExcelFiltersFor(tableId);
        popup.remove();
    });

    popup.find('.search-filter').on('input', function () {
        const txt = $(this).val().toLowerCase();
        container.find('label').each(function () {
            $(this).toggle($(this).text().toLowerCase().includes(txt));
        });
    });
});

function applyExcelFiltersFor(tableId) {
    const f = getFiltersFor(tableId);
    $('#' + tableId + ' tbody tr').each(function () {
        let show = true;
        for (const col in f) {
            const val = $(this).find('td').eq(col).text().trim();
            if (!f[col].includes(val)) { show = false; break; }
        }
        $(this).toggle(show);
    });
    // Solo calificaciones maneja paginación DOM
    if (tableId === 'studentsTable') {
        currentPage = 1;
        updatePagination();
    }
}

function sortTableGeneric(tableId, col, asc) {
    const rows = $('#' + tableId + ' tbody tr').get();
    rows.sort(function (a, b) {
        const A = $(a).find('td').eq(col).text().trim();
        const B = $(b).find('td').eq(col).text().trim();
        const nA = parseFloat(A), nB = parseFloat(B);
        if (!isNaN(nA) && !isNaN(nB)) return asc ? nA - nB : nB - nA;
        return asc ? A.localeCompare(B, 'es') : B.localeCompare(A, 'es');
    });
    const tbody = $('#' + tableId + ' tbody');
    $.each(rows, function (_, row) { tbody.append(row); });
    if (tableId === 'studentsTable') { currentPage = 1; updatePagination(); }
}

$(document).on('click', function (e) {
    if (!$(e.target).closest('.excel-filter-popup').length &&
        !$(e.target).closest('.excel-header').length) {
        $('.excel-filter-popup').remove();
    }
});

// Paginación Calificaciones
$(document).ready(function () {
    updatePagination();
    $('#firstPage').on('click', () => { currentPage = 1; updatePagination(); });
    $('#prevPage').on('click', () => { if (currentPage > 1) { currentPage--; updatePagination(); } });
    $('#nextPage').on('click', () => {
        const total = $('#studentsTable tbody tr:visible').length;
        const maxPage = Math.ceil(total / ITEMS_PER_PAGE);
        if (currentPage < maxPage) { currentPage++; updatePagination(); }
    });
});
