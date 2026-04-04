// js/alumno-scripts.js
(() => {
  "use strict";

  // =========================
  // A) EDITAR / GUARDAR / CANCELAR
  // =========================
  function initEdicionAlumno() {
    const btnEditar = document.getElementById("btn-editar");
    const btnGuardar = document.getElementById("btn-guardar");
    const btnCancelar = document.getElementById("btn-cancelar");
    const editableFields = document.querySelectorAll(".editable-field");

    if (!btnEditar || !btnGuardar || !btnCancelar || !editableFields.length) return;

    let originalValues = new Map();

    const saveOriginalValues = () => {
      originalValues.clear();
      editableFields.forEach(el => {
        if (el.type === "radio") originalValues.set(el, el.checked);
        else originalValues.set(el, el.value);
      });
    };

    const restoreOriginalValues = () => {
      editableFields.forEach(el => {
        if (!originalValues.has(el)) return;
        if (el.type === "radio") el.checked = originalValues.get(el);
        else el.value = originalValues.get(el);
      });
    };

    const setEditable = (enabled) => {
      editableFields.forEach(el => (el.disabled = !enabled));
      btnEditar.classList.toggle("d-none", enabled);
      btnGuardar.classList.toggle("d-none", !enabled);
      btnCancelar.classList.toggle("d-none", !enabled);
    };

    function syncCardFromForm() {
      const get = (name) => document.querySelector(`[name="${name}"]`);

      const nombre = get("nombre")?.value ?? "";
      const apP = get("ap_paterno")?.value ?? "";
      const apM = get("ap_materno")?.value ?? "";
      const matricula = get("matricula")?.value ?? "";
      const tipo = get("tipo")?.value ?? "";

      const cardNombre = document.getElementById("card-nombre");
      const cardMatricula = document.getElementById("card-matricula");
      const cardTipo = document.getElementById("card-tipo");

      if (cardNombre) cardNombre.textContent = `${nombre} ${apP} ${apM}`.trim() || "Alumno";
      if (cardMatricula) cardMatricula.textContent = matricula || "-";
      if (cardTipo) cardTipo.textContent = tipo || "Alumno";
    }

    setEditable(false);
    syncCardFromForm();

    btnEditar.addEventListener("click", () => {
      saveOriginalValues();
      setEditable(true);
    });

    btnCancelar.addEventListener("click", () => {
      restoreOriginalValues();
      setEditable(false);
      syncCardFromForm();
    });

    btnGuardar.addEventListener("click", () => {
      setEditable(false);
      syncCardFromForm();
    });
  }

  // =========================
  // B) DOCUMENTOS
  // =========================
  function initDocumentosAlumno() {
    const tbodyEnt = document.getElementById("tbodyEntregados");
    const tbodyPen = document.getElementById("tbodyPorEntregar");
    if (!tbodyEnt || !tbodyPen) return;

    const MAX_MB = 3;
    const MAX_BYTES = MAX_MB * 1024 * 1024;
    const API_BASE = "/api/documentos";
    const USE_MOCK = true;

    const DOCS_REQUERIDOS = [
      { id: "fotos", nombre: "6 FOTOS T/INFANTIL PAPEL MATE" },
      { id: "pago_examen", nombre: "PAGO DE EXAMEN DE ADMISIÓN" },
      { id: "acta", nombre: "ACTA DE NACIMIENTO (ACTUALIZADO)" },
      { id: "curp", nombre: "CURP (ACTUALIZADO)" },
      { id: "cert_sec", nombre: "CERTIFICADO DE SECUNDARIA" },
      { id: "comp_dom", nombre: "COMPROBANTE DE DOMICILIO (3 MESES)" },
      { id: "conducta", nombre: "CARTA DE BUENA CONDUCTA" },
    ];

    const $ = (id) => document.getElementById(id);

    function getUsuarioActual() {
      return "ADMIN";
    }

    function getFolioActual() {
      const params = new URLSearchParams(window.location.search);
      const id = params.get("id");
      if (id) return id;

      const matriculaInput = document.querySelector('[name="matricula"]');
      if (matriculaInput && matriculaInput.value) return matriculaInput.value;

      return localStorage.getItem("preinscripcion_folio") || "SIN_FOLIO";
    }

    function hoyDMY() {
      const d = new Date();
      return `${String(d.getDate()).padStart(2, "0")}/${String(d.getMonth() + 1).padStart(2, "0")}/${d.getFullYear()}`;
    }

    const api = {
      async getEstado({ folio }) {
        if (USE_MOCK) return mock.getEstado({ folio });
        const r = await fetch(`${API_BASE}?folio=${encodeURIComponent(folio)}`);
        if (!r.ok) throw new Error("GET estado falló");
        return r.json();
      },

      async upload({ folio, docId, file }) {
        if (USE_MOCK) return mock.upload({ folio, docId, file });

        const fd = new FormData();
        fd.append("folio", folio);
        fd.append("docId", docId);
        fd.append("file", file);

        const r = await fetch(`${API_BASE}/upload`, { method: "POST", body: fd });
        if (!r.ok) throw new Error("Upload falló");
        return r.json();
      },

      async download({ folio, docId }) {
        if (USE_MOCK) return mock.download({ folio, docId });
        const url = `${API_BASE}/${docId}/download?folio=${encodeURIComponent(folio)}`;
        window.open(url, "_blank");
      },

      async deleteDoc({ folio, docId }) {
        if (USE_MOCK) return mock.deleteDoc({ folio, docId });

        const r = await fetch(`${API_BASE}/${docId}?folio=${encodeURIComponent(folio)}`, {
          method: "DELETE"
        });
        if (!r.ok) throw new Error("Delete falló");
      }
    };

    const mock = {
      META_KEY: "docs_meta_v2",
      DB_NAME: "preins_docs_db",
      STORE: "files",

      loadMeta() {
        return JSON.parse(localStorage.getItem(this.META_KEY) || "{}");
      },

      saveMeta(meta) {
        localStorage.setItem(this.META_KEY, JSON.stringify(meta));
      },

      async openDB() {
        return new Promise((resolve, reject) => {
          const req = indexedDB.open(this.DB_NAME, 1);
          req.onupgradeneeded = () => {
            const db = req.result;
            if (!db.objectStoreNames.contains(this.STORE)) {
              db.createObjectStore(this.STORE);
            }
          };
          req.onsuccess = () => resolve(req.result);
          req.onerror = () => reject(req.error);
        });
      },

      async putFile(key, blob) {
        const db = await this.openDB();
        return new Promise((resolve, reject) => {
          const tx = db.transaction(this.STORE, "readwrite");
          tx.objectStore(this.STORE).put(blob, key);
          tx.oncomplete = () => resolve();
          tx.onerror = () => reject(tx.error);
        });
      },

      async getFile(key) {
        const db = await this.openDB();
        return new Promise((resolve, reject) => {
          const tx = db.transaction(this.STORE, "readonly");
          const req = tx.objectStore(this.STORE).get(key);
          req.onsuccess = () => resolve(req.result || null);
          req.onerror = () => reject(req.error);
        });
      },

      async deleteFile(key) {
        const db = await this.openDB();
        return new Promise((resolve, reject) => {
          const tx = db.transaction(this.STORE, "readwrite");
          tx.objectStore(this.STORE).delete(key);
          tx.oncomplete = () => resolve();
          tx.onerror = () => reject(tx.error);
        });
      },

      async getEstado({ folio }) {
        const meta = this.loadMeta();
        return DOCS_REQUERIDOS.map(d => {
          const k = `${folio}:${d.id}`;
          const m = meta[k];
          if (!m) return { docId: d.id, status: "pendiente" };
          return {
            docId: d.id,
            status: "entregado",
            uploadedBy: m.uploadedBy,
            deliveredAt: m.deliveredAt,
            fileName: m.fileName
          };
        });
      },

      async upload({ folio, docId, file }) {
        const meta = this.loadMeta();
        const key = `${folio}:${docId}`;

        await this.putFile(key, file);

        meta[key] = {
          uploadedBy: getUsuarioActual(),
          deliveredAt: hoyDMY(),
          fileName: file.name
        };

        this.saveMeta(meta);
      },

      async download({ folio, docId }) {
        const key = `${folio}:${docId}`;
        const blob = await this.getFile(key);
        if (!blob) return;

        const url = URL.createObjectURL(blob);
        window.open(url, "_blank");
        setTimeout(() => URL.revokeObjectURL(url), 60000);
      },

      async deleteDoc({ folio, docId }) {
        const meta = this.loadMeta();
        const key = `${folio}:${docId}`;
        delete meta[key];
        this.saveMeta(meta);
        await this.deleteFile(key);
      }
    };

    function escapeHtml(str) {
      return String(str || "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;");
    }

    function rerenderFeather() {
      if (window.feather && typeof window.feather.replace === "function") {
        window.feather.replace();
      }
    }

    function renderFromEstado(estado) {
      const tbodyEntregados = $("tbodyEntregados");
      const tbodyPorEntregar = $("tbodyPorEntregar");

      tbodyEntregados.innerHTML = "";
      tbodyPorEntregar.innerHTML = "";

      const byId = new Map(estado.map(x => [x.docId, x]));
      const folio = getFolioActual();

      for (const doc of DOCS_REQUERIDOS) {
        const item = byId.get(doc.id);

        if (!item || item.status !== "entregado") {
          const tr = document.createElement("tr");
          tr.innerHTML = `
            <td class="text-break">${escapeHtml(doc.nombre)}</td>
            <td>
              <input type="file"
                     class="form-control form-control-sm"
                     accept="application/pdf"
                     data-doc-id="${escapeHtml(doc.id)}">
              <div class="small text-muted mt-1">PDF (Máx ${MAX_MB}MB)</div>
            </td>
          `;
          tbodyPorEntregar.appendChild(tr);
        } else {
          const tr = document.createElement("tr");
          tr.innerHTML = `
            <td class="text-break">${escapeHtml(doc.nombre)}</td>
            <td>${escapeHtml(item.uploadedBy)}</td>
            <td>${escapeHtml(item.deliveredAt)}</td>
            <td></td>
            <td>
              <a href="#" data-open-doc="${escapeHtml(doc.id)}">
                ${escapeHtml(item.fileName || "Ver documento")}
              </a>
            </td>
            <td class="text-center">
              <button class="btn btn-sm btn-outline-danger" type="button" data-delete-doc="${escapeHtml(doc.id)}" title="Eliminar">
                <i data-feather="trash-2"></i>
              </button>
            </td>
          `;
          tbodyEntregados.appendChild(tr);
        }
      }

      // Upload
      tbodyPorEntregar.querySelectorAll("[data-doc-id]").forEach(inp => {
        inp.addEventListener("change", async e => {
          const file = e.target.files?.[0];
          const docId = e.target.dataset.docId;
          if (!file) return;

          if (!file.name.toLowerCase().endsWith(".pdf")) {
            alert("Solo PDF.");
            e.target.value = "";
            return;
          }
          if (file.size > MAX_BYTES) {
            alert(`Máx ${MAX_MB}MB.`);
            e.target.value = "";
            return;
          }

          await api.upload({ folio, docId, file });
          const nuevoEstado = await api.getEstado({ folio });
          renderFromEstado(nuevoEstado);
        });
      });

      // Download
      tbodyEntregados.querySelectorAll("[data-open-doc]").forEach(a => {
        a.addEventListener("click", e => {
          e.preventDefault();
          api.download({ folio, docId: a.dataset.openDoc });
        });
      });

      // Delete
      tbodyEntregados.querySelectorAll("[data-delete-doc]").forEach(btn => {
        btn.addEventListener("click", async () => {
          if (!confirm("¿Eliminar documento?")) return;
          await api.deleteDoc({ folio, docId: btn.dataset.deleteDoc });
          const nuevoEstado = await api.getEstado({ folio });
          renderFromEstado(nuevoEstado);
        });
      });

      rerenderFeather();
    }

    // Botón "Siguiente"
    const btnContinuar = document.getElementById("btnContinuar");
    if (btnContinuar) {
      btnContinuar.addEventListener("click", () => {
        alert("Siguiente (aquí pon tu ruta).");
      });
    }

    // Inicializar docs
    (async () => {
      try {
        const folio = getFolioActual();
        const estado = await api.getEstado({ folio });
        renderFromEstado(estado);
      } catch (err) {
        console.error(err);
        alert("Error cargando documentos.");
      }
    })();
  }

  // =========================
  // INIT ÚNICO
  // =========================
  document.addEventListener("DOMContentLoaded", () => {
    initEdicionAlumno();
    initDocumentosAlumno();
  });

})();