document.addEventListener("DOMContentLoaded", () => {
  "use strict";

  // ==========================
  // Helpers
  // ==========================
  const $ = (id) => document.getElementById(id);

  function safe(v) {
    const s = (v ?? "").toString().trim();
    return s || "—";
  }

  function setText(id, value) {
    const el = $(id);
    if (!el) return;

    const v = safe(value);

    if (el.tagName === "INPUT") {
      el.value = v === "—" ? "" : v;
      return;
    }

    if (el.tagName === "SELECT") {
      el.value = v === "—" ? "" : v;
      return;
    }

    el.textContent = v;
  }

  function setYesNo(id, value) {
    const v = (value ?? "").toString().trim().toUpperCase();
    setText(id, v === "SI" || v === "NO" ? v : "");
  }

  function toggle(id, show) {
    const el = $(id);
    if (!el) return;
    el.classList.toggle("d-none", !show);
  }

  function onlyDigits10(el) {
    if (!el) return;
    el.addEventListener("input", () => {
      el.value = (el.value || "").replace(/\D/g, "").slice(0, 10);
    });
  }

  window.setText = setText;

  // ==========================
  // Stepper
  // ==========================
  (function initWizardSteps() {
    const stepByPage = { "index.html": 0, "alumno.html": 1, "finalizar.html": 2 };
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
  })();

  // ==========================
  // Editar / Listo por card
  // ==========================
  function wireEditCard(cardId, btnId, onToggle) {
    const card = $(cardId);
    const btn = $(btnId);

    if (!card) console.warn(`[Editar] No existe la card: #${cardId}`);
    if (!btn) console.warn(`[Editar] No existe el botón: #${btnId}`);
    if (!card || !btn) return;

    function apply(enabled) {
      card.querySelectorAll("input, select, textarea").forEach((el) => {
        if (el.dataset.lock === "1") return;

        if (el.tagName === "SELECT" || el.tagName === "TEXTAREA") {
          el.disabled = !enabled;
        } else {
          el.readOnly = !enabled;
        }

        // Estética: bloqueado -> bg-light
        el.classList.toggle("bg-light", !enabled);
      });

      btn.dataset.editing = enabled ? "1" : "0";
      btn.classList.toggle("btn-outline-primary", !enabled);
      btn.classList.toggle("btn-primary", enabled);
      btn.innerHTML = enabled
        ? '<i class="fa-solid fa-check me-2"></i>Listo'
        : '<i class="fa-solid fa-pen-to-square me-2"></i>Editar';
    }

    btn.addEventListener("click", (e) => {
      e.preventDefault();
      e.stopPropagation();

      const editing = btn.dataset.editing === "1";
      apply(!editing);

      if (typeof onToggle === "function") onToggle(!editing);
    });

    apply(false);
    console.log(`[Editar] Enganchado: ${cardId} ↔ ${btnId}`);
  }

  // ==========================
  // Otros: dependientes
  // ==========================
  function updateOtrosDependientes() {
    const isEditing = $("btnEditarOtros")?.dataset.editing === "1";

    function enableIfYes(selectId, inputId) {
      const sel = $(selectId);
      const inp = $(inputId);
      if (!sel || !inp) return;

      const yes = (sel.value || "").toUpperCase() === "SI";
      const enable = isEditing && yes;

      // Si el card no está en edición, siempre bloquea
      inp.disabled = !enable;
      inp.readOnly = !enable;
      inp.classList.toggle("bg-light", !enable);

      // Si no es Sí, limpia
      if (!yes) inp.value = "";
    }

    enableIfYes("txtBeca", "txtTipoBeca");
    enableIfYes("txtLenguaIndigena", "txtLenguaCual");
    enableIfYes("txtDiscapacidad", "txtDiscapEspec");
    enableIfYes("txtEnfermedad", "txtEnfEspec");
  }

  // ==========================
  // 1) SIEMPRE enganchar botones
  // ==========================
  wireEditCard("cardDatosGenerales", "btnEditarDatosGenerales");
  wireEditCard("cardDomicilio", "btnEditarDomicilio");
  wireEditCard("cardTutor", "btnEditarTutor");
  wireEditCard("cardEscolares", "btnEditarEscolares");
  wireEditCard("cardOtros", "btnEditarOtros", () => updateOtrosDependientes());

  // Listeners otros (por si cambian select)
  ["txtBeca", "txtLenguaIndigena", "txtDiscapacidad", "txtEnfermedad"].forEach((id) => {
    const el = $(id);
    if (el) el.addEventListener("change", updateOtrosDependientes);
  });

  // Teléfonos 10 dígitos (no estorba aunque esté readonly)
  onlyDigits10($("txtTutorTelCasa"));
  onlyDigits10($("txtTutorTelTrabajo"));

  // Estado inicial dependientes
  updateOtrosDependientes();

  // ==========================
  // 2) Luego intentar cargar payload (SIN CRASHEAR)
  // ==========================
  let payload = {};
  try {
    payload = JSON.parse(localStorage.getItem("preinscripcion_payload") || "{}");
  } catch (e) {
    console.warn("preinscripcion_payload corrupto en localStorage");
    payload = {};
  }

  const datos = payload?.datos;
  if (!datos) {
    console.warn("No hay datos de preinscripción en localStorage (preinscripcion_payload).");
    // Importante: NO retornamos, porque los botones ya están activos.
    return;
  }

  const d = datos;

  // ==========================
  // Pintar (solo si hay datos)
  // ==========================

  // Generales
  setText("txtApellidoPaterno", d.generales?.apPaterno);
  setText("txtApellidoMaterno", d.generales?.apMaterno);
  setText("txtNombres", d.generales?.nombres);
  setText("txtGenero", d.generales?.genero);
  setText("txtFechaNacimiento", d.generales?.fechaNacimiento);
  setText("txtEstadoCivil", d.generales?.estadoCivil);
  setText("txtNacionalidad", d.generales?.nacionalidad);
  setText("txtCurp", d.generales?.curp);

  // Tipo de sangre (pinta en cualquiera que exista)
  setText("txtTipoSangre", d.generales?.tipoSangre);
  setText("tipoSangre", d.generales?.tipoSangre);

  // Lugar nacimiento (wrappers)
  const tieneLugarNac = !!(d.generales?.estadoNacimiento || d.generales?.municipioNacimiento);
  toggle("wrapEstadoNacimiento", tieneLugarNac);
  toggle("wrapMunicipioNacimiento", tieneLugarNac);
  setText("txtEstadoNacimiento", d.generales?.estadoNacimiento);
  setText("txtMunicipioNacimiento", d.generales?.municipioNacimiento);

  // Domicilio/contacto
  setText("txtCalle", d.domicilio?.calle);
  setText("txtNumExt", d.domicilio?.numExt);
  setText("txtNumInt", d.domicilio?.numInt);
  setText("txtCP", d.domicilio?.cp);
  setText("txtColonia", d.domicilio?.colonia);
  setText("txtEstado", d.domicilio?.estado);
  setText("txtMunicipio", d.domicilio?.municipio);
  setText("txtEmail", d.contacto?.email);
  setText("txtTelefono", d.contacto?.telefono);

  // Tutor
  setText("txtTutorParentesco", d.tutor?.parentesco);
  setText("txtTutorApPaterno", d.tutor?.apPaterno);
  setText("txtTutorApMaterno", d.tutor?.apMaterno);
  setText("txtTutorNombres", d.tutor?.nombres);
  setText("txtTutorTelCasa", d.tutor?.telCasa);
  setText("txtTutorTelTrabajo", d.tutor?.telTrabajo);

  // Escolares
  setText("txtEscProcedencia", d.escolares?.escuela);
  setText("txtEscCarrera", d.escolares?.carrera);
  setText("txtEscEstado", d.escolares?.estado);
  setText("txtEscMunicipio", d.escolares?.municipio);
  setText("txtEscPromedio", d.escolares?.promedio);
  setText("txtEscFechaInicio", d.escolares?.fechaInicio);
  setText("txtEscFechaFin", d.escolares?.fechaFin);
  setText("txtEscSistema", d.escolares?.sistema);
  setText("txtEscTipoPrepa", d.escolares?.tipoPrepa);

  // Otros
  setYesNo("txtBeca", d.otros?.beca);
  setText("txtTipoBeca", d.otros?.beca === "SI" ? d.otros?.tipoBeca : "—");

  setYesNo("txtOrigenIndigena", d.otros?.origenIndigena);
  setYesNo("txtLenguaIndigena", d.otros?.lenguaIndigena);
  setText("txtLenguaCual", d.otros?.lenguaIndigena === "SI" ? d.otros?.lenguaCual : "—");

  setYesNo("txtDiscapacidad", d.otros?.discapacidad);
  setText("txtDiscapEspec", d.otros?.discapacidad === "SI" ? d.otros?.discapEspec : "—");

  setYesNo("txtEnfermedad", d.otros?.enfermedad);
  setText("txtEnfEspec", d.otros?.enfermedad === "SI" ? d.otros?.enfEspec : "—");

  // Re-evaluar dependientes con valores pintados
  updateOtrosDependientes();
});
