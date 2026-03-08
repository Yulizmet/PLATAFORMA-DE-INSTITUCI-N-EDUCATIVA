 // documentos-scripts.js
(() => {
    "use strict";

    const LS_GENERAL_KEY = "preenrollment_general";
    const LS_DOCS_KEY = "preenrollment_docs";

    const tbodyEntregados = document.getElementById("tbodyEntregados");
    const tbodyPorEntregar = document.getElementById("tbodyPorEntregar");
    const btnContinuar = document.getElementById("btnContinuar");

    const DOCS = [
        { key: "Fotos", label: "Fotos" },
        { key: "PagoExamen", label: "Pago de examen" },
        { key: "ActaNacimiento", label: "Acta de nacimiento" },
        { key: "Curp", label: "CURP" },
        { key: "Certificados", label: "Certificados" },
        { key: "ComprobanteDomicilio", label: "Comprobante de domicilio" },
        { key: "CartaBuenaConducta", label: "Carta de buena conducta" }
    ];

    function safeParse(json, fallback) {
        try { return JSON.parse(json); } catch { return fallback; }
    }

    function getGeneral() {
        return safeParse(localStorage.getItem(LS_GENERAL_KEY), null);
    }

    // ✅ Incluye Files para guardar nombre del archivo
    function emptyDocs() {
        return {
            IdData: 0,
            Fotos: false,
            PagoExamen: false,
            ActaNacimiento: false,
            Curp: false,
            Certificados: false,
            ComprobanteDomicilio: false,
            CartaBuenaConducta: false,
            Files: {}
        };
    }

    function getDocs() {
        const raw = localStorage.getItem(LS_DOCS_KEY);
        if (!raw) return emptyDocs();

        const parsed = safeParse(raw, emptyDocs());
        const base = emptyDocs();

        const merged = { ...base, ...parsed };
        if (!merged.Files || typeof merged.Files !== "object") merged.Files = {};
        return merged;
    }

    function saveDocs(docs) {
        localStorage.setItem(LS_DOCS_KEY, JSON.stringify(docs));
    }

    function todayStr() {
        const d = new Date();
        const pad = (n) => String(n).padStart(2, "0");
        return `${pad(d.getDate())}/${pad(d.getMonth() + 1)}/${d.getFullYear()}`;
    }

    function getUserNameFromGeneral(general) {
        if (!general) return "Aspirante";
        return general.NombreCompleto || general.Nombre || "Aspirante";
    }

    function escapeHtml(str) {
        return String(str || "")
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;");
    }

    // ✅ Por Entregar: input normal
    function buildFileInput(docKey) {
        return `
      <input type="file" class="form-control form-control-sm"
             data-doc="${docKey}" accept=".pdf,.jpg,.jpeg,.png" />
      <small class="text-muted d-block mt-1">PDF</small>
    `;
    }

    // ✅ Entregados: solo nombre (link)
    function buildFileName(docKey, docs) {
        const name = docs.Files?.[docKey];
        if (!name) return `<span class="text-muted">Sin archivo</span>`;
        return `<a href="#" class="text-primary text-decoration-none" data-open-name="${docKey}">${escapeHtml(name)}</a>`;
    }

    function render() {
        const general = getGeneral();
        const docs = getDocs();

        tbodyEntregados.innerHTML = "";
        tbodyPorEntregar.innerHTML = "";

        const usuario = getUserNameFromGeneral(general);
        const fecha = todayStr();

        DOCS.forEach((d) => {
            const isDone = !!docs[d.key];

            if (isDone) {
                const tr = document.createElement("tr");
                tr.innerHTML = `
          <td>${d.label}</td>
          <td>${usuario}</td>
          <td>${fecha}</td>
          <td>—</td>

          <!-- ✅ SOLO nombre -->
          <td>${buildFileName(d.key, docs)}</td>

          <td class="text-center">
            <button type="button" class="btn btn-sm btn-outline-danger"
                    data-action="undo" data-doc="${d.key}" title="Eliminar / marcar como no entregado">
              <i class="fa-solid fa-trash"></i>
            </button>
          </td>
        `;
                tbodyEntregados.appendChild(tr);
            } else {
                const tr = document.createElement("tr");
                tr.innerHTML = `
          <td>${d.label}</td>
          <td>${buildFileInput(d.key)}</td>
        `;
                tbodyPorEntregar.appendChild(tr);
            }
        });

        bindEvents();
    }

    function bindEvents() {
        // ✅ Subir: marcar true y guardar nombre
        document.querySelectorAll('input[type="file"][data-doc]').forEach((inp) => {
            inp.addEventListener("change", (e) => {
                const key = e.target.getAttribute("data-doc");
                const file = e.target.files?.[0];
                if (!key || !file) return;

                const docs = getDocs();
                docs[key] = true;
                docs.Files[key] = file.name;
                saveDocs(docs);
                render();
            });
        });

        // ✅ Click en nombre: solo aviso (porque no guardas archivo real)
        document.querySelectorAll("[data-open-name]").forEach((a) => {
            a.addEventListener("click", (e) => {
                e.preventDefault();
                const key = a.getAttribute("data-open-name");
                const docs = getDocs();
                const name = docs.Files?.[key];
                alert(name ? `Archivo: ${name}\n\n(No se descarga porque no se guarda el archivo real.)` : "Sin archivo");
            });
        });

        // ✅ Eliminar: regresar a Por Entregar
        document.querySelectorAll('button[data-action="undo"][data-doc]').forEach((btn) => {
            btn.addEventListener("click", () => {
                const key = btn.getAttribute("data-doc");
                if (!key) return;

                const docs = getDocs();
                docs[key] = false;
                if (docs.Files) delete docs.Files[key];
                saveDocs(docs);
                render();
            });
        });
    }

    function init() {
        const general = getGeneral();
        if (!general) {
            window.location.href = window.PRE_URLS?.preinscripcion || "/";
            return;
        }

        const docs = getDocs();
        if (!docs.IdData && general.IdData) {
            docs.IdData = general.IdData;
            saveDocs(docs);
        }

        render();

        btnContinuar?.addEventListener("click", () => {
            const docs = getDocs();

            // ✅ faltante si está false O si no tiene nombre de archivo guardado
            const faltantes = DOCS
                .filter(d => docs[d.key] !== true || !docs.Files?.[d.key])
                .map(d => d.label);

            if (faltantes.length > 0) {
                alert("Te faltan documentos por subir:\n\n- " + faltantes.join("\n- "));
                return;
            }

            window.location.href = window.PRE_URLS?.confirmar || "/";
        });
    }

    init();
})();