(() => {
  "use strict";

  function setTextByIds(ids, value) {
    const v = (value ?? "").toString().trim();
    ids.forEach((id) => {
      const el = document.getElementById(id);
      if (el) el.textContent = v !== "" ? v : "—";
    });
  }

  function formatFechaEnvio(date = new Date()) {
    const fecha = date.toLocaleDateString("es-MX", {
      day: "2-digit",
      month: "short",
      year: "numeric",
    });

    const hora = date.toLocaleTimeString("es-MX", {
      hour: "numeric",
      minute: "2-digit",
      second: "2-digit",
      hour12: true,
    });

    return `${fecha} - ${hora}`;
  }

  function initWizardSteps() {
    const stepByPage = {
      "index.html": 0,
      "alumno.html": 1,
      "finalizar.html": 2,
    };

    const path = window.location.pathname;
    const page = (path.split("/").pop() || "index.html").toLowerCase();

    if (!(page in stepByPage)) return;

    const current = stepByPage[page];
    const steps = document.querySelectorAll("#wizardSteps .step-vertical");

    steps.forEach((el) => {
      const i = Number(el.dataset.step);
      el.classList.remove("active", "complete");
      if (i < current) el.classList.add("complete");
      if (i === current) el.classList.add("active");
    });
  }

  function generateFolio(prefix = "ITC") {
    const KEY = "itc_folio_counter";
    const WIDTH = 6;

    let counter = parseInt(localStorage.getItem(KEY), 10);
    if (!Number.isInteger(counter) || counter < 1) counter = 1;

    const folio = prefix + String(counter).padStart(WIDTH, "0");
    localStorage.setItem(KEY, String(counter + 1));

    return folio;
  }

  function initFolioITC() {
    const KEY_FOLIO_ACTUAL = "itc_folio_actual";

    let folio = localStorage.getItem(KEY_FOLIO_ACTUAL);
    if (!folio) {
      folio = generateFolio("ITC");
      localStorage.setItem(KEY_FOLIO_ACTUAL, folio);
    }

    setTextByIds(["cpFolio", "folioGenerado"], folio);
  }

  function initFechaEnvio() {
    const KEY_FECHA_ENVIO = "itc_fecha_envio";

    const fechaEnvio = localStorage.getItem(KEY_FECHA_ENVIO);
    if (!fechaEnvio) {
      setTextByIds(["fechaEnvio", "cpFechaEnvio"], "—");
      return;
    }

    setTextByIds(["fechaEnvio", "cpFechaEnvio"], fechaEnvio);
  }

  function leerPreinscripcionLS() {
    // Usa la misma key que tu storage.js
    const LS_KEY = "preinscripcion_activa";
    try {
      const raw = localStorage.getItem(LS_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch (e) {
      console.error("Error leyendo LocalStorage:", e);
      return null;
    }
  }

  function initNombreAspirante() {
    const payload = leerPreinscripcionLS();

    const g = payload?.datos?.generales || {};
    const nombreCompleto = [
      (g.nombres || "").trim(),
      (g.apPaterno || "").trim(),
      (g.apMaterno || "").trim(),
    ]
      .filter(Boolean)
      .join(" ");

    setTextByIds(["cpNombre"], nombreCompleto);
  }

  function initBtnEnviar() {
    const btn = document.getElementById("btnEnviar");
    if (!btn) return;

    btn.addEventListener("click", () => {
      const KEY_FECHA_ENVIO = "itc_fecha_envio";

      if (!localStorage.getItem(KEY_FECHA_ENVIO)) {
        localStorage.setItem(KEY_FECHA_ENVIO, formatFechaEnvio(new Date()));
      }

      localStorage.setItem("preinscripcion_enviada", "1");

      alert("Solicitud enviada correctamente");
    });
  }

  function initNuevoRegistroSiAplica() {
    const path = window.location.pathname;
    const page = (path.split("/").pop() || "index.html").toLowerCase();

    const enviado = localStorage.getItem("preinscripcion_enviada") === "1";

    if (page === "index.html" && enviado) {
      localStorage.removeItem("itc_folio_actual");
      localStorage.removeItem("itc_fecha_envio");
      localStorage.removeItem("preinscripcion_enviada");
    }
  }

  document.addEventListener("DOMContentLoaded", () => {
    initNuevoRegistroSiAplica();
    initWizardSteps();
    initFolioITC();
    initFechaEnvio();
    initNombreAspirante(); 
    initBtnEnviar();
  });
})();
