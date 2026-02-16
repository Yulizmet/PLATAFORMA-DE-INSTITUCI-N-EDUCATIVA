document.addEventListener("DOMContentLoaded", () => {
  const $ = (id) => document.getElementById(id);

  function val(id) {
    const el = $(id);
    return el ? (el.value ?? "").trim() : "";
  }

  function getRadioValue(name) {
    const checked = document.querySelector(`input[name="${name}"]:checked`);
    return checked ? checked.value : "";
  }

  function cleanCurp(curp) {
    return (curp || "").replace(/\s+/g, "").toUpperCase();
  }

  const trabajaSi = $("trabajaSi");
  const trabajaNo = $("trabajaNo");
  const wrapTrabajo = $("camposTrabajo");

  const trabOcu = $("trabOcupacion");
  const trabDir = $("trabDireccion");
  const trabTel = $("trabTelefono");

  function toggleTrabajo() {
    const trabaja = !!(trabajaSi && trabajaSi.checked);

    if (wrapTrabajo) wrapTrabajo.classList.toggle("d-none", !trabaja);

    [trabOcu, trabDir, trabTel].forEach((el) => {
      if (!el) return;
      el.required = trabaja;
      if (!trabaja) el.value = "";
    });
  }

  trabajaSi?.addEventListener("change", toggleTrabajo);
  trabajaNo?.addEventListener("change", toggleTrabajo);
  toggleTrabajo();

  function toggleReq({ yesId, wrapId, fieldId, type = "input" }) {
    const yes = $(yesId);
    const wrap = $(wrapId);
    const field = $(fieldId);
    if (!yes || !wrap || !field) return;

    const activo = yes.checked;
    wrap.classList.toggle("d-none", !activo);

    field.required = activo;

    if (!activo) {
      if (type === "select") field.value = "";
      else field.value = "";
    }
  }

  function becaHandler() {
    toggleReq({ yesId: "becaSi", wrapId: "wrapTipoBeca", fieldId: "tipoBeca", type: "select" });
  }
  function lenguaHandler() {
    toggleReq({ yesId: "lenguaSi", wrapId: "wrapLenguaCual", fieldId: "lenguaCual" });
  }
  function discapHandler() {
    toggleReq({ yesId: "discapacidadSi", wrapId: "wrapDiscapEspec", fieldId: "discapEspec" });
  }
  function enfHandler() {
    toggleReq({ yesId: "enfermedadSi", wrapId: "wrapEnfEspec", fieldId: "enfEspec" });
  }

  ["becaNo", "becaSi"].forEach((id) => $(id)?.addEventListener("change", becaHandler));
  ["lenguaNo", "lenguaSi"].forEach((id) => $(id)?.addEventListener("change", lenguaHandler));
  ["discapacidadNo", "discapacidadSi"].forEach((id) => $(id)?.addEventListener("change", discapHandler));
  ["enfermedadNo", "enfermedadSi"].forEach((id) => $(id)?.addEventListener("change", enfHandler));

  becaHandler();
  lenguaHandler();
  discapHandler();
  enfHandler();

  const btn = $("btnContinuar");
  const form = $("formPreinscripcion"); 

  function guardarYContinuar() {
    if (form && !form.checkValidity()) {
      form.classList.add("was-validated");
      return;
    }

    const curp = cleanCurp(val("curp"));
    const fecha = val("fechaNacimiento");

    if (!curp || curp.length !== 18) {
      alert("CURP inválida. Debe tener 18 caracteres.");
      return;
    }
    if (!fecha) {
      alert("Falta la fecha de nacimiento.");
      return;
    }

    const datos = {
      generales: {
        apPaterno: val("apPaterno"),
        apMaterno: val("apMaterno"),
        nombres: val("nombres"),
        genero: val("genero"),
        fechaNacimiento: fecha,
        estadoCivil: val("estadoCivil"),
        nacionalidad: val("nacionalidad"),
        estadoNacimiento: val("estadoNacimiento"),
        municipioNacimiento: val("municipioNacimiento"),
        curp: curp,
        trabaja: getRadioValue("trabaja"), 
        trabajo: {
          ocupacion: val("trabOcupacion"),
          direccion: val("trabDireccion"),
          telefono: val("trabTelefono"),
        }
      },

      domicilio: {
        calle: val("domCalle"),
        numExt: val("domNumExt"),
        numInt: val("domNumInt"),
        cp: val("domCP"),
        colonia: val("domColonia"),
        estado: val("domEstado"),
        municipio: val("domMunicipio"),
        email: val("domEmail"),
        telefono: val("domTelefono"),
      },

      tutor: {
        parentesco: val("tutorParentesco"),
        apPaterno: val("tutorApPaterno"),
        apMaterno: val("tutorApMaterno"),
        nombres: val("tutorNombres"),
        telCasa: val("tutorTelCasa"),
        telTrabajo: val("tutorTelTrabajo"),
      },

      escolares: {
        procedencia: val("escProcedencia"),
        carrera: val("escCarrera"),
        estado: val("escEstado"),
        municipio: val("escMunicipio"),
        promedio: val("escPromedio"),
        fechaInicio: val("escFechaInicio"),
        fechaFin: val("escFechaFin"),
        sistema: val("escSistema"),
        tipoPrepa: val("escTipoPrepa"),
      },

      otros: {
        beca: getRadioValue("beca"),
        tipoBeca: val("tipoBeca"),
        origenIndigena: getRadioValue("origenIndigena"),
        lenguaIndigena: getRadioValue("lenguaIndigena"),
        lenguaCual: val("lenguaCual"),
        discapacidad: getRadioValue("discapacidad"),
        discapEspec: val("discapEspec"),
        enfermedad: getRadioValue("enfermedad"),
        enfEspec: val("enfEspec"),
        comoSeEntero: val("comoSeEntero"),
      }
    };

    const hash = generarHash(curp, fecha);
    const existente = leerPreinscripcionLS();

    if (existente && existente.hash === hash) {
      window.location.href = "alumno.html";
      return;
    }

    console.log("OBJETO QUE VOY A GUARDAR:", { hash, createdAt: Date.now(), datos });

    guardarPreinscripcionLS({
      hash,
      createdAt: Date.now(),
      datos
    });

    window.location.href = "alumno.html";
  }

  btn?.addEventListener("click", (e) => {
    e.preventDefault();
    guardarYContinuar();
  });
});
