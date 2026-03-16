document.addEventListener("DOMContentLoaded", () => {
  const payload = leerPreinscripcionLS();

  if (!payload || !payload.datos) {
    window.location.href = "alumno.html";
    return;
  }

  const d = payload.datos;
  pintarConfirmacion(d);

  const btnEnviar = document.getElementById("btnEnviarSolicitud");
  if (btnEnviar) {
    btnEnviar.addEventListener("click", async (e) => {
      e.preventDefault();
      await enviarSolicitud(payload);
    });
  }
});

function setText(id, value) {
  const el = document.getElementById(id);
  if (!el) return;
  const v = (value ?? "").toString().trim();
  el.textContent = v !== "" ? v : "—";
}

function pintarConfirmacion(d) {
  if (d.generales) {
    const g = d.generales;

    setText("txtApellidoPaterno", g.apPaterno);
    setText("txtApellidoMaterno", g.apMaterno);
    setText("txtNombres", g.nombres);
    setText("txtGenero", g.genero);
    setText("txtFechaNacimiento", g.fechaNacimiento);
    setText("txtEstadoCivil", g.estadoCivil);
    setText("txtNacionalidad", g.nacionalidad);
    setText("txtCurp", (g.curp || "").toUpperCase());
    setText("txtTipoSangre", g.tipoSangre);

    const card = document.getElementById("lugarNacimientoCard");
    if (card) {
      const hayLugar = !!(g.estadoNacimiento || g.municipioNacimiento);
      card.classList.toggle("d-none", !hayLugar);

      if (hayLugar) {
        setText("txtEstadoNacimiento", g.estadoNacimiento);
        setText("txtMunicipioNacimiento", g.municipioNacimiento);
      }
    }
  }

  if (d.domicilio) {
    const dom = d.domicilio;

    setText("txtCalle", dom.calle);
    setText("txtNumExt", dom.numExt);
    setText("txtNumInt", dom.numInt);
    setText("txtCP", dom.cp);
    setText("txtColonia", dom.colonia);
    setText("txtEstado", dom.estado);
    setText("txtMunicipio", dom.municipio);
    setText("txtEmail", dom.email);
    setText("txtTelefono", dom.telefono);
  }

  if (d.tutor) {
    const t = d.tutor;

    setText("txtTutorParentesco", t.parentesco);
    setText("txtTutorApPaterno", t.apPaterno);
    setText("txtTutorApMaterno", t.apMaterno);
    setText("txtTutorNombres", t.nombres);
    setText("txtTutorTelCasa", t.telCasa);
    setText("txtTutorTelTrabajo", t.telTrabajo);
  }

  if (d.escolares) {
    const e = d.escolares;

    setText("txtEscProcedencia", e.procedencia);

    setText("txtEscCarrera", e.carrera);

    setText("txtCarreraSolicitada", e.carreraSolicitada || e.carrera);

    setText("txtEscEstado", e.estado);
    setText("txtEscMunicipio", e.municipio);
    setText("txtEscPromedio", e.promedio);
    setText("txtEscFechaInicio", e.fechaInicio);
    setText("txtEscFechaFin", e.fechaFin);
    setText("txtEscSistema", e.sistema);
    setText("txtEscTipoPrepa", e.tipoPrepa);
  }

  if (d.otros) {
    const o = d.otros;

    setText("txtBeca", o.beca);
    setText("txtTipoBeca", o.beca === "Sí" ? o.tipoBeca : "—");

    setText("txtOrigenIndigena", o.origenIndigena);

    setText("txtLenguaIndigena", o.lenguaIndigena);
    setText("txtLenguaCual", o.lenguaIndigena === "Sí" ? o.lenguaCual : "—");

    setText("txtDiscapacidad", o.discapacidad);
    setText("txtDiscapEspec", o.discapacidad === "Sí" ? o.discapEspec : "—");

    setText("txtEnfermedad", o.enfermedad);
    setText("txtEnfEspec", o.enfermedad === "Sí" ? o.enfEspec : "—");
  }
}

async function enviarSolicitud(payload) {
  const btn = document.getElementById("btnEnviarSolicitud");
  if (btn) btn.disabled = true;

  try {
    const res = await fetch("/api/preinscripcion", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });

    if (!res.ok) throw new Error("Error en el servidor");

    limpiarPreinscripcionLS();
    window.location.href = "finalizar.html";
  } catch (err) {
    alert("No se pudo enviar la solicitud. Intenta de nuevo.");
    if (btn) btn.disabled = false;
  }
}
