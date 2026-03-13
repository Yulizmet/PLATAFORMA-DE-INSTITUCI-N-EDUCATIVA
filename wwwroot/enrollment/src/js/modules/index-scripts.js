// index-scripts.js (COMPLETO Y CORRECTO)
// Mantiene TODAS tus funciones importantes (incluida Nacionalidad Mexican -> Estado/Municipio nacimiento)
// y además guarda preinscripcion_activa al darle "Siguiente" (#btnContinuar) para el flujo:
// index.html -> documentos.html -> alumno.html(confirmación)

(() => {
    "use strict";

    // ==========================
    // Config
    // ==========================
    const LS_KEY = "preinscripcion_activa";

    // ==========================
    // Helpers base
    // ==========================
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

    function show(el) {
        if (!el) return;
        el.classList.remove("d-none");
    }

    function hide(el) {
        if (!el) return;
        el.classList.add("d-none");
    }

    function soloTelefono(input) {
        if (!input) return;
        input.addEventListener("input", () => {
            input.value = (input.value || "").replace(/\D/g, "").slice(0, 10);
        });
    }

    function soloNumeros(input) {
        if (!input) return;
        input.addEventListener("input", () => {
            input.value = (input.value || "").replace(/\D/g, "");
        });
    }

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

    // ==========================
    // Charts (si existen libs)
    // ==========================
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
                datasets: [
                    {
                        label: "Sales ($)",
                        fill: true,
                        backgroundColor: gradient,
                        borderColor: window.theme?.primary,
                        data: [2115, 1562, 1584, 1892, 1587, 1923, 2566, 2448, 2805, 3438, 2917, 3327],
                    },
                ],
            },
            options: {
                maintainAspectRatio: false,
                legend: { display: false },
                tooltips: { intersect: false },
                hover: { intersect: true },
                plugins: { filler: { propagate: false } },
                scales: {
                    xAxes: [{ reverse: true, gridLines: { color: "rgba(0,0,0,0.0)" } }],
                    yAxes: [
                        {
                            ticks: { stepSize: 1000 },
                            display: true,
                            borderDash: [3, 3],
                            gridLines: { color: "rgba(0,0,0,0.0)" },
                        },
                    ],
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
                datasets: [
                    {
                        data: [4306, 3801, 1689],
                        backgroundColor: [window.theme?.primary, window.theme?.warning, window.theme?.danger],
                        borderWidth: 5,
                    },
                ],
            },
            options: {
                responsive: !window.MSInputMethodContext,
                maintainAspectRatio: false,
                legend: { display: false },
                cutoutPercentage: 75,
            },
        });
    }

    // ==========================
    // Radios toggle
    // ==========================
    function toggleByRadios({ radioOn, radioOff, target, hiddenClass = "d-none", onHideClearIds = [] }) {
        if (!radioOn || !radioOff || !target) return;

        const doShow = () => target.classList.remove(hiddenClass);
        const doHide = () => {
            target.classList.add(hiddenClass);
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

    function initTogglesExtra() {
        // Beca
        toggleByRadios({
            radioOn: $("becaSi"),
            radioOff: $("becaNo"),
            target: $("wrapTipoBeca"),
            onHideClearIds: ["tipoBeca"],
        });

        // Lengua indígena
        toggleByRadios({
            radioOn: $("lenguaSi"),
            radioOff: $("lenguaNo"),
            target: $("wrapLenguaCual"),
            onHideClearIds: ["lenguaCual"],
        });

        // Discapacidad
        toggleByRadios({
            radioOn: $("discapacidadSi"),
            radioOff: $("discapacidadNo"),
            target: $("wrapDiscapEspec"),
            onHideClearIds: ["discapEspec"],
        });

        // Enfermedad
        toggleByRadios({
            radioOn: $("enfermedadSi"),
            radioOff: $("enfermedadNo"),
            target: $("wrapEnfEspec"),
            onHideClearIds: ["enfEspec"],
        });
    }

    // ==========================
    // 1) TU FUNCIÓN CLAVE: Nacionalidad -> Estado/Municipio de nacimiento
    // (la que dijiste que te quité)
    // ==========================
    function initMexicoEstadosMunicipios() {
        const MX_JSON_URLS = [
            "https://raw.githubusercontent.com/htmike/Estados-y-municipios-MX/master/municipios-341c9-export.json",
            "https://cdn.jsdelivr.net/gh/htmike/Estados-y-municipios-MX@master/municipios-341c9-export.json",
        ];
        const FETCH_TIMEOUT_MS = 12000;

        let dataLoaded = false;
        let estados = [];
        let initialized = false;

        function toTitleCase(text) {
            return String(text)
                .toLowerCase()
                .replace(/\b\p{L}/gu, (c) => c.toUpperCase());
        }

        function resetMX(estadoSel, muniSel) {
            estadoSel.innerHTML = '<option value="">Selecciona un estado</option>';
            muniSel.innerHTML = '<option value="">Selecciona un municipio</option>';
            muniSel.disabled = true;
        }

        function isMexicoSelected(paisSel) {
            // tu criterio original
            return (paisSel.value || "").trim() === "Mexican";
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

        async function loadMXOnce(loadingEl) {
            if (dataLoaded) return;

            if (loadingEl) loadingEl.classList.remove("d-none");

            let lastErr = null;
            try {
                for (const url of MX_JSON_URLS) {
                    try {
                        const raw = await fetchWithTimeout(url, FETCH_TIMEOUT_MS);
                        const arr = Array.isArray(raw) ? raw : Object.values(raw);

                        estados = arr
                            .map((e) => ({
                                nombre: toTitleCase(e.nombre ?? e.nom_ent ?? e.name ?? ""),
                                municipios: Array.isArray(e.municipios) ? e.municipios.map((m) => toTitleCase(m)) : [],
                            }))
                            .filter((e) => e.nombre);

                        estados.sort((a, b) => a.nombre.localeCompare(b.nombre, "es"));
                        dataLoaded = true;
                        return;
                    } catch (err) {
                        lastErr = err;
                    }
                }
                throw lastErr || new Error("No se pudo cargar el JSON de estados/municipios.");
            } finally {
                if (loadingEl) loadingEl.classList.add("d-none");
            }
        }

        function fillEstados(estadoSel, muniSel) {
            resetMX(estadoSel, muniSel);
            estados.forEach((e) => {
                const opt = document.createElement("option");
                opt.value = e.nombre;
                opt.textContent = e.nombre;
                estadoSel.appendChild(opt);
            });
        }

        function fillMunicipios(estadoNombre, muniSel) {
            muniSel.innerHTML = '<option value="">Selecciona un municipio</option>';
            muniSel.disabled = true;

            const estado = estados.find((e) => e.nombre === estadoNombre);
            if (!estado) return;

            estado.municipios
                .slice()
                .sort((a, b) => a.localeCompare(b, "es"))
                .forEach((m) => {
                    const opt = document.createElement("option");
                    opt.value = m;
                    opt.textContent = m;
                    muniSel.appendChild(opt);
                });

            muniSel.disabled = false;
        }

        function ensureErrorUI(card) {
            let wrap = $("mxErrorWrap");
            if (!wrap) {
                wrap = document.createElement("div");
                wrap.id = "mxErrorWrap";
                wrap.className = "col-12 d-none";
                wrap.innerHTML = '<small id="mxErrorSmall" class="text-danger"></small>';
                card.querySelector(".card-body")?.appendChild(wrap);
            }
            return { wrap: $("mxErrorWrap"), small: $("mxErrorSmall") };
        }

        async function handlePaisChange(paisSel, card, estadoSel, muniSel, loadingEl, errUI) {
            if (isMexicoSelected(paisSel)) {
                card.classList.remove("d-none");
                errUI.wrap.classList.add("d-none");

                try {
                    await loadMXOnce(loadingEl);
                    fillEstados(estadoSel, muniSel);

                    if (estadoSel.options.length <= 1) {
                        errUI.small.textContent = "No se recibieron estados desde la fuente.";
                        errUI.wrap.classList.remove("d-none");
                    }
                } catch (err) {
                    console.error("Error cargando estados/municipios:", err);
                    resetMX(estadoSel, muniSel);
                    errUI.small.textContent = "No se pudieron cargar estados/municipios.";
                    errUI.wrap.classList.remove("d-none");
                }
            } else {
                card.classList.add("d-none");
                resetMX(estadoSel, muniSel);
                errUI.wrap.classList.add("d-none");
            }
        }

        function init() {
            if (initialized) return;
            initialized = true;

            // IDs EXACTOS que tú usas
            const paisSel = $("nacionalidad");
            const card = $("lugarNacimientoCard");
            const estadoSel = $("estadoNacimiento");
            const muniSel = $("municipioNacimiento");
            const loadingEl = $("mxLoading");

            if (!paisSel || !card || !estadoSel || !muniSel) return;

            const errUI = ensureErrorUI(card);

            const onChange = () => handlePaisChange(paisSel, card, estadoSel, muniSel, loadingEl, errUI);

            paisSel.addEventListener("change", onChange);
            paisSel.addEventListener("input", onChange);
            estadoSel.addEventListener("change", () => fillMunicipios(estadoSel.value, muniSel));

            onChange();
        }

        init();
    }

    // ==========================
    // 2) Loader MX para pares (domicilio / escolares)
    // ==========================
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
                                municipios: Array.isArray(e.municipios) ? e.municipios.map((m) => toTitleCase(m)) : [],
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

    // ==========================
    // Wizard steps
    // ==========================
    function initWizardSteps() {
        const wizard = document.querySelector("#wizardSteps");
        const steps = document.querySelectorAll("#wizardSteps .step-vertical");
        if (!wizard || !steps.length) return;

        const stepByPage = {
            "index.html": 0,
            "documentos.html": 1,
            "alumno.html": 2,
            "finalizar.html": 3,
        };

        const path = window.location.pathname;
        const page = (path.split("/").pop() || "index.html").toLowerCase();
        if (!(page in stepByPage)) return;

        const pageStep = stepByPage[page];

        const hasActivo = !!localStorage.getItem(LS_KEY);
        const enviado = localStorage.getItem("preinscripcion_enviada") === "1";
        const maxStep = enviado ? 3 : hasActivo ? 2 : 0;

        steps.forEach((el) => {
            const step = parseInt(el.dataset.step, 10);
            const href = el.dataset.href;

            el.classList.remove("active", "complete", "locked");
            el.classList.toggle("active", step === pageStep);
            el.classList.toggle("complete", step < pageStep);

            const isForward = step > pageStep;
            const isBeyondMax = step > maxStep;

            if (isForward || isBeyondMax) {
                el.classList.add("locked");
            }

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

    // ==========================
    // CP API propia
    // ==========================
    function initCPApi() {
        const API_BASE = "https://hola273115.bsite.net/api/CodigosPostales";

        const cpInput = $("domCP");
        const coloniaSelect = $("domColonia");
        const cpError = $("domCPError");

        const estadoSelect = $("domEstado");
        const municipioSelect = $("domMunicipio");

        // Solo estos son realmente obligatorios
        if (!cpInput || !coloniaSelect) return;

        let debounceTimer = null;
        let lastCpRequested = "";

        const normalizeCP = (v) => (v || "").replace(/\D/g, "").slice(0, 5);

        function showError(showIt) {
            if (!cpError) return;
            cpError.classList.toggle("d-none", !showIt);
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
            return [...new Set((list || []).map((x) => String(x).trim()).filter(Boolean))]
                .sort((a, b) => a.localeCompare(b, "es"));
        }

        async function getJson(url) {
            const res = await fetch(url, {
                method: "GET",
                headers: { Accept: "application/json" },
                cache: "no-store"
            });

            const text = await res.text();
            let data = null;

            try {
                data = text ? JSON.parse(text) : null;
            } catch {
                data = text;
            }

            if (!res.ok) {
                throw { status: res.status, data };
            }

            return data;
        }

        function normalizeApiPayload(data) {
            console.log("Respuesta API CP:", data);

            // Caso 1: la API regresa un array directo
            if (Array.isArray(data)) {
                return {
                    colonias: uniqSort(data),
                    estado: "",
                    municipio: ""
                };
            }

            // Caso 2: la API regresa objeto
            return {
                colonias: uniqSort(
                    data?.colonias ||
                    data?.colonia ||
                    data?.Colonias ||
                    data?.Colonia ||
                    []
                ),
                estado: String(
                    data?.estado ||
                    data?.Estado ||
                    ""
                ).trim(),
                municipio: String(
                    data?.municipio ||
                    data?.Municipio ||
                    ""
                ).trim()
            };
        }

        async function loadByCP(cp) {
            lastCpRequested = cp;
            showError(false);

            coloniaSelect.disabled = true;
            coloniaSelect.innerHTML = `<option value="">Cargando colonias...</option>`;

            try {
                const url = `${API_BASE}/${encodeURIComponent(cp)}`;
                console.log("Consultando CP:", url);

                const data = await getJson(url);
                const { colonias, estado, municipio } = normalizeApiPayload(data);

                if (cp !== lastCpRequested) return;

                if (!colonias.length) {
                    resetSelect(coloniaSelect, "Sin selección");
                    showError(true);
                    return;
                }

                fillSelect(coloniaSelect, colonias, "Selecciona una colonia");
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

            if (cpInput.value !== cp) {
                cpInput.value = cp;
            }

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

    // Asegúrate de llamarla cuando cargue el DOM
    document.addEventListener("DOMContentLoaded", initCPApi);
    // ==========================
    // Guardado en LocalStorage + continuar
    // ==========================
    function getVal(...ids) {
        for (const id of ids) {
            const el = $(id);
            if (!el) continue;
            const v = (el.value ?? "").toString().trim();
            if (v !== "") return v;
        }
        return "";
    }

    function readPayload() {
        try {
            return JSON.parse(localStorage.getItem(LS_KEY) || "null");
        } catch {
            return null;
        }
    }

    function savePayload(payload) {
        localStorage.setItem(LS_KEY, JSON.stringify(payload));
    }

    function bindContinuar() {
        const btn = $("btnContinuar");
        if (!btn) return;

        btn.addEventListener("click", (e) => {
            e.preventDefault();

            // Si tu form usa submit y quieres validación nativa:
            // puedes checar .checkValidity() aquí si tienes el id del form.
            // Por ahora guardamos lo que haya.

            const payload = readPayload() || {};
            payload.datos = payload.datos || {};
            payload.documentos = payload.documentos || {};

            // ======================
            // IMPORTANTE:
            // Aquí debes mapear con TUS IDs reales.
            // Yo uso nombres "comunes". Si alguno no existe, queda vacío.
            // ======================

            payload.datos.generales = {
                apPaterno: getVal("apPaterno", "apellidoPaterno"),
                apMaterno: getVal("apMaterno", "apellidoMaterno"),
                nombres: getVal("nombres", "nombre"),
                genero: getVal("genero"),
                fechaNacimiento: getVal("fechaNacimiento"),
                estadoCivil: getVal("estadoCivil"),
                nacionalidad: getVal("nacionalidad"),
                curp: getVal("curp"),
                tipoSangre: getVal("tipoSangre", "tipo_de_sangre", "tipoDeSangre"),
                estadoNacimiento: getVal("estadoNacimiento"),
                municipioNacimiento: getVal("municipioNacimiento"),
            };

            payload.datos.domicilio = {
                telefono: getVal("domTelefono"),
                calle: getVal("domCalle"),
                numExt: getVal("domNumExt"),
                numInt: getVal("domNumInt"),
                cp: getVal("domCP"),
                colonia: getVal("domColonia"),
                estado: getVal("domEstado"),
                municipio: getVal("domMunicipio"),
                email: getVal("domEmail", "email"),
            };

            payload.datos.tutor = {
                parentesco: getVal("tutorParentesco"),
                apPaterno: getVal("tutorApPaterno"),
                apMaterno: getVal("tutorApMaterno"),
                nombres: getVal("tutorNombres"),
                telCasa: getVal("tutorTelCasa"),
                telTrabajo: getVal("tutorTelTrabajo"),
            };

            payload.datos.escolares = {
                procedencia: getVal("escProcedencia", "escuelaProcedencia"),
                carrera: getVal("escCarrera", "carreraCursada"),
                estado: getVal("escEstado"),
                municipio: getVal("escMunicipio"),
                promedio: getVal("escPromedio"),
                fechaInicio: getVal("escFechaInicio"),
                fechaFin: getVal("escFechaFin"),
                sistema: getVal("escSistema"),
                tipoPrepa: getVal("escTipoPrepa"),
            };

            payload.datos.otros = {
                beca: $("becaSi")?.checked ? "Sí" : $("becaNo")?.checked ? "No" : getVal("beca"),
                tipoBeca: getVal("tipoBeca"),

                origenIndigena: getVal("origenIndigena"),

                lenguaIndigena: $("lenguaSi")?.checked ? "Sí" : $("lenguaNo")?.checked ? "No" : getVal("lenguaIndigena"),
                lenguaCual: getVal("lenguaCual"),

                discapacidad: $("discapacidadSi")?.checked ? "Sí" : $("discapacidadNo")?.checked ? "No" : getVal("discapacidad"),
                discapEspec: getVal("discapEspec"),

                enfermedad: $("enfermedadSi")?.checked ? "Sí" : $("enfermedadNo")?.checked ? "No" : getVal("enfermedad"),
                enfEspec: getVal("enfEspec"),
            };

            // ✅ Arranca expiración de 5 minutos a partir de aquí
            payload._createdAt = Date.now();

            savePayload(payload);

            // 👉 paso siguiente
            window.location.href = "documentos.html";
        });
    }

    // ==========================
    // INIT
    // ==========================
    onReady(() => {
        // Inputs
        soloTelefono($("domTelefono"));
        soloTelefono($("tutorTelCasa"));
        soloTelefono($("tutorTelTrabajo"));

        soloNumeros($("domNumExt"));
        soloNumeros($("domNumInt"));
        soloNumeros($("domCP"));

        // Bootstrap validation
        initBootstrapValidation();

        // Charts
        initLineChart();
        initPieChart();

        // Toggles generales (los que tenías)
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

        initTogglesExtra();

        initMexicoEstadosMunicipios();

        const mx = createMXLoader();
        mx.initPair({ estadoId: "domEstado", municipioId: "domMunicipio", logLabel: "Domicilio" });
        mx.initPair({ estadoId: "escEstado", municipioId: "escMunicipio", logLabel: "Datos Escolares" });

        initCPApi();

        initWizardSteps();

        bindContinuar();
    });
})();