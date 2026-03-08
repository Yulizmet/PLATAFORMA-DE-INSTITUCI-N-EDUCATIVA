// index-scripts.js (SIN CÓDIGO REPETIDO)
(() => {
    "use strict";

    // ========= Helpers =========
    function onReady(fn) {
        if (document.readyState === "loading") {
            document.addEventListener("DOMContentLoaded", fn);
        } else {
            fn();
        }
    }

    function $(id) {
        return document.getElementById(id);
    }

    function show(el, hiddenClass = "d-none") {
        if (!el) return;
        el.classList.remove(hiddenClass);
    }

    function hide(el, hiddenClass = "d-none") {
        if (!el) return;
        el.classList.add(hiddenClass);
    }

    // ========= Inputs helpers =========
    function soloTelefono(input) {
        if (!input || typeof input.addEventListener !== "function") return;
        input.addEventListener("input", () => {
            input.value = input.value.replace(/\D/g, "").slice(0, 10);
        });
    }

    function soloNumeros(input) {
        if (!input || typeof input.addEventListener !== "function") return;
        input.addEventListener("input", () => {
            input.value = input.value.replace(/\D/g, "");
        });
    }

    function toggleByRadios({
        radioOn,
        radioOff,
        target,
        hiddenClass = "d-none",
        onHideClearIds = [],
    }) {
        if (!radioOn || !radioOff || !target) return;

        const doShow = () => show(target, hiddenClass);
        const doHide = () => {
            hide(target, hiddenClass);
            onHideClearIds.forEach((id) => {
                const el = $(id);
                if (el) el.value = "";
            });
        };

        radioOn.addEventListener("change", doShow);
        radioOff.addEventListener("change", doHide);

        if (radioOn.checked) doShow();
        if (radioOff.checked) doHide();
    }

    // ========= Bootstrap validation =========
    function initBootstrapValidation() {
        const forms = document.querySelectorAll(".needs-validation");
        Array.from(forms).forEach((form) => {
            form.addEventListener(
                "submit",
                (event) => {
                    if (!form.checkValidity()) {
                        event.preventDefault();
                        event.stopPropagation();
                    }
                    form.classList.add("was-validated");
                },
                false
            );
        });
    }

    // ========= Charts =========
    function initLineChart() {
        const canvas = $("chartjs-dashboard-line");
        if (!canvas) return;
        if (typeof Chart === "undefined") return;

        const ctx = canvas.getContext("2d");
        const gradient = ctx.createLinearGradient(0, 0, 0, 225);
        gradient.addColorStop(0, "rgba(215, 227, 244, 1)");
        gradient.addColorStop(1, "rgba(215, 227, 244, 0)");

        new Chart(canvas, {
            type: "line",
            data: {
                labels: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
                datasets: [{
                    label: "Sales ($)",
                    fill: true,
                    backgroundColor: gradient,
                    borderColor: window.theme?.primary,
                    data: [2115, 1562, 1584, 1892, 1587, 1923, 2566, 2448, 2805, 3438, 2917, 3327],
                }],
            },
            options: {
                maintainAspectRatio: false,
                legend: { display: false },
                tooltips: { intersect: false },
                hover: { intersect: true },
                plugins: { filler: { propagate: false } },
                scales: {
                    xAxes: [{ reverse: true, gridLines: { color: "rgba(0,0,0,0.0)" } }],
                    yAxes: [{
                        ticks: { stepSize: 1000 },
                        display: true,
                        borderDash: [3, 3],
                        gridLines: { color: "rgba(0,0,0,0.0)" },
                    }],
                },
            },
        });
    }

    function initPieChart() {
        const canvas = $("chartjs-dashboard-pie");
        if (!canvas) return;
        if (typeof Chart === "undefined") return;

        new Chart(canvas, {
            type: "pie",
            data: {
                labels: ["Chrome", "Firefox", "IE"],
                datasets: [{
                    data: [4306, 3801, 1689],
                    backgroundColor: [window.theme?.primary, window.theme?.warning, window.theme?.danger],
                    borderWidth: 5,
                }],
            },
            options: {
                responsive: !window.MSInputMethodContext,
                maintainAspectRatio: false,
                legend: { display: false },
                cutoutPercentage: 75,
            },
        });
    }

    function initBarChart() {
        const canvas = $("chartjs-dashboard-bar");
        if (!canvas) return;
        if (typeof Chart === "undefined") return;

        new Chart(canvas, {
            type: "bar",
            data: {
                labels: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
                datasets: [{
                    label: "This year",
                    backgroundColor: window.theme?.primary,
                    borderColor: window.theme?.primary,
                    hoverBackgroundColor: window.theme?.primary,
                    hoverBorderColor: window.theme?.primary,
                    data: [54, 67, 41, 55, 62, 45, 55, 73, 60, 76, 48, 79],
                    barPercentage: 0.75,
                    categoryPercentage: 0.5,
                }],
            },
            options: {
                maintainAspectRatio: false,
                legend: { display: false },
                scales: {
                    yAxes: [{ gridLines: { display: false }, stacked: false, ticks: { stepSize: 20 } }],
                    xAxes: [{ stacked: false, gridLines: { color: "transparent" } }],
                },
            },
        });
    }

    // ========= Wizard steps =========
    function initWizardSteps() {
        const wizard = document.querySelector("#wizardSteps");
        const steps = document.querySelectorAll("#wizardSteps .step-vertical");
        if (!wizard || !steps.length) return;

        const stepByPage = {
            "index.html": 0,
            "alumno.html": 1,
            "finalizar.html": 2,
        };

        const path = window.location.pathname;
        const page = (path.split("/").pop() || "index.html").toLowerCase();
        if (!(page in stepByPage)) return;

        const pageStep = stepByPage[page];

        const hasActivo = !!localStorage.getItem("preinscripcion_activa");
        const enviado = localStorage.getItem("preinscripcion_enviada") === "1";
        const maxStep = enviado ? 2 : hasActivo ? 1 : 0;

        steps.forEach((el) => {
            const step = parseInt(el.dataset.step, 10);
            const href = el.dataset.href;

            el.classList.remove("active", "complete", "locked");
            el.classList.toggle("active", step === pageStep);
            el.classList.toggle("complete", step < pageStep);

            const isForward = step > pageStep;
            const isBeyondMax = step > maxStep;
            if (isForward || isBeyondMax) el.classList.add("locked");

            if (el.dataset.bound === "1") return;
            el.dataset.bound = "1";

            el.addEventListener("click", (e) => {
                e.preventDefault();
                if (step < pageStep && href) {
                    window.location.href = href;
                    return;
                }
                if (step === pageStep) return;
                alert("Primero completa el paso actual antes de continuar.");
            });
        });
    }

    // ========= MX loader (Estados/Municipios) =========
    function createMXLoader() {
        const MX_JSON_URLS = [
            "https://raw.githubusercontent.com/htmike/Estados-y-municipios-MX/master/municipios-341c9-export.json",
            "https://cdn.jsdelivr.net/gh/htmike/Estados-y-municipios-MX@master/municipios-341c9-export.json",
        ];
        const FETCH_TIMEOUT_MS = 12000;

        let dataLoaded = false;
        let estados = [];
        let loadingPromise = null;

        function toTitleCase(text) {
            return String(text)
                .toLowerCase()
                .replace(/\b\p{L}/gu, (c) => c.toUpperCase());
        }

        async function fetchWithTimeout(url, ms) {
            const controller = new AbortController();
            const t = setTimeout(() => controller.abort(), ms);
            try {
                const res = await fetch(url, { signal: controller.signal, cache: "no-store" });
                if (!res.ok) throw new Error(`HTTP ${res.status} en ${url}`);
                return await res.json();
            } finally {
                clearTimeout(t);
            }
        }

        async function loadOnce() {
            if (dataLoaded) return estados;
            if (loadingPromise) return loadingPromise;

            loadingPromise = (async () => {
                let lastErr = null;
                for (const url of MX_JSON_URLS) {
                    try {
                        const raw = await fetchWithTimeout(url, FETCH_TIMEOUT_MS);
                        const arr = Array.isArray(raw) ? raw : Object.values(raw);

                        estados = arr
                            .map((e) => ({
                                nombre: toTitleCase(e.nombre ?? e.nom_ent ?? e.name ?? ""),
                                municipios: Array.isArray(e.municipios)
                                    ? e.municipios.map((m) => toTitleCase(m))
                                    : [],
                            }))
                            .filter((e) => e.nombre);

                        estados.sort((a, b) => a.nombre.localeCompare(b.nombre, "es"));
                        dataLoaded = true;
                        return estados;
                    } catch (err) {
                        lastErr = err;
                    }
                }
                throw lastErr || new Error("No se pudo cargar el JSON de estados/municipios.");
            })();

            return loadingPromise;
        }

        function fillEstados(selectEstado) {
            if (!selectEstado) return;
            selectEstado.innerHTML = '<option value="">Sin selección</option>';
            estados.forEach((e) => {
                const opt = document.createElement("option");
                opt.value = e.nombre;
                opt.textContent = e.nombre;
                selectEstado.appendChild(opt);
            });
        }

        function fillMunicipios(selectMunicipio, estadoNombre) {
            if (!selectMunicipio) return;
            selectMunicipio.innerHTML = '<option value="">Sin selección</option>';
            selectMunicipio.disabled = true;

            const estado = estados.find((e) => e.nombre === estadoNombre);
            if (!estado) return;

            estado.municipios
                .slice()
                .sort((a, b) => a.localeCompare(b, "es"))
                .forEach((m) => {
                    const opt = document.createElement("option");
                    opt.value = m;
                    opt.textContent = m;
                    selectMunicipio.appendChild(opt);
                });

            selectMunicipio.disabled = false;
        }

        async function initPair({ estadoId, municipioId, logLabel }) {
            const selectEstado = $(estadoId);
            const selectMunicipio = $(municipioId);
            if (!selectEstado || !selectMunicipio) return;

            try {
                await loadOnce();
                fillEstados(selectEstado);

                if (selectEstado.value) fillMunicipios(selectMunicipio, selectEstado.value);

                selectEstado.addEventListener("change", () => {
                    fillMunicipios(selectMunicipio, selectEstado.value);
                });
            } catch (e) {
                console.error(`Error cargando estados/municipios para ${logLabel}:`, e);
            }
        }

        return { initPair };
    }

    // ========= Toggles extra =========
    function initTogglesExtra() {
        toggleByRadios({
            radioOn: $("becaSi"),
            radioOff: $("becaNo"),
            target: $("wrapTipoBeca"),
            onHideClearIds: ["tipoBeca"],
        });

        toggleByRadios({
            radioOn: $("lenguaSi"),
            radioOff: $("lenguaNo"),
            target: $("wrapLenguaCual"),
            onHideClearIds: ["lenguaCual"],
        });

        toggleByRadios({
            radioOn: $("discapacidadSi"),
            radioOff: $("discapacidadNo"),
            target: $("wrapDiscapEspec"),
            onHideClearIds: ["discapEspec"],
        });

        toggleByRadios({
            radioOn: $("enfermedadSi"),
            radioOff: $("enfermedadNo"),
            target: $("wrapEnfEspec"),
            onHideClearIds: ["enfEspec"],
        });
    }

    // ========= CP API (colonias por código postal) =========
    function initCPApi() {
        const API_BASE = "https://ariesfall.bsite.net/api/CodigosPostales";

        const cpInput = $("domCP");
        const coloniaSelect = $("domColonia");
        const cpError = $("domCPError");

        const estadoSelect = $("domEstado");
        const municipioSelect = $("domMunicipio");

        // Si no estás en la página que tiene esto, no hace nada
        if (!cpInput || !coloniaSelect || !cpError) return;

        let debounceTimer = null;
        let lastCpRequested = "";

        const normalizeCP = (v) => (v || "").replace(/\D/g, "").slice(0, 5);

        function showError(flag) {
            cpError.classList.toggle("d-none", !flag);
        }

        function resetSelect(selectEl, text = "Sin selección") {
            if (!selectEl) return;
            selectEl.innerHTML = `<option value="">${text}</option>`;
            selectEl.disabled = true;
        }

        function fillSelect(selectEl, items, placeholder = "Sin selección") {
            if (!selectEl) return;
            selectEl.innerHTML = `<option value="">${placeholder}</option>`;
            items.forEach((v) => {
                const val = String(v ?? "").trim();
                if (!val) return;
                const opt = document.createElement("option");
                opt.value = val;
                opt.textContent = val;
                selectEl.appendChild(opt);
            });
            selectEl.disabled = false;
        }

        function uniqSort(list) {
            return [...new Set(list.map((x) => String(x).trim()).filter(Boolean))].sort((a, b) =>
                a.localeCompare(b, "es")
            );
        }

        async function getJson(url) {
            const res = await fetch(url, { headers: { Accept: "application/json" }, cache: "no-store" });
            const data = await res.json().catch(() => null);
            if (!res.ok) throw { status: res.status, data };
            return data;
        }

        function normalizeApiPayload(data) {
            if (Array.isArray(data)) {
                return { colonias: uniqSort(data), estado: "", municipio: "" };
            }
            const colonias = uniqSort(data?.colonias ?? data?.colonia ?? []);
            const estado = (data?.estado ?? "").toString().trim();
            const municipio = (data?.municipio ?? "").toString().trim();
            return { colonias, estado, municipio };
        }

        async function loadByCP(cp) {
            lastCpRequested = cp;
            showError(false);

            coloniaSelect.disabled = true;
            coloniaSelect.innerHTML = `<option value="">Cargando colonias...</option>`;

            try {
                const url = `${API_BASE}/${encodeURIComponent(cp)}`;
                const data = await getJson(url);
                const { colonias, estado, municipio } = normalizeApiPayload(data);

                if (cp !== lastCpRequested) return;

                if (!colonias.length) {
                    resetSelect(coloniaSelect, "Sin selección");
                    showError(true);
                    return;
                }

                fillSelect(coloniaSelect, colonias, "Sin selección");
                showError(false);

                if (estadoSelect && estado) {
                    const exists = Array.from(estadoSelect.options).some(
                        (o) => (o.value || "").trim().toLowerCase() === estado.toLowerCase()
                    );
                    if (!exists) {
                        const opt = document.createElement("option");
                        opt.value = estado;
                        opt.textContent = estado;
                        estadoSelect.appendChild(opt);
                    }
                    estadoSelect.value = estado;
                }

                if (municipioSelect && municipio) {
                    municipioSelect.disabled = false;
                    const exists = Array.from(municipioSelect.options).some(
                        (o) => (o.value || "").trim().toLowerCase() === municipio.toLowerCase()
                    );
                    if (!exists) {
                        const opt = document.createElement("option");
                        opt.value = municipio;
                        opt.textContent = municipio;
                        municipioSelect.appendChild(opt);
                    }
                    municipioSelect.value = municipio;
                }
            } catch (err) {
                if (cp !== lastCpRequested) return;
                resetSelect(coloniaSelect, "Sin selección");
                showError(true);
                console.error("CP API propia error:", err);
            }
        }

        cpInput.addEventListener("input", () => {
            const cp = normalizeCP(cpInput.value);
            if (cpInput.value !== cp) cpInput.value = cp;

            showError(false);

            if (cp.length < 5) {
                lastCpRequested = "";
                resetSelect(coloniaSelect, "Sin selección");
                return;
            }

            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => loadByCP(cp), 300);
        });

        resetSelect(coloniaSelect, "Sin selección");
    }

    // ========= Init =========
    onReady(() => {
        // Inputs
        soloTelefono($("domTelefono"));
        soloTelefono($("tutorTelCasa"));
        soloTelefono($("tutorTelTrabajo"));

        soloNumeros($("domNumExt"));
        soloNumeros($("domNumInt"));
        soloNumeros($("domCP"));

        // Validation
        initBootstrapValidation();

        // Charts
        initLineChart();
        initPieChart();
        initBarChart();

        // Toggles base
        toggleByRadios({
            radioOn: $("matriculaSi"),
            radioOff: $("matriculaNo"),
            target: $("campoMatricula"),
        });

        toggleByRadios({
            radioOn: $("trabajaSi"),
            radioOff: $("trabajaNo"),
            target: $("camposTrabajo"),
        });

        // Toggles extra
        initTogglesExtra();

        // Estados/Municipios (pares)
        const mx = createMXLoader();
        mx.initPair({ estadoId: "domEstado", municipioId: "domMunicipio", logLabel: "Domicilio" });
        mx.initPair({ estadoId: "escEstado", municipioId: "escMunicipio", logLabel: "Datos Escolares" });

        // Wizard
        initWizardSteps();

        // CP API
        initCPApi();
    });
})();