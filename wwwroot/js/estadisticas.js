// estadisticas.js
(function () {
    // ── Datos ────────────────────────────────────────────────────────────────
    const raw = window.__estadisticasData || {};
    const students = (typeof raw.students === 'string') ? JSON.parse(raw.students) : (raw.students || []);
    const socialServices = (typeof raw.socialServices === 'string') ? JSON.parse(raw.socialServices) : (raw.socialServices || []);
    const procedures = (typeof raw.procedures === 'string') ? JSON.parse(raw.procedures) : (raw.procedures || []);
    const psychologyLogs = (typeof raw.psychologyLogs === 'string') ? JSON.parse(raw.psychologyLogs) : (raw.psychologyLogs || []);
    const medicalLogs = (typeof raw.medicalLogs === 'string') ? JSON.parse(raw.medicalLogs) : (raw.medicalLogs || []);

    let filteredSocial = [...socialServices];
    let filteredProcs = [...procedures];
    let filteredPsy = [...psychologyLogs];
    let filteredMed = [...medicalLogs];
    let filteredStudents = [...students];
    let filteredGroups = [...students];

    const PAGE_SIZE = 15;
    let pageSocial = 1, pageProcs = 1, pagePsy = 1, pageMed = 1, pageStudents = 1, pageGroups = 1;

    let charts = {};

    // ── Sin animaciones ───────────────────────────────────────────────────────
    function noAnim(opts) {
        opts = opts || {};
        opts.animation = false;
        opts.animations = false;
        if (!opts.transitions) opts.transitions = {};
        if (!opts.transitions.active) opts.transitions.active = {};
        opts.transitions.active.animation = { duration: 0 };
        return opts;
    }

    // ── Plugin etiquetas ─────────────────────────────────────────────────────
    const valueLabelPlugin = {
        id: 'valueLabelPlugin',
        afterDatasetsDraw(chart, _args, opts) {
            const ctx = chart.ctx; ctx.save();
            const font = (opts && opts.font) || '12px Arial';
            const textColor = (opts && opts.color) || '#fff';
            const bg = (opts && opts.bgColor) || 'rgba(40,40,40,0.88)';
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
                        x = elem.x + Math.cos(mid) * r; y = elem.y + Math.sin(mid) * r;
                    } else { x = elem.x; y = (elem.y || 0) - 10; }

                    ctx.font = font;
                    const tw = ctx.measureText(label).width;
                    const th = parseInt(font) || 12;
                    const bw = tw + pad * 2, bh = th + pad * 2;
                    const bx = x - bw / 2, by = y - bh / 2, r2 = Math.min(5, bh / 2);
                    ctx.fillStyle = bg;
                    ctx.beginPath();
                    ctx.moveTo(bx + r2, by); ctx.lineTo(bx + bw - r2, by);
                    ctx.quadraticCurveTo(bx + bw, by, bx + bw, by + r2);
                    ctx.lineTo(bx + bw, by + bh - r2);
                    ctx.quadraticCurveTo(bx + bw, by + bh, bx + bw - r2, by + bh);
                    ctx.lineTo(bx + r2, by + bh);
                    ctx.quadraticCurveTo(bx, by + bh, bx, by + bh - r2);
                    ctx.lineTo(bx, by + r2); ctx.quadraticCurveTo(bx, by, bx + r2, by);
                    ctx.closePath(); ctx.fill();
                    ctx.fillStyle = textColor; ctx.textAlign = 'center'; ctx.textBaseline = 'middle';
                    ctx.fillText(label, x, y);
                });
            });
            ctx.restore();
        }
    };
    if (window.Chart && typeof Chart.register === 'function') {
        try { Chart.register(valueLabelPlugin); } catch (_) { }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    function makeChart(id, config) {
        const el = document.getElementById(id); if (!el) return null;
        if (charts[id]) { charts[id].destroy(); }
        config.options = noAnim(config.options);
        charts[id] = new Chart(el.getContext('2d'), config);
        return charts[id];
    }

    function emptyChart(id) {
        makeChart(id, {
            type: 'bar',
            data: { labels: ['Sin datos'], datasets: [{ label: 'Sin datos', data: [1], backgroundColor: '#e9ecef' }] },
            options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } } }
        });
    }

    function vlOpts(color, bg) {
        return { valueLabelPlugin: { color: color || '#fff', font: '12px Arial', bgColor: bg || 'rgba(40,40,40,0.88)', formatter: v => v } };
    }

    function baseBarOpts(extra) {
        const base = {
            responsive: true,
            maintainAspectRatio: false,
            scales: { y: { beginAtZero: true, ticks: { precision: 0 } } },
            plugins: vlOpts('#fff')
        };
        if (!extra) return base;
        if (extra.scales) base.scales = Object.assign(base.scales, extra.scales);
        if (extra.plugins) base.plugins = Object.assign(base.plugins, extra.plugins);
        return base;
    }

    function escHtml(s) {
        return String(s || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
    }
    function csvEsc(t) {
        if (t == null) return '';
        let o = String(t).replace(/\"/g, '""');
        return /[",\n\r]/.test(o) ? '"' + o + '"' : o;
    }
    function paginate(list, page, size) {
        const start = (page - 1) * size;
        return { items: list.slice(start, start + size), from: start + 1, to: Math.min(start + size, list.length), total: list.length };
    }
    function pagText(from, to, total) {
        return total > 0 ? `Mostrando ${from}-${to} de ${total}` : 'Sin resultados';
    }

    // ── Helper: detectar vista/sub-vista activa ──────────────────────────────
    // FIX: centraliza la detección de qué sección está visible para
    //      que tanto Excel como PDF usen la misma lógica.
    function getActiveSection() {
        const viewStudents = document.getElementById('viewStudents');
        const viewSocialServices = document.getElementById('viewSocialServices');
        const viewProcedures = document.getElementById('viewProcedures');
        const viewBitacoras = document.getElementById('viewBitacoras');
        const subViewSemestre = document.getElementById('subViewSemestre');
        const subViewPsicologia = document.getElementById('subViewPsicologia');

        if (viewStudents && !viewStudents.classList.contains('d-none')) {
            // Detectar sub-vista activa dentro de Calificaciones
            if (subViewSemestre && !subViewSemestre.classList.contains('d-none')) {
                return 'calificaciones_semestre';
            }
            return 'calificaciones_grupo';
        }
        if (viewSocialServices && !viewSocialServices.classList.contains('d-none')) return 'social';
        if (viewProcedures && !viewProcedures.classList.contains('d-none')) return 'tramites';
        if (viewBitacoras && !viewBitacoras.classList.contains('d-none')) {
            if (subViewPsicologia && !subViewPsicologia.classList.contains('d-none')) return 'psicologia';
            return 'enfermeria';
        }
        return null;
    }

    // ════════════════════════════════════════════════════════════════════════
    // CALIFICACIONES
    // ════════════════════════════════════════════════════════════════════════
    function computeStudentStats(list) {
        const s = { Total: list.length, Inscrito: 0, Cursando: 0, Aprobado: 0, Reprobado: 0 };
        list.forEach(r => { s[r.Estado] = (s[r.Estado] || 0) + 1; });
        return s;
    }
    function updateStudentCards(list) {
        const s = computeStudentStats(list);
        $('#statTotal').text(s.Total); $('#statInscritos').text(s.Inscrito || 0);
        $('#statCursando').text(s.Cursando || 0); $('#statAprobados').text(s.Aprobado || 0);
    }
    function renderStudentPie(list) {
        const map = { Inscrito: '#6c757d', Cursando: '#ffc107', Aprobado: '#198754', Reprobado: '#dc3545' };
        const st = computeStudentStats(list);
        const labels = [], data = [], colors = [];
        Object.entries(map).forEach(([k, c]) => { if ((st[k] || 0) > 0) { labels.push(k); data.push(st[k]); colors.push(c); } });
        if (!data.length) { emptyChart('pieChart'); return; }
        makeChart('pieChart', {
            type: 'pie',
            data: { labels, datasets: [{ data, backgroundColor: colors }] },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, plugins: vlOpts('#fff') }
        });
    }
    function renderStudentBar(list) {
        const bc = {};
        list.forEach(s => {
            if (!bc[s.Curso]) bc[s.Curso] = { sum: 0, count: 0 };
            if (s.Nota > 0) { bc[s.Curso].sum += s.Nota; bc[s.Curso].count++; }
        });
        const labels = Object.keys(bc).filter(l => bc[l].count > 0);
        if (!labels.length) { emptyChart('barChart'); return; }
        makeChart('barChart', {
            type: 'bar',
            data: { labels, datasets: [{ label: 'Promedio nota', data: labels.map(l => +(bc[l].sum / bc[l].count).toFixed(2)), backgroundColor: '#0d6efd' }] },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true, max: 10 } }, plugins: vlOpts('#fff') }
        });
    }
    function renderGradeHistogram(list) {
        const b = [0, 0, 0, 0, 0];
        list.forEach(s => { const n = Number(s.Nota); if (isNaN(n)) return; if (n < 2) b[0]++; else if (n < 4) b[1]++; else if (n < 6) b[2]++; else if (n < 8) b[3]++; else b[4]++; });
        const al = ['0-2', '2-4', '4-6', '6-8', '8-10'], fl = [], fb = [];
        b.forEach((v, i) => { if (v > 0) { fl.push(al[i]); fb.push(v); } });
        if (!fb.length) { emptyChart('gradeHistogramChart'); return; }
        makeChart('gradeHistogramChart', {
            type: 'bar',
            data: { labels: fl, datasets: [{ label: 'Alumnos', data: fb, backgroundColor: '#6f42c1' }] },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }, plugins: vlOpts('#fff') }
        });
    }
    function renderCourseStatusStacked(list) {
        const bc = {};
        list.forEach(s => { const c = s.Curso || 'Sin curso'; if (!bc[c]) bc[c] = { Inscrito: 0, Cursando: 0, Aprobado: 0, Reprobado: 0 }; bc[c][s.Estado] = (bc[c][s.Estado] || 0) + 1; });
        const labels = Object.keys(bc).filter(c => (bc[c].Inscrito || 0) + (bc[c].Cursando || 0) + (bc[c].Aprobado || 0) + (bc[c].Reprobado || 0) > 0);
        if (!labels.length) { emptyChart('courseStatusStacked'); return; }
        makeChart('courseStatusStacked', {
            type: 'bar',
            data: {
                labels, datasets: [
                    { label: 'Inscrito', data: labels.map(l => bc[l].Inscrito || 0), backgroundColor: '#6c757d' },
                    { label: 'Cursando', data: labels.map(l => bc[l].Cursando || 0), backgroundColor: '#ffc107' },
                    { label: 'Aprobado', data: labels.map(l => bc[l].Aprobado || 0), backgroundColor: '#198754' },
                    { label: 'Reprobado', data: labels.map(l => bc[l].Reprobado || 0), backgroundColor: '#dc3545' }
                ]
            },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, scales: { x: { stacked: true }, y: { stacked: true, beginAtZero: true } }, plugins: { legend: { position: 'top' }, ...vlOpts('#fff') } }
        });
    }
    function renderAllStudentCharts(list) { renderStudentPie(list); renderStudentBar(list); renderGradeHistogram(list); renderCourseStatusStacked(list); }

    function renderGroupPie(list) {
        const map = { Inscrito: '#6c757d', Cursando: '#ffc107', Aprobado: '#198754', Reprobado: '#dc3545' };
        const st = computeStudentStats(list);
        const labels = [], data = [], colors = [];
        Object.entries(map).forEach(([k, c]) => { if ((st[k] || 0) > 0) { labels.push(k); data.push(st[k]); colors.push(c); } });
        if (!data.length) { emptyChart('pieChartGrupo'); return; }
        makeChart('pieChartGrupo', { type: 'pie', data: { labels, datasets: [{ data, backgroundColor: colors }] }, plugins: [valueLabelPlugin], options: { responsive: true, maintainAspectRatio: false, plugins: vlOpts('#fff') } });
    }
    function renderGroupBar(list) {
        const bc = {};
        list.forEach(s => { if (!bc[s.Curso]) bc[s.Curso] = { sum: 0, count: 0 }; if (s.Nota > 0) { bc[s.Curso].sum += s.Nota; bc[s.Curso].count++; } });
        const labels = Object.keys(bc).filter(l => bc[l].count > 0);
        if (!labels.length) { emptyChart('barChartGrupo'); return; }
        makeChart('barChartGrupo', { type: 'bar', data: { labels, datasets: [{ label: 'Promedio nota', data: labels.map(l => +(bc[l].sum / bc[l].count).toFixed(2)), backgroundColor: '#0d6efd' }] }, plugins: [valueLabelPlugin], options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true, max: 10 } }, plugins: vlOpts('#fff') } });
    }
    function renderGroupHistogram(list) {
        const b = [0, 0, 0, 0, 0];
        list.forEach(s => { const n = Number(s.Nota); if (isNaN(n)) return; if (n < 2) b[0]++; else if (n < 4) b[1]++; else if (n < 6) b[2]++; else if (n < 8) b[3]++; else b[4]++; });
        const al = ['0-2', '2-4', '4-6', '6-8', '8-10'], fl = [], fb = [];
        b.forEach((v, i) => { if (v > 0) { fl.push(al[i]); fb.push(v); } });
        if (!fb.length) { emptyChart('gradeHistogramChartGrupo'); return; }
        makeChart('gradeHistogramChartGrupo', { type: 'bar', data: { labels: fl, datasets: [{ label: 'Alumnos', data: fb, backgroundColor: '#6f42c1' }] }, plugins: [valueLabelPlugin], options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }, plugins: vlOpts('#fff') } });
    }
    function renderGroupStatusStacked(list) {
        const bc = {};
        list.forEach(s => { const g = s.Grupo || 'Sin grupo'; if (!bc[g]) bc[g] = { Inscrito: 0, Cursando: 0, Aprobado: 0, Reprobado: 0 }; bc[g][s.Estado] = (bc[g][s.Estado] || 0) + 1; });
        const labels = Object.keys(bc).filter(g => (bc[g].Inscrito || 0) + (bc[g].Cursando || 0) + (bc[g].Aprobado || 0) + (bc[g].Reprobado || 0) > 0);
        if (!labels.length) { emptyChart('groupStatusStacked'); return; }
        makeChart('groupStatusStacked', { type: 'bar', data: { labels, datasets: [{ label: 'Inscrito', data: labels.map(l => bc[l].Inscrito || 0), backgroundColor: '#6c757d' }, { label: 'Cursando', data: labels.map(l => bc[l].Cursando || 0), backgroundColor: '#ffc107' }, { label: 'Aprobado', data: labels.map(l => bc[l].Aprobado || 0), backgroundColor: '#198754' }, { label: 'Reprobado', data: labels.map(l => bc[l].Reprobado || 0), backgroundColor: '#dc3545' }] }, plugins: [valueLabelPlugin], options: { responsive: true, maintainAspectRatio: false, scales: { x: { stacked: true }, y: { stacked: true, beginAtZero: true } }, plugins: { legend: { position: 'top' }, ...vlOpts('#fff') } } });
    }
    function renderAllGroupCharts(list) { renderGroupPie(list); renderGroupBar(list); renderGroupHistogram(list); renderGroupStatusStacked(list); }

    function applyStudentFilters() {
        const name = ($('#filterName').val() || '').toLowerCase();
        const status = $('#filterStatus').val() || '';
        const genero = $('#filterGenero').val() || '';
        const semestre = $('#filterSemestre').val() || '';
        filteredStudents = students.filter(s =>
            (!name || (s.Nombre || '').toLowerCase().includes(name))
            && (!status || s.Estado === status)
            && (!genero || s.Genero === genero)
            && (!semestre || String(s.Semestre) === semestre)
        );
        studentExcelFilters = {}; $('.excel-filter-popup').remove();
        pageStudents = 1;
        populateStudentsTable(filteredStudents);
        updateStudentCards(filteredStudents); renderAllStudentCharts(filteredStudents);
    }
    function resetStudentFilters() {
        $('#filterName').val(''); $('#filterStatus').val(''); $('#filterGenero').val(''); $('#filterSemestre').val('');
        studentExcelFilters = {}; $('.excel-filter-popup').remove();
        filteredStudents = [...students];
        pageStudents = 1;
        populateStudentsTable(filteredStudents);
        updateStudentCards(filteredStudents); renderAllStudentCharts(filteredStudents);
    }
    function applyGroupFilters() {
        const name = ($('#filterGrupoName').val() || '').toLowerCase();
        const status = $('#filterGrupoStatus').val() || '';
        const genero = ($('#filterGrupoGenero').val() || '');
        const grupo = ($('#filterGrupoGrupo').val() || '').toLowerCase();
        filteredGroups = students.filter(s =>
            (!name || (s.Nombre || '').toLowerCase().includes(name))
            && (!status || s.Estado === status)
            && (!genero || s.Genero === genero)
            && (!grupo || (s.Grupo || '').toLowerCase().includes(grupo))
        );
        pageGroups = 1;
        populateGroupTable(filteredGroups);
        updateStudentCards(filteredGroups); renderAllGroupCharts(filteredGroups);
    }
    function resetGroupFilters() {
        $('#filterGrupoName').val(''); $('#filterGrupoStatus').val(''); $('#filterGrupoGenero').val(''); $('#filterGrupoGrupo').val('');
        filteredGroups = [...students];
        pageGroups = 1;
        populateGroupTable(filteredGroups);
        updateStudentCards(filteredGroups); renderAllGroupCharts(filteredGroups);
    }
    function populateStudentsTable(list) {
        const tbody = $('#studentsTable tbody'); if (!tbody.length) return;
        tbody.empty();
        if (!list || !list.length) { tbody.html('<tr><td colspan="8" class="text-center text-muted py-4">Sin datos disponibles</td></tr>'); $('#pageInfo').text('Sin resultados'); return; }
        const { items, from, to, total } = paginate(list, pageStudents, PAGE_SIZE);
        items.forEach(s => {
            const badge = s.Estado === 'Aprobado' ? '<span class="badge bg-success">Aprobado</span>'
                : s.Estado === 'Reprobado' ? '<span class="badge bg-danger">Reprobado</span>'
                    : s.Estado === 'Cursando' ? '<span class="badge bg-warning text-dark">Cursando</span>'
                        : '<span class="badge bg-secondary">Inscrito</span>';
            const fecha = s.FechaInscripcion ? new Date(s.FechaInscripcion).toISOString().slice(0, 10) : '';
            tbody.append(`<tr data-name="${escHtml(s.Nombre)}" data-status="${escHtml(s.Estado)}" data-genero="${escHtml(s.Genero)}" data-semestre="${s.Semestre}"><td>${s.Id}</td><td>${escHtml(s.Nombre)}</td><td>${escHtml(s.Genero)}</td><td>${escHtml(s.Curso)}</td><td>${s.Semestre}</td><td>${badge}</td><td>${s.Nota}</td><td>${fecha}</td></tr>`);
        });
        $('#pageInfo').text(pagText(from, to, total));
    }
    function populateGroupTable(list) {
        const tbody = $('#grupoTable tbody'); if (!tbody.length) return;
        tbody.empty();
        if (!list || !list.length) { tbody.html('<tr><td colspan="8" class="text-center text-muted py-4">Sin datos disponibles</td></tr>'); $('#pageInfoGrupo').text('Sin resultados'); return; }
        const { items, from, to, total } = paginate(list, pageGroups, PAGE_SIZE);
        items.forEach(s => {
            const badge = s.Estado === 'Aprobado' ? '<span class="badge bg-success">Aprobado</span>'
                : s.Estado === 'Reprobado' ? '<span class="badge bg-danger">Reprobado</span>'
                    : s.Estado === 'Cursando' ? '<span class="badge bg-warning text-dark">Cursando</span>'
                        : '<span class="badge bg-secondary">Inscrito</span>';
            const fecha = s.FechaInscripcion ? new Date(s.FechaInscripcion).toISOString().slice(0, 10) : '';
            tbody.append(`<tr data-name="${escHtml(s.Nombre)}" data-status="${escHtml(s.Estado)}" data-genero="${escHtml(s.Genero)}" data-grupo="${escHtml(s.Grupo || '')}"><td>${s.Id}</td><td>${escHtml(s.Nombre)}</td><td>${escHtml(s.Genero)}</td><td>${escHtml(s.Curso)}</td><td>${escHtml(s.Grupo || 'Sin grupo')}</td><td>${badge}</td><td>${s.Nota}</td><td>${fecha}</td></tr>`);
        });
        $('#pageInfoGrupo').text(pagText(from, to, total));
    }

    // ════════════════════════════════════════════════════════════════════════
    // SERVICIOS SOCIALES
    // ════════════════════════════════════════════════════════════════════════
    function getStatusBadge(s) {
        const m = { 'Completado': '<span class="badge bg-success">Completado</span>', 'En progreso': '<span class="badge bg-warning text-dark">En progreso</span>', 'Pendiente': '<span class="badge bg-secondary">Pendiente</span>' };
        return m[s] || `<span class="badge bg-secondary">${escHtml(s)}</span>`;
    }
    function populateSocialTable(list) {
        const tbody = $('#socialServiceTable tbody'); if (!tbody.length) return;
        tbody.empty();
        if (!list || !list.length) { tbody.html('<tr><td colspan="9" class="text-center text-muted py-4">Sin datos disponibles</td></tr>'); $('#pageInfoSocial').text('Sin resultados'); return; }
        const { items, from, to, total } = paginate(list, pageSocial, PAGE_SIZE);
        items.forEach(item => {
            const la = item.LastAttendanceDate ? new Date(item.LastAttendanceDate).toLocaleDateString('es-MX') : 'Sin registro';
            tbody.append(`<tr><td>${escHtml(item.StudentName || 'Sin nombre')}</td><td>${escHtml(item.TeacherName || 'Sin asignar')}</td><td>${escHtml(item.GroupName || 'Sin grupo')}</td><td>${item.TotalPresent || 0}</td><td>${item.TotalAbsent || 0}</td><td>${item.TotalJustified || 0}</td><td>${(item.AttendanceRate || 0).toFixed(1)}%</td><td>${getStatusBadge(item.Status)}</td><td>${la}</td></tr>`);
        });
        $('#pageInfoSocial').text(pagText(from, to, total));
    }
    function updateSocialCards(list) {
        if (!list || !list.length) { $('#statSocialTotal,#statSocialHours,#statSocialPending').text('0'); $('#statSocialAttendance').text('0%'); return; }
        const wa = list.filter(i => (i.TotalAttendances || 0) > 0);
        $('#statSocialTotal').text(wa.length);
        $('#statSocialHours').text(list.reduce((s, i) => s + (i.TotalPresent || 0), 0));
        $('#statSocialPending').text(list.reduce((s, i) => s + (i.TotalAbsent || 0) + (i.TotalJustified || 0), 0));
        $('#statSocialAttendance').text((wa.length > 0 ? (wa.reduce((s, i) => s + (i.AttendanceRate || 0), 0) / wa.length).toFixed(1) : '0.0') + '%');
    }
    function renderSocialPie(list) {
        const cnt = { 'Completado': 0, 'En progreso': 0, 'Pendiente': 0 };
        list.forEach(i => { if (cnt[i.Status] !== undefined) cnt[i.Status]++; else cnt['Pendiente']++; });
        const labels = Object.keys(cnt).filter(k => cnt[k] > 0);
        if (!labels.length) { emptyChart('socialPieChart'); return; }
        makeChart('socialPieChart', {
            type: 'doughnut',
            data: { labels, datasets: [{ data: labels.map(l => cnt[l]), backgroundColor: ['#198754', '#ffc107', '#6c757d'] }] },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' }, ...vlOpts('#fff') } }
        });
    }
    function renderSocialHoursBar(list) {
        if (!list.length) { emptyChart('socialHoursBar'); return; }
        const sorted = [...list].sort((a, b) => (b.TotalPresent || 0) - (a.TotalPresent || 0)).slice(0, 10);
        makeChart('socialHoursBar', {
            type: 'bar',
            data: { labels: sorted.map(i => (i.StudentName || '?').split(' ').slice(0, 2).join(' ')), datasets: [{ label: 'Asistencias', data: sorted.map(i => i.TotalPresent || 0), backgroundColor: '#198754' }] },
            plugins: [valueLabelPlugin],
            options: { indexAxis: 'y', responsive: true, maintainAspectRatio: false, scales: { x: { beginAtZero: true, ticks: { precision: 0 } } }, plugins: { legend: { display: false }, ...vlOpts('#fff') } }
        });
    }
    function renderSocialAttendance(list) {
        if (!list.length) { emptyChart('socialAttendanceBar'); return; }
        const byG = {};
        list.forEach(i => { const g = i.GroupName || 'Sin grupo'; if (!byG[g]) byG[g] = { sum: 0, count: 0 }; byG[g].sum += (i.AttendanceRate || 0); byG[g].count++; });
        const labels = Object.keys(byG);
        makeChart('socialAttendanceBar', {
            type: 'bar',
            data: { labels, datasets: [{ label: 'Asistencia %', data: labels.map(l => +(byG[l].sum / byG[l].count).toFixed(1)), backgroundColor: '#20c997' }] },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true, max: 100, ticks: { callback: v => v + '%' } } }, plugins: { legend: { display: false }, ...vlOpts('#fff') } }
        });
    }
    function renderSocialStacked(list) {
        if (!list.length) { emptyChart('socialHoursStacked'); return; }
        const byG = {};
        list.forEach(i => { const g = i.GroupName || 'Sin grupo'; if (!byG[g]) byG[g] = { p: 0, a: 0, j: 0 }; byG[g].p += (i.TotalPresent || 0); byG[g].a += (i.TotalAbsent || 0); byG[g].j += (i.TotalJustified || 0); });
        const labels = Object.keys(byG);
        makeChart('socialHoursStacked', {
            type: 'bar',
            data: {
                labels, datasets: [
                    { label: 'Presentes', data: labels.map(l => byG[l].p), backgroundColor: '#198754' },
                    { label: 'Ausencias', data: labels.map(l => byG[l].a), backgroundColor: '#dc3545' },
                    { label: 'Justificadas', data: labels.map(l => byG[l].j), backgroundColor: '#ffc107' }
                ]
            },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, scales: { x: { stacked: true }, y: { stacked: true, beginAtZero: true, ticks: { precision: 0 } } }, plugins: { legend: { position: 'top' }, ...vlOpts('#fff') } }
        });
    }
    function renderAllSocialCharts(list) { renderSocialPie(list); renderSocialHoursBar(list); renderSocialAttendance(list); renderSocialStacked(list); }

    function applySocialFilters() {
        const name = ($('#filterSocialName').val() || '').toLowerCase().trim();
        const teacher = ($('#filterSocialTeacher').val() || '').toLowerCase().trim();
        const status = $('#filterSocialStatus').val() || '';
        const group = ($('#filterSocialGroup').val() || '').toLowerCase().trim();
        filteredSocial = socialServices.filter(i =>
            (!name || (i.StudentName || '').toLowerCase().includes(name)) &&
            (!teacher || (i.TeacherName || '').toLowerCase().includes(teacher)) &&
            (!status || (i.Status || '') === status) &&
            (!group || (i.GroupName || '').toLowerCase().includes(group))
        );
        pageSocial = 1; populateSocialTable(filteredSocial); updateSocialCards(filteredSocial); renderAllSocialCharts(filteredSocial);
    }
    function resetSocialFilters() {
        $('#filterSocialName,#filterSocialTeacher,#filterSocialGroup').val(''); $('#filterSocialStatus').val('');
        socialExcelFilters = {}; filteredSocial = [...socialServices];
        pageSocial = 1; populateSocialTable(filteredSocial); updateSocialCards(filteredSocial); renderAllSocialCharts(filteredSocial);
    }

    // ════════════════════════════════════════════════════════════════════════
    // TRÁMITES
    // ════════════════════════════════════════════════════════════════════════
    function getProcBadge(code, name) {
        const m = { 'APPROVED': '<span class="badge bg-success">Pagó inscripción</span>', 'PENDING': '<span class="badge bg-warning text-dark">Pendiente</span>', 'REJECTED': '<span class="badge bg-danger">No pagó</span>' };
        return m[code] || `<span class="badge bg-secondary">${escHtml(name || 'Desconocido')}</span>`;
    }
    function populateProcsTable(list) {
        const tbody = $('#proceduresTable tbody'); if (!tbody.length) return;
        tbody.empty();
        if (!list || !list.length) { tbody.html('<tr><td colspan="8" class="text-center text-muted py-4">Sin datos disponibles</td></tr>'); $('#pageInfoTramites').text('Sin resultados'); return; }
        const { items, from, to, total } = paginate(list, pageProcs, PAGE_SIZE);
        items.forEach(item => {
            const dc = item.DateCreated ? new Date(item.DateCreated).toLocaleDateString('es-MX') : 'N/A';
            const du = item.DateUpdated ? new Date(item.DateUpdated).toLocaleDateString('es-MX') : 'N/A';
            tbody.append(`<tr><td>${escHtml(item.Folio || 'Sin folio')}</td><td>${escHtml(item.StudentName || 'Sin nombre')}</td><td>${escHtml(item.ProcedureType || 'Sin tipo')}</td><td>${escHtml(item.AreaName || 'Sin área')}</td><td>${getProcBadge(item.InternalCode, item.StatusName)}</td><td>${dc}</td><td>${du}</td><td>${item.DaysElapsed || 0} días</td></tr>`);
        });
        $('#pageInfoTramites').text(pagText(from, to, total));
    }
    function updateProcCards(list) {
        if (!list || !list.length) { $('#statProcsTotal,#statProcsAction,#statProcsInProgress,#statProcsFinalized').text('0'); return; }
        $('#statProcsTotal').text(list.length);
        $('#statProcsAction').text(list.filter(i => i.InternalCode === 'PENDING').length);
        $('#statProcsInProgress').text(list.filter(i => i.InternalCode !== 'APPROVED' && i.InternalCode !== 'REJECTED' && i.InternalCode !== 'PENDING').length);
        $('#statProcsFinalized').text(list.filter(i => i.InternalCode === 'APPROVED').length);
    }
    function renderTramitesPie(list) {
        const cnt = { APPROVED: 0, PENDING: 0, REJECTED: 0, OTRO: 0 };
        const ln = { APPROVED: 'Pagó inscripción', PENDING: 'Pendiente', REJECTED: 'No pagó', OTRO: 'Otro' };
        const lc = { APPROVED: '#198754', PENDING: '#ffc107', REJECTED: '#dc3545', OTRO: '#6c757d' };
        list.forEach(i => { const k = cnt[i.InternalCode] !== undefined ? i.InternalCode : 'OTRO'; cnt[k]++; });
        const keys = Object.keys(cnt).filter(k => cnt[k] > 0);
        if (!keys.length) { emptyChart('tramitesPieChart'); return; }
        makeChart('tramitesPieChart', {
            type: 'doughnut',
            data: { labels: keys.map(k => ln[k]), datasets: [{ data: keys.map(k => cnt[k]), backgroundColor: keys.map(k => lc[k]) }] },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' }, ...vlOpts('#fff') } }
        });
    }
    function renderTramitesArea(list) {
        if (!list.length) { emptyChart('tramitesAreaBar'); return; }
        const byA = {};
        list.forEach(i => { const a = i.AreaName || 'Sin área'; byA[a] = (byA[a] || 0) + 1; });
        const labels = Object.keys(byA).sort((a, b) => byA[b] - byA[a]);
        makeChart('tramitesAreaBar', {
            type: 'bar',
            data: { labels, datasets: [{ label: 'Trámites', data: labels.map(l => byA[l]), backgroundColor: '#0d6efd' }] },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }, plugins: { legend: { display: false }, ...vlOpts('#fff') } }
        });
    }
    function renderTramitesTipo(list) {
        if (!list.length) { emptyChart('tramitesTipoBar'); return; }
        const byT = {};
        list.forEach(i => { const t = i.ProcedureType || 'Sin tipo'; byT[t] = (byT[t] || 0) + 1; });
        const sorted = Object.entries(byT).sort((a, b) => b[1] - a[1]).slice(0, 10);
        makeChart('tramitesTipoBar', {
            type: 'bar',
            data: { labels: sorted.map(([l]) => l), datasets: [{ label: 'Cantidad', data: sorted.map(([, v]) => v), backgroundColor: '#fd7e14' }] },
            plugins: [valueLabelPlugin],
            options: { indexAxis: 'y', responsive: true, maintainAspectRatio: false, scales: { x: { beginAtZero: true } }, plugins: { legend: { display: false }, ...vlOpts('#fff') } }
        });
    }
    function renderTramitesDias(list) {
        if (!list.length) { emptyChart('tramitesDiasBar'); return; }
        const byA = {};
        list.forEach(i => { const a = i.AreaName || 'Sin área'; if (!byA[a]) byA[a] = { sum: 0, count: 0 }; byA[a].sum += (i.DaysElapsed || 0); byA[a].count++; });
        const labels = Object.keys(byA);
        makeChart('tramitesDiasBar', {
            type: 'bar',
            data: { labels, datasets: [{ label: 'Días promedio', data: labels.map(l => +(byA[l].sum / byA[l].count).toFixed(1)), backgroundColor: '#20c997' }] },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }, plugins: { legend: { display: false }, ...vlOpts('#fff') } }
        });
    }
    function renderAllTramitesCharts(list) { renderTramitesPie(list); renderTramitesArea(list); renderTramitesTipo(list); renderTramitesDias(list); }

    function applyTramiteFilters() {
        const user = ($('#filterTramiteUser').val() || '').toLowerCase().trim();
        const folio = ($('#filterTramiteFolio').val() || '').toLowerCase().trim();
        const status = $('#filterTramiteStatus').val() || '';
        const area = ($('#filterTramiteArea').val() || '').toLowerCase().trim();
        filteredProcs = procedures.filter(i =>
            (!user || (i.StudentName || '').toLowerCase().includes(user)) &&
            (!folio || String(i.Folio || '').toLowerCase().includes(folio)) &&
            (!status || (i.InternalCode || '') === status) &&
            (!area || (i.AreaName || '').toLowerCase().includes(area))
        );
        pageProcs = 1; populateProcsTable(filteredProcs); updateProcCards(filteredProcs); renderAllTramitesCharts(filteredProcs);
    }
    function resetTramiteFilters() {
        $('#filterTramiteUser,#filterTramiteFolio,#filterTramiteArea').val(''); $('#filterTramiteStatus').val('');
        tramiteExcelFilters = {}; filteredProcs = [...procedures];
        pageProcs = 1; populateProcsTable(filteredProcs); updateProcCards(filteredProcs); renderAllTramitesCharts(filteredProcs);
    }

    // ════════════════════════════════════════════════════════════════════════
    // BITÁCORAS — PSICOLOGÍA
    // ════════════════════════════════════════════════════════════════════════
    function getPsyBadge(s) {
        const m = { 'Asistió': '<span class="badge bg-success">Asistió</span>', 'No asistió': '<span class="badge bg-danger">No asistió</span>', 'Justificado': '<span class="badge bg-warning text-dark">Justificado</span>' };
        return m[s] || `<span class="badge bg-secondary">${escHtml(s || 'Sin estado')}</span>`;
    }
    function populatePsyTable(list) {
        const tbody = $('#psychologyTable tbody'); if (!tbody.length) return;
        tbody.empty();
        if (!list || !list.length) { tbody.html('<tr><td colspan="7" class="text-center text-muted py-4">Sin datos disponibles</td></tr>'); $('#pageInfoPsy').text('Sin resultados'); return; }
        const { items, from, to, total } = paginate(list, pagePsy, PAGE_SIZE);
        items.forEach(item => {
            const ad = item.AppointmentDate ? new Date(item.AppointmentDate).toLocaleString('es-MX') : 'N/A';
            const ca = item.CreatedAt ? new Date(item.CreatedAt).toLocaleDateString('es-MX') : 'N/A';
            tbody.append(`<tr><td>${escHtml(item.Folio || 'Sin folio')}</td><td>${escHtml(item.StudentName || 'Sin nombre')}</td><td>${escHtml(item.EnrollmentOrMatricula || 'Sin matrícula')}</td><td>${ad}</td><td>${getPsyBadge(item.AttendanceStatus)}</td><td>${escHtml(item.Observations || '')}</td><td>${ca}</td></tr>`);
        });
        $('#pageInfoPsy').text(pagText(from, to, total));
    }
    function updatePsyCards(list) {
        if (!list || !list.length) { $('#statPsyTotal,#statPsyAttended,#statPsyAbsent,#statPsyJustified').text('0'); return; }
        $('#statPsyTotal').text(list.length);
        $('#statPsyAttended').text(list.filter(i => (i.AttendanceStatus || '') === 'Asistió').length);
        $('#statPsyAbsent').text(list.filter(i => (i.AttendanceStatus || '') === 'No asistió').length);
        $('#statPsyJustified').text(list.filter(i => (i.AttendanceStatus || '') === 'Justificado').length);
    }
    function renderPsyPie(list) {
        const cnt = { 'Asistió': 0, 'No asistió': 0, 'Justificado': 0 };
        list.forEach(i => { const k = cnt[i.AttendanceStatus] !== undefined ? i.AttendanceStatus : 'No asistió'; cnt[k]++; });
        const keys = Object.keys(cnt).filter(k => cnt[k] > 0);
        if (!keys.length) { emptyChart('psyPieChart'); return; }
        makeChart('psyPieChart', {
            type: 'doughnut',
            data: { labels: keys, datasets: [{ data: keys.map(k => cnt[k]), backgroundColor: ['#198754', '#dc3545', '#ffc107'] }] },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' }, ...vlOpts('#fff') } }
        });
    }
    function renderPsyStudentBar(list) {
        if (!list.length) { emptyChart('psyStudentBar'); return; }
        const byS = {};
        list.forEach(i => { const n = i.StudentName || 'Sin nombre'; byS[n] = (byS[n] || 0) + 1; });
        const sorted = Object.entries(byS).sort((a, b) => b[1] - a[1]).slice(0, 10);
        makeChart('psyStudentBar', {
            type: 'bar',
            data: { labels: sorted.map(([l]) => l.split(' ').slice(0, 2).join(' ')), datasets: [{ label: 'Citas', data: sorted.map(([, v]) => v), backgroundColor: '#6f42c1' }] },
            plugins: [valueLabelPlugin],
            options: { indexAxis: 'y', responsive: true, maintainAspectRatio: false, scales: { x: { beginAtZero: true, ticks: { precision: 0 } } }, plugins: { legend: { display: false }, ...vlOpts('#fff') } }
        });
    }
    function renderPsyMonthBar(list) {
        if (!list.length) { emptyChart('psyMonthBar'); return; }
        const byM = {};
        list.forEach(i => {
            const d = i.AppointmentDate || i.CreatedAt; if (!d) return;
            const dt = new Date(d);
            const key = `${dt.getFullYear()}-${String(dt.getMonth() + 1).padStart(2, '0')}`;
            byM[key] = (byM[key] || 0) + 1;
        });
        const labels = Object.keys(byM).sort();
        if (!labels.length) { emptyChart('psyMonthBar'); return; }
        makeChart('psyMonthBar', {
            type: 'bar',
            data: { labels, datasets: [{ label: 'Citas', data: labels.map(l => byM[l]), backgroundColor: '#0d6efd' }] },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }, plugins: { legend: { display: false }, ...vlOpts('#fff') } }
        });
    }
    function renderPsyAttStacked(list) {
        if (!list.length) { emptyChart('psyAttendanceStacked'); return; }
        const byM = {};
        list.forEach(i => {
            const d = i.AppointmentDate || i.CreatedAt; if (!d) return;
            const dt = new Date(d);
            const key = `${dt.getFullYear()}-${String(dt.getMonth() + 1).padStart(2, '0')}`;
            if (!byM[key]) byM[key] = { a: 0, na: 0, j: 0 };
            const s = i.AttendanceStatus || '';
            if (s === 'Asistió') byM[key].a++; else if (s === 'Justificado') byM[key].j++; else byM[key].na++;
        });
        const labels = Object.keys(byM).sort();
        if (!labels.length) { emptyChart('psyAttendanceStacked'); return; }
        makeChart('psyAttendanceStacked', {
            type: 'bar',
            data: {
                labels, datasets: [
                    { label: 'Asistió', data: labels.map(l => byM[l].a), backgroundColor: '#198754' },
                    { label: 'No asistió', data: labels.map(l => byM[l].na), backgroundColor: '#dc3545' },
                    { label: 'Justificado', data: labels.map(l => byM[l].j), backgroundColor: '#ffc107' }
                ]
            },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, scales: { x: { stacked: true }, y: { stacked: true, beginAtZero: true, ticks: { precision: 0 } } }, plugins: { legend: { position: 'top' }, ...vlOpts('#fff') } }
        });
    }
    function renderAllPsyCharts(list) { renderPsyPie(list); renderPsyStudentBar(list); renderPsyMonthBar(list); renderPsyAttStacked(list); }

    function applyPsyFilters() {
        const name = ($('#filterPsyName').val() || '').toLowerCase().trim();
        const folio = ($('#filterPsyFolio').val() || '').toLowerCase().trim();
        const status = $('#filterPsyStatus').val() || '';
        const mat = ($('#filterPsyMatricula').val() || '').toLowerCase().trim();
        filteredPsy = psychologyLogs.filter(i =>
            (!name || (i.StudentName || '').toLowerCase().includes(name)) &&
            (!folio || String(i.Folio || '').toLowerCase().includes(folio)) &&
            (!status || (i.AttendanceStatus || '') === status) &&
            (!mat || (i.EnrollmentOrMatricula || '').toLowerCase().includes(mat))
        );
        pagePsy = 1; populatePsyTable(filteredPsy); updatePsyCards(filteredPsy); renderAllPsyCharts(filteredPsy);
    }
    function resetPsyFilters() {
        $('#filterPsyName,#filterPsyFolio,#filterPsyMatricula').val(''); $('#filterPsyStatus').val('');
        psyExcelFilters = {}; filteredPsy = [...psychologyLogs];
        pagePsy = 1; populatePsyTable(filteredPsy); updatePsyCards(filteredPsy); renderAllPsyCharts(filteredPsy);
    }

    // ════════════════════════════════════════════════════════════════════════
    // BITÁCORAS — ENFERMERÍA
    // ════════════════════════════════════════════════════════════════════════
    function getMedBadge(s) {
        const m = { 'Estable': '<span class="badge bg-success">Estable</span>', 'Alta': '<span class="badge bg-success">Alta</span>', 'Urgente': '<span class="badge bg-danger">Urgente</span>', 'Critico': '<span class="badge bg-danger">Crítico</span>', 'Observacion': '<span class="badge bg-warning text-dark">Observación</span>', 'Pendiente': '<span class="badge bg-warning text-dark">Pendiente</span>' };
        return m[s] || `<span class="badge bg-secondary">${escHtml(s || 'Sin estado')}</span>`;
    }
    function populateMedTable(list) {
        const tbody = $('#medicalTable tbody'); if (!tbody.length) return;
        tbody.empty();
        if (!list || !list.length) { tbody.html('<tr><td colspan="9" class="text-center text-muted py-4">Sin datos disponibles</td></tr>'); $('#pageInfoMed').text('Sin resultados'); return; }
        const { items, from, to, total } = paginate(list, pageMed, PAGE_SIZE);
        items.forEach(item => {
            const rd = item.RecordDate ? new Date(item.RecordDate).toLocaleString('es-MX') : 'N/A';
            tbody.append(`<tr><td>${escHtml(item.Folio || 'Sin folio')}</td><td>${escHtml(item.StudentName || 'Sin nombre')}</td><td>${escHtml(item.EnrollmentOrMatricula || 'Sin matrícula')}</td><td>${rd}</td><td>${escHtml(item.ConsultationReason || '')}</td><td>${escHtml(item.VitalSigns || '')}</td><td>${escHtml(item.Observations || '')}</td><td>${escHtml(item.TreatmentAction || '')}</td><td>${getMedBadge(item.Status)}</td></tr>`);
        });
        $('#pageInfoMed').text(pagText(from, to, total));
    }
    function updateMedCards(list) {
        if (!list || !list.length) { $('#statMedTotal,#statMedAttended,#statMedPending,#statMedOther').text('0'); return; }
        const ok = ['Estable', 'Alta'], bad = ['Urgente', 'Critico', 'Observacion', 'Pendiente'];
        $('#statMedTotal').text(list.length);
        $('#statMedAttended').text(list.filter(i => ok.includes(i.Status || '')).length);
        $('#statMedPending').text(list.filter(i => bad.includes(i.Status || '')).length);
        $('#statMedOther').text(list.filter(i => !ok.includes(i.Status || '') && !bad.includes(i.Status || '')).length);
    }
    function renderMedPie(list) {
        if (!list.length) { emptyChart('medPieChart'); return; }
        const cnt = {};
        list.forEach(i => { const s = i.Status || 'Sin estado'; cnt[s] = (cnt[s] || 0) + 1; });
        const keys = Object.keys(cnt);
        const palette = ['#198754', '#20c997', '#dc3545', '#e74c3c', '#ffc107', '#6c757d'];
        makeChart('medPieChart', {
            type: 'doughnut',
            data: { labels: keys, datasets: [{ data: keys.map(k => cnt[k]), backgroundColor: keys.map((_, i) => palette[i % palette.length]) }] },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' }, ...vlOpts('#fff') } }
        });
    }
    function renderMedReasonBar(list) {
        if (!list.length) { emptyChart('medReasonBar'); return; }
        const byR = {};
        list.forEach(i => { const r = i.ConsultationReason || 'Sin motivo'; byR[r] = (byR[r] || 0) + 1; });
        const sorted = Object.entries(byR).sort((a, b) => b[1] - a[1]).slice(0, 10);
        makeChart('medReasonBar', {
            type: 'bar',
            data: { labels: sorted.map(([l]) => l), datasets: [{ label: 'Consultas', data: sorted.map(([, v]) => v), backgroundColor: '#fd7e14' }] },
            plugins: [valueLabelPlugin],
            options: { indexAxis: 'y', responsive: true, maintainAspectRatio: false, scales: { x: { beginAtZero: true, ticks: { precision: 0 } } }, plugins: { legend: { display: false }, ...vlOpts('#fff') } }
        });
    }
    function renderMedMonthBar(list) {
        if (!list.length) { emptyChart('medMonthBar'); return; }
        const byM = {};
        list.forEach(i => {
            const d = i.RecordDate; if (!d) return;
            const dt = new Date(d);
            const key = `${dt.getFullYear()}-${String(dt.getMonth() + 1).padStart(2, '0')}`;
            byM[key] = (byM[key] || 0) + 1;
        });
        const labels = Object.keys(byM).sort();
        if (!labels.length) { emptyChart('medMonthBar'); return; }
        makeChart('medMonthBar', {
            type: 'bar',
            data: { labels, datasets: [{ label: 'Consultas', data: labels.map(l => byM[l]), backgroundColor: '#0d6efd' }] },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }, plugins: { legend: { display: false }, ...vlOpts('#fff') } }
        });
    }
    function renderMedStudentBar(list) {
        if (!list.length) { emptyChart('medStudentBar'); return; }
        const byS = {};
        list.forEach(i => { const n = i.StudentName || 'Sin nombre'; byS[n] = (byS[n] || 0) + 1; });
        const sorted = Object.entries(byS).sort((a, b) => b[1] - a[1]).slice(0, 10);
        makeChart('medStudentBar', {
            type: 'bar',
            data: { labels: sorted.map(([l]) => l.split(' ').slice(0, 2).join(' ')), datasets: [{ label: 'Consultas', data: sorted.map(([, v]) => v), backgroundColor: '#6610f2' }] },
            plugins: [valueLabelPlugin],
            options: { indexAxis: 'y', responsive: true, maintainAspectRatio: false, scales: { x: { beginAtZero: true, ticks: { precision: 0 } } }, plugins: { legend: { display: false }, ...vlOpts('#fff') } }
        });
    }
    function renderAllMedCharts(list) { renderMedPie(list); renderMedReasonBar(list); renderMedMonthBar(list); renderMedStudentBar(list); }

    function applyMedFilters() {
        const name = ($('#filterMedName').val() || '').toLowerCase().trim();
        const folio = ($('#filterMedFolio').val() || '').toLowerCase().trim();
        const status = $('#filterMedStatus').val() || '';
        const reason = ($('#filterMedReason').val() || '').toLowerCase().trim();
        filteredMed = medicalLogs.filter(i =>
            (!name || (i.StudentName || '').toLowerCase().includes(name)) &&
            (!folio || String(i.Folio || '').toLowerCase().includes(folio)) &&
            (!status || (i.Status || '') === status) &&
            (!reason || (i.ConsultationReason || '').toLowerCase().includes(reason))
        );
        pageMed = 1; populateMedTable(filteredMed); updateMedCards(filteredMed); renderAllMedCharts(filteredMed);
    }
    function resetMedFilters() {
        $('#filterMedName,#filterMedFolio,#filterMedReason').val(''); $('#filterMedStatus').val('');
        medExcelFilters = {}; filteredMed = [...medicalLogs];
        pageMed = 1; populateMedTable(filteredMed); updateMedCards(filteredMed); renderAllMedCharts(filteredMed);
    }

    // ════════════════════════════════════════════════════════════════════════
    // GENERAL — Dashboard ejecutivo
    // ════════════════════════════════════════════════════════════════════════
    function renderGeneralCharts() {
        const totalBitacoras = psychologyLogs.length + medicalLogs.length;
        const set = (id, val) => { const el = document.getElementById(id); if (el) el.textContent = val; };
        set('genKpiAlumnos', students.length);
        set('genKpiServicio', socialServices.length);
        set('genKpiTramites', procedures.length);
        set('genKpiBitacoras', totalBitacoras);

        const stMap = { Inscrito: 0, Cursando: 0, Aprobado: 0, Reprobado: 0 };
        students.forEach(s => { if (stMap[s.Estado] !== undefined) stMap[s.Estado]++; });
        const stLabels = Object.keys(stMap).filter(k => stMap[k] > 0);
        const stColors = { Inscrito: '#6c757d', Cursando: '#ffc107', Aprobado: '#198754', Reprobado: '#dc3545' };
        if (stLabels.length) {
            makeChart('genAlumnosPie', {
                type: 'doughnut',
                data: { labels: stLabels, datasets: [{ data: stLabels.map(k => stMap[k]), backgroundColor: stLabels.map(k => stColors[k]) }] },
                plugins: [valueLabelPlugin],
                options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' }, ...vlOpts('#fff') } }
            });
        } else { emptyChart('genAlumnosPie'); }

        const genAlumnosBody = document.getElementById('genAlumnosBody');
        if (genAlumnosBody) {
            const total = students.length || 1;
            genAlumnosBody.innerHTML = Object.entries(stMap)
                .map(([k, v]) => {
                    const pct = total > 0 ? ((v / total) * 100).toFixed(1) : '0.0';
                    const c = stColors[k] || '#aaa';
                    return `<tr><td><span style="display:inline-block;width:10px;height:10px;border-radius:50%;background:${c};margin-right:6px;"></span>${k}</td><td class="text-end fw-semibold">${v}</td><td class="text-end text-muted">${pct}%</td></tr>`;
                }).join('');
        }

        const aprobados = stMap['Aprobado'] || 0;
        const aprobPct = students.length > 0 ? ((aprobados / students.length) * 100).toFixed(1) : '0.0';
        set('genAprobadosPct', aprobPct + '%');
        const aprobBar = document.getElementById('genAprobadosBar');
        if (aprobBar) aprobBar.style.width = aprobPct + '%';

        const ssMap = { 'Completado': 0, 'En progreso': 0, 'Pendiente': 0 };
        socialServices.forEach(i => { const k = ssMap[i.Status] !== undefined ? i.Status : 'Pendiente'; ssMap[k]++; });
        const ssLabels = Object.keys(ssMap).filter(k => ssMap[k] > 0);
        if (ssLabels.length) {
            makeChart('genServicioPie', {
                type: 'doughnut',
                data: { labels: ssLabels, datasets: [{ data: ssLabels.map(k => ssMap[k]), backgroundColor: ['#198754', '#ffc107', '#6c757d'] }] },
                plugins: [valueLabelPlugin],
                options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' }, ...vlOpts('#fff') } }
            });
        } else { emptyChart('genServicioPie'); }

        const ssBody = document.getElementById('genServicioBody');
        if (ssBody) {
            const totalPresent = socialServices.reduce((s, i) => s + (i.TotalPresent || 0), 0);
            const totalAbsent = socialServices.reduce((s, i) => s + (i.TotalAbsent || 0), 0);
            const wa = socialServices.filter(i => (i.TotalAttendances || 0) > 0);
            const avgAtt = wa.length > 0
                ? (wa.reduce((s, i) => s + (i.AttendanceRate || 0), 0) / wa.length).toFixed(1)
                : '0.0';
            ssBody.innerHTML = `
                <tr><td>Alumnos activos</td><td class="text-end fw-semibold">${socialServices.length}</td></tr>
                <tr><td>Asistencias totales</td><td class="text-end fw-semibold">${totalPresent}</td></tr>
                <tr><td>Faltas totales</td><td class="text-end fw-semibold">${totalAbsent}</td></tr>
                <tr><td>Asistencia promedio</td><td class="text-end fw-semibold">${avgAtt}%</td></tr>`;
        }

        const trCodeMap = { APPROVED: 'Pagó inscripción', PENDING: 'Pendiente', REJECTED: 'No pagó' };
        const trMap = { 'Pagó inscripción': 0, 'Pendiente': 0, 'No pagó': 0 };
        procedures.forEach(i => { const k = trCodeMap[i.InternalCode] || 'Otro'; trMap[k] = (trMap[k] || 0) + 1; });
        const trLabels = Object.keys(trMap).filter(k => trMap[k] > 0);
        if (trLabels.length) {
            makeChart('genTramitesPie', {
                type: 'doughnut',
                data: { labels: trLabels, datasets: [{ data: trLabels.map(k => trMap[k]), backgroundColor: ['#198754', '#ffc107', '#dc3545', '#6c757d'] }] },
                plugins: [valueLabelPlugin],
                options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom' }, ...vlOpts('#fff') } }
            });
        } else { emptyChart('genTramitesPie'); }

        const trBody = document.getElementById('genTramitesBody');
        if (trBody) {
            const trTotal = procedures.length;
            const trApproved = trMap['Pagó inscripción'] || 0;
            const trPending = trMap['Pendiente'] || 0;
            const trRejected = trMap['No pagó'] || 0;
            const trPct = trTotal > 0 ? ((trApproved / trTotal) * 100).toFixed(1) : '0.0';
            trBody.innerHTML = `
                <tr><td>Total solicitudes</td><td class="text-end fw-semibold">${trTotal}</td></tr>
                <tr><td><span style="display:inline-block;width:10px;height:10px;border-radius:50%;background:#198754;margin-right:6px;"></span>Pagó inscripción</td><td class="text-end fw-semibold text-success">${trApproved}</td></tr>
                <tr><td><span style="display:inline-block;width:10px;height:10px;border-radius:50%;background:#ffc107;margin-right:6px;"></span>Pendiente</td><td class="text-end fw-semibold">${trPending}</td></tr>
                <tr><td><span style="display:inline-block;width:10px;height:10px;border-radius:50%;background:#dc3545;margin-right:6px;"></span>No pagó</td><td class="text-end fw-semibold text-danger">${trRejected}</td></tr>
                <tr class="table-light"><td class="text-muted small">% con pago</td><td class="text-end fw-bold text-success">${trPct}%</td></tr>`;
        }

        const psyMap = { 'Asistió': 0, 'No asistió': 0, 'Justificado': 0 };
        psychologyLogs.forEach(i => { const k = psyMap[i.AttendanceStatus] !== undefined ? i.AttendanceStatus : 'No asistió'; psyMap[k]++; });
        const medStatusMap = { 'Estable': 0, 'Alta': 0, 'Urgente': 0, 'Critico': 0, 'Observacion': 0, 'Pendiente': 0 };
        medicalLogs.forEach(i => { const s = i.Status || 'Pendiente'; if (medStatusMap[s] !== undefined) medStatusMap[s]++; else medStatusMap['Pendiente']++; });
        const medPositivo = (medStatusMap['Estable'] || 0) + (medStatusMap['Alta'] || 0);
        const medNegativo = (medStatusMap['Urgente'] || 0) + (medStatusMap['Critico'] || 0);
        const medOtro = (medStatusMap['Observacion'] || 0) + (medStatusMap['Pendiente'] || 0);

        makeChart('genBitacorasBar', {
            type: 'bar',
            data: {
                labels: ['Total', 'Positivo', 'Negativo', 'Otro'],
                datasets: [
                    { label: 'Psicología', data: [psychologyLogs.length, psyMap['Asistió'], psyMap['No asistió'], psyMap['Justificado']], backgroundColor: '#6610f2' },
                    { label: 'Enfermería', data: [medicalLogs.length, medPositivo, medNegativo, medOtro], backgroundColor: '#0d6efd' }
                ]
            },
            plugins: [valueLabelPlugin],
            options: { responsive: true, maintainAspectRatio: false, scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }, plugins: { legend: { position: 'bottom' }, ...vlOpts('#fff') } }
        });

        const bitBody = document.getElementById('genBitacorasBody');
        if (bitBody) {
            bitBody.innerHTML = `
                <tr><td>Total registros</td><td class="text-end fw-semibold">${psychologyLogs.length}</td><td class="text-end fw-semibold">${medicalLogs.length}</td></tr>
                <tr><td>Positivo (Asistió / Estable+Alta)</td><td class="text-end fw-semibold text-success">${psyMap['Asistió']}</td><td class="text-end fw-semibold text-success">${medPositivo}</td></tr>
                <tr><td>Negativo (No asistió / Urgente+Crít.)</td><td class="text-end fw-semibold text-danger">${psyMap['No asistió']}</td><td class="text-end fw-semibold text-danger">${medNegativo}</td></tr>
                <tr><td>Otro (Justificado / Obs.+Pend.)</td><td class="text-end fw-semibold text-warning">${psyMap['Justificado']}</td><td class="text-end fw-semibold text-warning">${medOtro}</td></tr>`;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // EXPORTAR CSV  — FIX: usa getActiveSection() para detectar sub-vista
    // ════════════════════════════════════════════════════════════════════════
    function exportVisibleTableToCSV(tableId, fn) {
        const $t = $('#' + tableId); if (!$t.length) return;
        const rows = [], h = [];
        $t.find('thead th').each(function () { h.push(csvEsc($(this).text().replace('▼', '').replace('▲', '').trim())); });
        rows.push(h.join(','));
        $t.find('tbody tr').each(function () {
            const c = []; $(this).find('td').each(function () { c.push(csvEsc($(this).text().trim())); });
            if (c.length) rows.push(c.join(','));
        });
        dlCSV(rows.join('\n'), fn);
    }
    function exportDataToCSV(data, headers, keys, fn) {
        if (!data || !data.length) return;
        const rows = [headers.map(csvEsc).join(',')];
        data.forEach(item => rows.push(keys.map(k => csvEsc(String(item[k] !== undefined ? item[k] : ''))).join(',')));
        dlCSV(rows.join('\n'), fn);
    }
    function dlCSV(content, fn) {
        const blob = new Blob(['\uFEFF' + content], { type: 'text/csv;charset=utf-8;' });
        const a = document.createElement('a'); a.href = URL.createObjectURL(blob); a.download = fn || 'export.csv';
        document.body.appendChild(a); a.click(); document.body.removeChild(a); URL.revokeObjectURL(a.href);
    }

    // ════════════════════════════════════════════════════════════════════════
    // EXPORTAR PDF  — FIX: usa getActiveSection() para detectar sub-vista
    // ════════════════════════════════════════════════════════════════════════
    function exportToPDF() {
        if (!window.jspdf || !window.jspdf.jsPDF) { alert('jsPDF no cargó, verifica tu conexión.'); return; }
        const { jsPDF } = window.jspdf;
        const doc = new jsPDF({ orientation: 'landscape', unit: 'mm', format: 'a4' });
        const pageW = doc.internal.pageSize.getWidth();
        const pageH = doc.internal.pageSize.getHeight();
        const margin = 12;
        const dateStr = new Date().toLocaleDateString('es-MX');

        // ── FIX: detectar sección activa con helper centralizado ──────────────
        const section = getActiveSection();
        let title = document.getElementById('pageTitle').textContent;
        let tableId = null, chartIds = [], filename = 'estadisticas.pdf';

        if (section === 'calificaciones_semestre') {
            title = 'Calificaciones — Por Semestre';
            tableId = 'studentsTable';
            chartIds = ['pieChart', 'barChart', 'gradeHistogramChart', 'courseStatusStacked'];
            filename = `calificaciones_semestre_${new Date().toISOString().slice(0, 10)}.pdf`;
        } else if (section === 'calificaciones_grupo') {
            title = 'Calificaciones — Por Grupo';
            tableId = 'grupoTable';
            chartIds = ['pieChartGrupo', 'barChartGrupo', 'gradeHistogramChartGrupo', 'groupStatusStacked'];
            filename = `calificaciones_grupo_${new Date().toISOString().slice(0, 10)}.pdf`;
        } else if (section === 'social') {
            title = 'Estadísticas de Servicio Social';
            tableId = 'socialServiceTable';
            chartIds = ['socialPieChart', 'socialHoursBar', 'socialAttendanceBar', 'socialHoursStacked'];
            filename = `servicios_sociales_${new Date().toISOString().slice(0, 10)}.pdf`;
        } else if (section === 'tramites') {
            title = 'Estadísticas de Trámites';
            tableId = 'proceduresTable';
            chartIds = ['tramitesPieChart', 'tramitesAreaBar', 'tramitesTipoBar', 'tramitesDiasBar'];
            filename = `tramites_${new Date().toISOString().slice(0, 10)}.pdf`;
        } else if (section === 'psicologia') {
            title = 'Bitácoras — Psicología';
            tableId = 'psychologyTable';
            chartIds = ['psyPieChart', 'psyStudentBar', 'psyMonthBar', 'psyAttendanceStacked'];
            filename = `psicologia_${new Date().toISOString().slice(0, 10)}.pdf`;
        } else if (section === 'enfermeria') {
            title = 'Bitácoras — Enfermería';
            tableId = 'medicalTable';
            chartIds = ['medPieChart', 'medReasonBar', 'medMonthBar', 'medStudentBar'];
            filename = `enfermeria_${new Date().toISOString().slice(0, 10)}.pdf`;
        } else {
            alert('Selecciona una sección antes de exportar el PDF.');
            return;
        }

        // Cabecera
        doc.setFillColor(98, 9, 0); doc.rect(0, 0, pageW, 16, 'F');
        doc.setTextColor(255, 255, 255); doc.setFontSize(13); doc.setFont('helvetica', 'bold');
        doc.text(title, margin, 10.5);
        doc.setFontSize(9); doc.setFont('helvetica', 'normal');
        doc.text(`Generado: ${dateStr}`, pageW - margin, 10.5, { align: 'right' });
        doc.setTextColor(0, 0, 0);

        let cy = 22;

        // Tarjetas de stats
        const statCards = getPDFStatCards(section);
        if (statCards.length) {
            const cw = (pageW - margin * 2 - (statCards.length - 1) * 4) / statCards.length;
            statCards.forEach((card, i) => {
                const cx = margin + i * (cw + 4);
                doc.setFillColor(245, 242, 240); doc.setDrawColor(177, 165, 150);
                doc.roundedRect(cx, cy, cw, 14, 2, 2, 'FD');
                doc.setFontSize(7); doc.setFont('helvetica', 'normal'); doc.setTextColor(80, 80, 80);
                doc.text(card.label, cx + cw / 2, cy + 5, { align: 'center' });
                doc.setFontSize(11); doc.setFont('helvetica', 'bold'); doc.setTextColor(98, 9, 0);
                doc.text(String(card.value), cx + cw / 2, cy + 11, { align: 'center' });
            });
            doc.setTextColor(0, 0, 0); cy += 20;
        }

        // Tabla — exporta TODOS los datos filtrados (no sólo la página visible)
        if (tableId) {
            const $table = $('#' + tableId);
            const headers = [];
            $table.find('thead th').each(function () {
                headers.push($(this).text().replace('▼', '').replace('▲', '').trim());
            });
            const rows = [];
            $table.find('tbody tr').each(function () {
                const row = []; $(this).find('td').each(function () { row.push($(this).text().trim()); });
                if (row.length) rows.push(row);
            });
            if (rows.length) {
                doc.setFontSize(8); doc.setFont('helvetica', 'bold'); doc.setTextColor(98, 9, 0);
                doc.text('Datos de la tabla', margin, cy); cy += 3; doc.setTextColor(0, 0, 0);
                doc.autoTable({
                    head: [headers], body: rows, startY: cy,
                    margin: { left: margin, right: margin },
                    styles: { fontSize: 7, cellPadding: 2, overflow: 'linebreak' },
                    headStyles: { fillColor: [98, 9, 0], textColor: 255, fontStyle: 'bold' },
                    alternateRowStyles: { fillColor: [245, 242, 240] },
                    didDrawPage: (data) => {
                        if (data.pageNumber > 1) {
                            doc.setFillColor(98, 9, 0); doc.rect(0, 0, pageW, 10, 'F');
                            doc.setTextColor(255, 255, 255); doc.setFontSize(9);
                            doc.text(title, margin, 7); doc.setTextColor(0, 0, 0);
                        }
                    }
                });
                cy = doc.lastAutoTable.finalY + 8;
            }
        }

        // Gráficas (2 por fila)
        const validCharts = chartIds.filter(id => document.getElementById(id) && charts[id]);
        if (validCharts.length) {
            if (cy + 65 > pageH) {
                doc.addPage();
                doc.setFillColor(98, 9, 0); doc.rect(0, 0, pageW, 10, 'F');
                doc.setTextColor(255, 255, 255); doc.setFontSize(9);
                doc.text(title + ' — Gráficas', margin, 7); doc.setTextColor(0, 0, 0);
                cy = 16;
            }
            doc.setFontSize(8); doc.setFont('helvetica', 'bold'); doc.setTextColor(98, 9, 0);
            doc.text('Gráficas', margin, cy); cy += 4; doc.setTextColor(0, 0, 0);

            const chartW = (pageW - margin * 2 - 6) / 2;
            const chartH = 58;

            validCharts.forEach((id, idx) => {
                const col = idx % 2;
                if (col === 0 && idx > 0 && cy + chartH + 2 > pageH - 8) {
                    doc.addPage();
                    doc.setFillColor(98, 9, 0); doc.rect(0, 0, pageW, 10, 'F');
                    doc.setTextColor(255, 255, 255); doc.setFontSize(9);
                    doc.text(title + ' — Gráficas', margin, 7); doc.setTextColor(0, 0, 0);
                    cy = 16;
                }
                const cx = margin + col * (chartW + 6);
                const canvas = document.getElementById(id);
                const cardEl = canvas ? canvas.closest('.card') : null;
                const chartTitle = cardEl ? (cardEl.querySelector('h5') || {}).textContent || '' : '';
                doc.setFontSize(7); doc.setFont('helvetica', 'bold'); doc.setTextColor(98, 9, 0);
                doc.text(chartTitle, cx, cy); doc.setTextColor(0, 0, 0);
                try {
                    const imgData = canvas.toDataURL('image/png', 1.0);
                    doc.setDrawColor(177, 165, 150); doc.setFillColor(255, 255, 255);
                    doc.roundedRect(cx, cy + 2, chartW, chartH - 4, 2, 2, 'FD');
                    doc.addImage(imgData, 'PNG', cx + 1, cy + 3, chartW - 2, chartH - 6);
                } catch (e) {
                    doc.setFontSize(7); doc.setTextColor(150, 150, 150);
                    doc.text('(gráfica no disponible)', cx + chartW / 2, cy + chartH / 2, { align: 'center' });
                    doc.setTextColor(0, 0, 0);
                }
                if (col === 1 || idx === validCharts.length - 1) { cy += chartH + 6; }
            });
        }

        // Pie de página
        const totalPages = doc.internal.getNumberOfPages();
        for (let p = 1; p <= totalPages; p++) {
            doc.setPage(p); doc.setFontSize(7); doc.setTextColor(130, 130, 130);
            doc.text(`Página ${p} de ${totalPages} — ${title} — ${dateStr}`, pageW / 2, pageH - 4, { align: 'center' });
        }
        doc.save(filename);
    }

    // FIX: recibe section como parámetro en lugar de releer el DOM
    function getPDFStatCards(section) {
        if (section === 'calificaciones_semestre' || section === 'calificaciones_grupo')
            return [
                { label: 'Total alumnos', value: $('#statTotal').text() },
                { label: 'Inscritos', value: $('#statInscritos').text() },
                { label: 'Cursando', value: $('#statCursando').text() },
                { label: 'Aprobados', value: $('#statAprobados').text() }
            ];
        if (section === 'social')
            return [
                { label: 'Con asistencia', value: $('#statSocialTotal').text() },
                { label: 'Presentes', value: $('#statSocialHours').text() },
                { label: 'Ausencias', value: $('#statSocialPending').text() },
                { label: 'Promedio', value: $('#statSocialAttendance').text() }
            ];
        if (section === 'tramites')
            return [
                { label: 'Total', value: $('#statProcsTotal').text() },
                { label: 'Acción req.', value: $('#statProcsAction').text() },
                { label: 'En proceso', value: $('#statProcsInProgress').text() },
                { label: 'Finalizadas', value: $('#statProcsFinalized').text() }
            ];
        if (section === 'psicologia')
            return [
                { label: 'Total', value: $('#statPsyTotal').text() },
                { label: 'Asistió', value: $('#statPsyAttended').text() },
                { label: 'No asistió', value: $('#statPsyAbsent').text() },
                { label: 'Justificados', value: $('#statPsyJustified').text() }
            ];
        if (section === 'enfermeria')
            return [
                { label: 'Total', value: $('#statMedTotal').text() },
                { label: 'Atendidos', value: $('#statMedAttended').text() },
                { label: 'Pendientes', value: $('#statMedPending').text() },
                { label: 'Otros', value: $('#statMedOther').text() }
            ];
        return [];
    }

    // ════════════════════════════════════════════════════════════════════════
    // INIT
    // ════════════════════════════════════════════════════════════════════════
    $(function () {
        renderGeneralCharts();

        filteredStudents = [...students]; populateStudentsTable(filteredStudents); updateStudentCards(filteredStudents); renderAllStudentCharts(filteredStudents);
        filteredGroups = [...students]; populateGroupTable(filteredGroups); renderAllGroupCharts(filteredGroups);
        window.refreshSemestreCharts = function () { renderAllStudentCharts(filteredStudents); };
        window.refreshGroupCharts = function () { renderAllGroupCharts(filteredGroups); };

        filteredSocial = [...socialServices]; populateSocialTable(filteredSocial); updateSocialCards(filteredSocial); renderAllSocialCharts(filteredSocial);
        filteredProcs = [...procedures]; populateProcsTable(filteredProcs); updateProcCards(filteredProcs); renderAllTramitesCharts(filteredProcs);
        filteredPsy = [...psychologyLogs]; populatePsyTable(filteredPsy); updatePsyCards(filteredPsy); renderAllPsyCharts(filteredPsy);
        filteredMed = [...medicalLogs]; populateMedTable(filteredMed); updateMedCards(filteredMed); renderAllMedCharts(filteredMed);

        // Calificaciones — Por Semestre
        $('#filterName').on('input', applyStudentFilters);
        $('#filterStatus,#filterGenero,#filterSemestre').on('change', applyStudentFilters);
        $('#applyStudentFilters').on('click', applyStudentFilters);
        $('#resetFilters').on('click', resetStudentFilters);
        $('#firstPage').on('click', () => { pageStudents = 1; populateStudentsTable(filteredStudents); });
        $('#prevPage').on('click', () => { if (pageStudents > 1) { pageStudents--; populateStudentsTable(filteredStudents); } });
        $('#nextPage').on('click', () => { const m = Math.ceil(filteredStudents.length / PAGE_SIZE); if (pageStudents < m) { pageStudents++; populateStudentsTable(filteredStudents); } });

        // Calificaciones — Por Grupo
        $('#filterGrupoName,#filterGrupoGrupo').on('input', applyGroupFilters);
        $('#filterGrupoStatus,#filterGrupoGenero').on('change', applyGroupFilters);
        $('#applyGroupFilters').on('click', applyGroupFilters);
        $('#resetGroupFilters').on('click', resetGroupFilters);
        $('#firstPageGrupo').on('click', () => { pageGroups = 1; populateGroupTable(filteredGroups); });
        $('#prevPageGrupo').on('click', () => { if (pageGroups > 1) { pageGroups--; populateGroupTable(filteredGroups); } });
        $('#nextPageGrupo').on('click', () => { const m = Math.ceil(filteredGroups.length / PAGE_SIZE); if (pageGroups < m) { pageGroups++; populateGroupTable(filteredGroups); } });

        // Servicios Sociales
        $('#filterSocialName,#filterSocialTeacher,#filterSocialGroup').on('input', applySocialFilters);
        $('#filterSocialStatus').on('change', applySocialFilters);
        $('#applySocialFilters').on('click', applySocialFilters);
        $('#resetSocialFilters').on('click', resetSocialFilters);
        $('#firstPageSocial').on('click', () => { pageSocial = 1; populateSocialTable(filteredSocial); });
        $('#prevPageSocial').on('click', () => { if (pageSocial > 1) { pageSocial--; populateSocialTable(filteredSocial); } });
        $('#nextPageSocial').on('click', () => { const m = Math.ceil(filteredSocial.length / PAGE_SIZE); if (pageSocial < m) { pageSocial++; populateSocialTable(filteredSocial); } });

        // Trámites
        $('#filterTramiteUser,#filterTramiteFolio,#filterTramiteArea').on('input', applyTramiteFilters);
        $('#filterTramiteStatus').on('change', applyTramiteFilters);
        $('#applyTramiteFilters').on('click', applyTramiteFilters);
        $('#resetTramiteFilters').on('click', resetTramiteFilters);
        $('#firstPageTramites').on('click', () => { pageProcs = 1; populateProcsTable(filteredProcs); });
        $('#prevPageTramites').on('click', () => { if (pageProcs > 1) { pageProcs--; populateProcsTable(filteredProcs); } });
        $('#nextPageTramites').on('click', () => { const m = Math.ceil(filteredProcs.length / PAGE_SIZE); if (pageProcs < m) { pageProcs++; populateProcsTable(filteredProcs); } });

        // Psicología
        $('#filterPsyName,#filterPsyFolio,#filterPsyMatricula').on('input', applyPsyFilters);
        $('#filterPsyStatus').on('change', applyPsyFilters);
        $('#applyPsyFilters').on('click', applyPsyFilters);
        $('#resetPsyFilters').on('click', resetPsyFilters);
        $('#firstPagePsy').on('click', () => { pagePsy = 1; populatePsyTable(filteredPsy); });
        $('#prevPagePsy').on('click', () => { if (pagePsy > 1) { pagePsy--; populatePsyTable(filteredPsy); } });
        $('#nextPagePsy').on('click', () => { const m = Math.ceil(filteredPsy.length / PAGE_SIZE); if (pagePsy < m) { pagePsy++; populatePsyTable(filteredPsy); } });

        // Enfermería
        $('#filterMedName,#filterMedFolio,#filterMedReason').on('input', applyMedFilters);
        $('#filterMedStatus').on('change', applyMedFilters);
        $('#applyMedFilters').on('click', applyMedFilters);
        $('#resetMedFilters').on('click', resetMedFilters);
        $('#firstPageMed').on('click', () => { pageMed = 1; populateMedTable(filteredMed); });
        $('#prevPageMed').on('click', () => { if (pageMed > 1) { pageMed--; populateMedTable(filteredMed); } });
        $('#nextPageMed').on('click', () => { const m = Math.ceil(filteredMed.length / PAGE_SIZE); if (pageMed < m) { pageMed++; populateMedTable(filteredMed); } });

        // PDF
        $('#btnExportPDF').on('click', exportToPDF);

        // ── FIX: Excel — usa getActiveSection() para detectar sub-vista ─────
        $('#btnExportTable').on('click', function () {
            const d = new Date().toISOString().slice(0, 10);
            const section = getActiveSection();

            if (section === 'calificaciones_semestre') {
                exportVisibleTableToCSV('studentsTable', `calificaciones_semestre_${d}.csv`);
            } else if (section === 'calificaciones_grupo') {
                exportVisibleTableToCSV('grupoTable', `calificaciones_grupo_${d}.csv`);
            } else if (section === 'social') {
                exportDataToCSV(filteredSocial,
                    ['Alumno', 'Maestro asesor', 'Grupo', 'Asistencias', 'Faltas', 'Justificadas', '% Asistencia', 'Estado', 'Última asistencia'],
                    ['StudentName', 'TeacherName', 'GroupName', 'TotalPresent', 'TotalAbsent', 'TotalJustified', 'AttendanceRate', 'Status', 'LastAttendanceDate'],
                    `servicios_sociales_${d}.csv`);
            } else if (section === 'tramites') {
                exportDataToCSV(filteredProcs,
                    ['Folio', 'Usuario', 'Tipo de trámite', 'Área', 'Estado', 'Fecha creación', 'Fecha actualización', 'Días transcurridos'],
                    ['Folio', 'StudentName', 'ProcedureType', 'AreaName', 'StatusName', 'DateCreated', 'DateUpdated', 'DaysElapsed'],
                    `tramites_${d}.csv`);
            } else if (section === 'psicologia') {
                exportDataToCSV(filteredPsy,
                    ['Folio', 'Alumno', 'Matrícula', 'Fecha cita', 'Asistencia', 'Observaciones', 'Fecha creación'],
                    ['Folio', 'StudentName', 'EnrollmentOrMatricula', 'AppointmentDate', 'AttendanceStatus', 'Observations', 'CreatedAt'],
                    `psicologia_${d}.csv`);
            } else if (section === 'enfermeria') {
                exportDataToCSV(filteredMed,
                    ['Folio', 'Alumno', 'Matrícula', 'Fecha registro', 'Motivo', 'Signos vitales', 'Observaciones', 'Acción', 'Estado'],
                    ['Folio', 'StudentName', 'EnrollmentOrMatricula', 'RecordDate', 'ConsultationReason', 'VitalSigns', 'Observations', 'TreatmentAction', 'Status'],
                    `enfermeria_${d}.csv`);
            }
        });
    });

})(); // ── fin IIFE ──


// ════════════════════════════════════════════════════════════════════════════
// FILTROS TIPO EXCEL — 5 tablas
// ════════════════════════════════════════════════════════════════════════════
let studentExcelFilters = {};
let socialExcelFilters = {};
let tramiteExcelFilters = {};
let psyExcelFilters = {};
let medExcelFilters = {};
let grupoExcelFilters = {};

function getExcelFilters(tableId) {
    if (tableId === 'studentsTable') return studentExcelFilters;
    if (tableId === 'socialServiceTable') return socialExcelFilters;
    if (tableId === 'proceduresTable') return tramiteExcelFilters;
    if (tableId === 'psychologyTable') return psyExcelFilters;
    if (tableId === 'medicalTable') return medExcelFilters;
    if (tableId === 'grupoTable') return grupoExcelFilters;
    return {};
}
function setExcelFilters(tableId, obj) {
    if (tableId === 'studentsTable') { studentExcelFilters = obj; return; }
    if (tableId === 'socialServiceTable') { socialExcelFilters = obj; return; }
    if (tableId === 'proceduresTable') { tramiteExcelFilters = obj; return; }
    if (tableId === 'psychologyTable') { psyExcelFilters = obj; return; }
    if (tableId === 'medicalTable') { medExcelFilters = obj; return; }
    if (tableId === 'grupoTable') { grupoExcelFilters = obj; return; }
}

// Paginación Calificaciones (DOM)
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

const sortState = {};

$(document).on('click', '.excel-icon', function (e) {
    e.stopPropagation();

    const $th = $(this).closest('.excel-header');
    const col = $th.data('col');
    const tableId = $th.data('table');
    const offset = $th.offset();

    const existing = $('.excel-filter-popup');
    if (existing.length && existing.data('origin-col') == col && existing.data('origin-table') === tableId) {
        existing.remove(); return;
    }
    $('.excel-filter-popup').remove();

    const $table = $('#' + tableId);
    const curFilters = getExcelFilters(tableId);
    const values = new Set();
    $table.find('tbody tr').each(function () {
        const txt = $(this).find('td').eq(col).text().trim();
        if (txt) values.add(txt);
    });

    const popup = $(`
        <div class="excel-filter-popup">
            <input type="text" class="search-filter" placeholder="Buscar valor..."
                style="width:100%;padding:6px;border-radius:4px;border:1px solid rgba(255,255,255,0.12);background:#2a2a2a;color:#fff;margin-bottom:4px;box-sizing:border-box;">
            <div class="filter-values"></div>
            <div style="display:flex;justify-content:flex-end;gap:6px;margin-top:8px;">
                <button class="select-all" style="font-size:11px;">Todo</button>
                <button class="deselect-all" style="font-size:11px;">Ninguno</button>
                <button class="apply-filter" style="background:#0d6efd;padding:4px 14px;">OK</button>
            </div>
        </div>
    `);
    popup.css({
        top: Math.min(offset.top + $th.outerHeight() + 4 - $(window).scrollTop(), window.innerHeight - 280),
        left: Math.min(offset.left, window.innerWidth - 260)
    });
    const container = popup.find('.filter-values');
    const active = curFilters[col] || null;
    values.forEach(v => {
        const checked = (!active || active.includes(v)) ? 'checked' : '';
        container.append(`<label><input type="checkbox" value="${v.replace(/"/g, '&quot;')}" ${checked}> ${v}</label>`);
    });
    $('body').append(popup);
    popup.data('origin-col', col).data('origin-table', tableId);
    popup.on('click', ev => ev.stopPropagation());
    popup.find('.select-all').click(() => container.find('input[type=checkbox]').prop('checked', true));
    popup.find('.deselect-all').click(() => container.find('input[type=checkbox]').prop('checked', false));
    popup.find('.apply-filter').click(function () {
        const selected = [];
        popup.find('input[type=checkbox]:checked').each(function () { selected.push($(this).val()); });
        const f = getExcelFilters(tableId); f[col] = selected; setExcelFilters(tableId, f);
        applyExcelFor(tableId); popup.remove();
    });
    popup.find('.search-filter').on('input', function () {
        const txt = $(this).val().toLowerCase();
        container.find('label').each(function () { $(this).toggle($(this).text().toLowerCase().includes(txt)); });
    });
});

$(document).on('click', '.excel-header', function (e) {
    if ($(e.target).hasClass('excel-icon')) return;
    e.stopPropagation();
    $('.excel-filter-popup').remove();

    const col = $(this).data('col');
    const tableId = $(this).data('table');
    const key = tableId + '_' + col;

    const asc = !(sortState[key] === true);
    sortState[key] = asc;

    $(this).find('.excel-icon').text(asc ? '▲' : '▼');
    $('[data-table="' + tableId + '"].excel-header').not(this).find('.excel-icon').text('▼');

    sortGeneric(tableId, col, asc);
});

function applyExcelFor(tableId) {
    const f = getExcelFilters(tableId);
    $('#' + tableId + ' tbody tr').each(function () {
        let show = true;
        for (const col in f) {
            const val = $(this).find('td').eq(col).text().trim();
            if (!f[col].includes(val)) { show = false; break; }
        }
        $(this).toggle(show);
    });
    if (tableId === 'studentsTable') {
        const vis = $('#studentsTable tbody tr:visible').length;
        $('#pageInfo').text(vis > 0 ? `Mostrando 1-${Math.min(vis, PAGE_SIZE)} de ${vis}` : 'Sin resultados');
    } else if (tableId === 'grupoTable') {
        const vis = $('#grupoTable tbody tr:visible').length;
        $('#pageInfoGrupo').text(vis > 0 ? `Mostrando 1-${Math.min(vis, PAGE_SIZE)} de ${vis}` : 'Sin resultados');
    }
}
//finally
function sortGeneric(tableId, col, asc) {
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
}

$(document).on('click', function (e) {
    if (!$(e.target).closest('.excel-filter-popup').length && !$(e.target).closest('.excel-header').length)
        $('.excel-filter-popup').remove();
});