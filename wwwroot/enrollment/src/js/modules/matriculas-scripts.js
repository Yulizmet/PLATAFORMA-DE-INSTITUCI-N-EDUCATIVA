// js/matricula-scripts.js

document.addEventListener("DOMContentLoaded", () => {

  // =========================
  // 1) TABLA CLICKEABLE (Matriculas)
  // =========================
  const rows = document.querySelectorAll(".alumno-row");
  if (rows.length) {
    rows.forEach(row => {
      row.addEventListener("click", () => {
        const id = row.dataset.id;
        if (!id) return;
        window.location.href = `alumno.html?id=${encodeURIComponent(id)}`;
      });
    });
  }

  // =========================
  // 2) EDITAR / GUARDAR (Alumno)
  // =========================
  const btnEditar = document.getElementById("btn-editar");
  const btnGuardar = document.getElementById("btn-guardar");
  const editableFields = document.querySelectorAll(".editable-field");

  if (btnEditar && btnGuardar && editableFields.length) {

    const setEditable = (isEditable) => {
      editableFields.forEach(el => {
        // disabled para input/select/textarea
        el.disabled = !isEditable;
      });

      // UI botones
      btnEditar.classList.toggle("d-none", isEditable);
      btnGuardar.classList.toggle("d-none", !isEditable);
    };

    // Inicia bloqueado
    setEditable(false);

    btnEditar.addEventListener("click", () => {
      setEditable(true);
    });

    btnGuardar.addEventListener("click", () => {
      // Aquí podrías mandar al backend (fetch/AJAX)
      // Ejemplo (después lo hacemos):
      // const data = new FormData(document.getElementById("form-alumno"));
      // fetch('/api/alumno/update', { method:'POST', body:data })

      setEditable(false);
    });

    // Opcional: leer id y mostrarlo en alguna parte
    const params = new URLSearchParams(window.location.search);
    const id = params.get("id");
    if (id) {
      // Si quieres, puedes usar esto para cargar info por matrícula desde backend.
      // Por ahora solo lo mostramos en la tarjeta si existe el span:
      const matriculaCard = document.getElementById("alumno-matricula-card");
      if (matriculaCard) matriculaCard.textContent = id;
    }
  }

});