import { jsPDF } from "jspdf";

function getText(id) {
  const el = document.getElementById(id);
  return el ? (el.textContent || "").trim() : "";
}

function getLS(key) {
  return (localStorage.getItem(key) || "").trim();
}

function safe(value, fallback = "—") {
  return value && value.trim() ? value.trim() : fallback;
}

export function convertPDF() {
  const folio = safe(getText("cpFolio") || getText("folioGenerado") || getLS("itc_folio_actual"));
  const fecha = safe(getText("fechaEnvio") || getText("cpFechaEnvio") || getLS("itc_fecha_envio"));

  const nombre = safe(getText("cpNombre") || getLS("cp_nombre") || getLS("pre_nombre"));
  const especialidad = safe(getText("cpEspecialidad") || getLS("cp_especialidad") || getLS("pre_especialidad"));
  const segunda = safe(getText("cpSegunda") || getLS("cp_segunda") || getLS("pre_segunda"));

  // 2) Crear PDF
  const doc = new jsPDF({ unit: "mm", format: "a4" });

  // Márgenes y helpers
  const left = 20;
  let y = 22;
  const line = 8;
  const pageWidth = doc.internal.pageSize.getWidth();

  // Encabezado
  doc.setFont("helvetica", "bold");
  doc.setFontSize(16);
  doc.text("COMPROBANTE DE PREINSCRIPCIÓN", pageWidth / 2, y, { align: "center" });

  y += 10;
  doc.setFontSize(11);
  doc.setFont("helvetica", "normal");
  doc.text("Instituto / Plantel: ITC", pageWidth / 2, y, { align: "center" });

  // Línea
  y += 8;
  doc.setLineWidth(0.3);
  doc.line(left, y, pageWidth - left, y);

  y += 10;

  // Datos principales
  doc.setFont("helvetica", "bold");
  doc.text("NÚMERO DE FOLIO:", left, y);
  doc.setFont("helvetica", "normal");
  doc.text(folio, left + 45, y);

  y += line;

  doc.setFont("helvetica", "bold");
  doc.text("FECHA DE ENVÍO:", left, y);
  doc.setFont("helvetica", "normal");
  doc.text(fecha, left + 45, y);

  y += line + 4;

  // Sección aspirante
  doc.setFont("helvetica", "bold");
  doc.text("DATOS DEL ASPIRANTE", left, y);
  y += 6;
  doc.setLineWidth(0.2);
  doc.line(left, y, pageWidth - left, y);
  y += 8;

  const maxW = pageWidth - left * 2;

  doc.setFont("helvetica", "bold");
  doc.text("NOMBRE:", left, y);
  doc.setFont("helvetica", "normal");
  doc.text(doc.splitTextToSize(nombre, maxW - 30), left + 30, y);

  y += line;

  doc.setFont("helvetica", "bold");
  doc.text("ESPECIALIDAD:", left, y);
  doc.setFont("helvetica", "normal");
  doc.text(doc.splitTextToSize(especialidad, maxW - 35), left + 35, y);

  y += line;

  doc.setFont("helvetica", "bold");
  doc.text("SEGUNDA OPCIÓN:", left, y);
  doc.setFont("helvetica", "normal");
  doc.text(doc.splitTextToSize(segunda, maxW - 42), left + 42, y);

  // Pie
  y = 285;
  doc.setFontSize(9);
  doc.setFont("helvetica", "normal");
  doc.text(
    "Este documento es un comprobante de preinscripción generado automáticamente.",
    pageWidth / 2,
    y,
    { align: "center" }
  );

  // 3) Guardar
  doc.save(`Comprobante-${folio}.pdf`);
}
