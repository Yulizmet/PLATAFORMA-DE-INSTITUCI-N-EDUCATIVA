(function () {
  "use strict";

  // ===== CONFIG EXACTO =====
  const CONFIG = {
    marginTop: 20,
    marginBottom: 25,
    marginLeft: 25,
    marginRight: 25,

    // Logo PERFECTO: NO TOCAR
    logoX: 25,
    logoY: 20,
    logoW: 40.9,
    logoH: 11.9,

    // Encabezado (solo letras)
    titleY: 26,
  };

  function _txt(id) {
    const el = document.getElementById(id);
    return el ? (el.textContent || "").trim() : "";
  }
  function _ls(key) {
    return (localStorage.getItem(key) || "").trim();
  }
  function _safe(v, fb = "—") {
    v = (v || "").toString().trim();
    return v ? v : fb;
  }
  function _upper(v) {
    return (v || "").toString().trim().toUpperCase();
  }

  // ====== LS payload (misma key que tu proyecto) ======
  function leerPreinscripcionLS() {
    const LS_KEY = "preinscripcion_activa";
    try {
      const raw = localStorage.getItem(LS_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch (e) {
      console.error("Error leyendo LocalStorage:", e);
      return null;
    }
  }

  // ====== Generación automática ======
  function getGeneracionAuto() {
    const start = new Date().getFullYear();
    return `${start}-${start + 3}`;
  }

  // ====== Normaliza carrera (evita "GENERAL" por default) ======
  function normalizeCarrera(v) {
    const s = (v ?? "").toString().trim();
    if (!s) return "";
    const up = s.toUpperCase();
    if (up === "GENERAL" || up === "—" || up === "SIN SELECCIÓN") return "";
    return s;
  }

  // ====== Fecha / edad helpers ======
  function parseDateLoose(value) {
    const s = (value || "").toString().trim();
    if (!s) return null;

    // YYYY-MM-DD
    if (/^\d{4}-\d{2}-\d{2}$/.test(s)) {
      const d = new Date(s + "T00:00:00");
      return isNaN(d.getTime()) ? null : d;
    }

    // DD/MM/YYYY o DD-MM-YYYY
    const m = s.match(/^(\d{2})[\/-](\d{2})[\/-](\d{4})$/);
    if (m) {
      const dd = Number(m[1]);
      const mm = Number(m[2]);
      const yyyy = Number(m[3]);
      const d = new Date(yyyy, mm - 1, dd);
      return isNaN(d.getTime()) ? null : d;
    }

    const d = new Date(s);
    return isNaN(d.getTime()) ? null : d;
  }

  function calcEdad(fechaNacStr) {
    const d = parseDateLoose(fechaNacStr);
    if (!d) return "—";

    const now = new Date();
    let age = now.getFullYear() - d.getFullYear();
    const m = now.getMonth() - d.getMonth();
    if (m < 0 || (m === 0 && now.getDate() < d.getDate())) age--;
    return String(age);
  }

  function formatFechaMX(fechaNacStr) {
    const d = parseDateLoose(fechaNacStr);
    if (!d) return "—";
    return d.toLocaleDateString("es-MX", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    });
  }

  function buildNombreCompleto(g = {}) {
    return [g.nombres, g.apPaterno, g.apMaterno]
      .map((x) => (x || "").toString().trim())
      .filter(Boolean)
      .join(" ");
  }

  function buildDomicilioLine(dom = {}) {
    const calle = (dom.calle || "").trim();
    const numExt = (dom.numExt || "").toString().trim();
    const numInt = (dom.numInt || "").toString().trim();
    const colonia = (dom.colonia || "").trim();
    const cp = (dom.cp || "").toString().trim();

    const num = [numExt && `#${numExt}`, numInt && `Int. ${numInt}`]
      .filter(Boolean)
      .join(" ");
    const p1 = [calle, num].filter(Boolean).join(" ");
    const p2 = [colonia, cp && `C.P. ${cp}`].filter(Boolean).join(", ");

    return [p1, p2].filter(Boolean).join(" — ");
  }

  // ====== Texto en una línea, bajando font si no cabe ======
  function textFitOneLine(doc, text, x, y, maxW, startSize = 11, minSize = 7.5) {
    let size = startSize;
    doc.setFontSize(size);
    while (size > minSize && doc.getTextWidth(text) > maxW) {
      size -= 0.5;
      doc.setFontSize(size);
    }
    doc.text(text, x, y);
    return size;
  }

  function imgToDataURL(imgEl) {
    return new Promise((resolve) => {
      if (!imgEl) return resolve(null);

      const done = () => {
        try {
          const canvas = document.createElement("canvas");
          canvas.width = imgEl.naturalWidth || imgEl.width;
          canvas.height = imgEl.naturalHeight || imgEl.height;
          const ctx = canvas.getContext("2d");
          ctx.drawImage(imgEl, 0, 0);
          resolve(canvas.toDataURL("image/png"));
        } catch (e) {
          console.warn("No se pudo convertir logo a DataURL:", e);
          resolve(null);
        }
      };

      if (imgEl.complete && imgEl.naturalWidth) return done();
      imgEl.onload = done;
      imgEl.onerror = () => resolve(null);
      setTimeout(() => resolve(null), 1500);
    });
  }

  function safeRect(doc, x, y, w, h, label = "rect") {
    const ok = [x, y, w, h].every((v) => Number.isFinite(v)) && w > 0 && h > 0;
    if (!ok) {
      console.error(`Rect inválido (${label}):`, { x, y, w, h });
      throw new Error(`Rect inválido (${label})`);
    }
    doc.rect(x, y, w, h);
  }

  function wrapText(doc, text, maxWidth) {
    return doc.splitTextToSize(text, maxWidth);
  }

  function justifyLine(doc, line, x, y, maxWidth) {
    const words = line.trim().split(/\s+/);
    if (words.length <= 1) return doc.text(line, x, y);

    const lineNoSpaces = words.join("");
    const wordsWidth = doc.getTextWidth(lineNoSpaces);
    const gaps = words.length - 1;
    const extra = maxWidth - wordsWidth;

    if (extra <= 0) return doc.text(line, x, y);

    const gapW = extra / gaps;
    let cursor = x;

    for (let i = 0; i < words.length; i++) {
      const w = words[i];
      doc.text(w, cursor, y);
      cursor += doc.getTextWidth(w);
      if (i < words.length - 1) cursor += gapW;
    }
  }

  function drawJustifiedParagraph(
    doc,
    text,
    x,
    y,
    boxInnerW,
    boxInnerH,
    fontSize,
    lineGap = 1.02
  ) {
    const localLineH = fontSize * 0.3527777778 * lineGap;
    const lines = wrapText(doc, text, boxInnerW);

    const maxLines = Math.floor(boxInnerH / localLineH);
    const clipped = lines.slice(0, Math.max(0, maxLines));

    for (let i = 0; i < clipped.length; i++) {
      const line = clipped[i];
      const isLast = i === clipped.length - 1;
      if (isLast) doc.text(line, x, y + i * localLineH);
      else justifyLine(doc, line, x, y + i * localLineH, boxInnerW);
    }

    return y + clipped.length * localLineH;
  }

  window.convertirPdf = async function convertirPdf() {
    try {
      if (!window.jspdf || typeof window.jspdf.jsPDF !== "function") {
        alert("jsPDF no está cargado. Revisa el orden de scripts.");
        return;
      }

      const { jsPDF } = window.jspdf;
      const doc = new jsPDF({ unit: "mm", format: "a4" });

      const pageW = doc.internal.pageSize.getWidth();
      const pageH = doc.internal.pageSize.getHeight();
      const left = CONFIG.marginLeft;
      const right = CONFIG.marginRight;

      const gap = 10;
      const rightColumnX = CONFIG.logoX + CONFIG.logoW + gap;
      const titleXRight = pageW - right;
      const titleMaxW = titleXRight - rightColumnX;

      // ====== Folio ======
      const folio = _upper(
        _safe(_txt("cpFolio") || _txt("folioGenerado") || _ls("itc_folio_actual"))
      );

      // ====== Datos desde LS ======
      const payload = leerPreinscripcionLS();
      const d = payload?.datos || {};
      const g = d.generales || {};
      const dom = d.domicilio || {};
      const e = d.escolares || {};

      const nombreCompletoLS = buildNombreCompleto(g);
      const nombre = _upper(_safe(_txt("cpNombre") || nombreCompletoLS));

      // ✅ Carrera solicitada (evita GENERAL)
      const carreraPrefer = normalizeCarrera(e.carreraSolicitada);
      const carreraFallback = normalizeCarrera(e.carrera);
      const carreraLS = carreraPrefer || carreraFallback || "";
      const carrera = _upper(_safe(_txt("cpEspecialidad") || carreraLS));

      // ====== Datos aspirante ======
      const aspNombreCompleto = _upper(_safe(buildNombreCompleto(g)));
      const aspGenero = _upper(_safe(g.genero));
      const aspFechaNacFmt = _safe(formatFechaMX(g.fechaNacimiento));
      const aspEdad = _safe(calcEdad(g.fechaNacimiento));
      const aspCurp = _upper(_safe(g.curp));
      const aspTipoSangre = _upper(_safe(g.tipoSangre));

      const aspProcedencia = _upper(_safe(e.procedencia));
      const aspPromedio = _safe(e.promedio);

      const domEstado = _upper(_safe(dom.estado));
      const domMunicipio = _upper(_safe(dom.municipio));
      const domLinea = _upper(_safe(buildDomicilioLine(dom)));

      const anioConcluyo = (() => {
        const fin = (e.fechaFin || "").toString().trim();
        const dt = parseDateLoose(fin);
        return dt ? String(dt.getFullYear()) : "—";
      })();

      // ====== Logo ======
      const logoEl = document.getElementById("pdfLogo");
      const logoDataUrl = await imgToDataURL(logoEl);
      if (logoDataUrl) {
        doc.addImage(
          logoDataUrl,
          "PNG",
          CONFIG.logoX,
          CONFIG.logoY,
          CONFIG.logoW,
          CONFIG.logoH
        );
      }

      // ====== Encabezado ======
      doc.setFont("helvetica", "bold");
      doc.setFontSize(18);
      const title = "FICHA DE PREINSCRIPCION ITACE";
      const titleLines = doc.splitTextToSize(title, titleMaxW);
      doc.text(titleLines, titleXRight, CONFIG.titleY, { align: "right" });

      const titleLineH = 18 * 0.3527777778 * 1.2;
      const afterTitleY = CONFIG.titleY + (titleLines.length - 1) * titleLineH;

      doc.setFontSize(12);
      doc.text(`GENERACIÓN ${getGeneracionAuto()}`, titleXRight, afterTitleY + 10, {
        align: "right",
      });

      // ====== Datos generales arriba ======
      doc.setFontSize(12);
      const lineH = 12 * 0.3527777778 * 1.5;

      const logoBottom = CONFIG.logoY + CONFIG.logoH;
      let y = logoBottom + 20;

      doc.setFont("helvetica", "bold");
      doc.text("NÚMERO DE FOLIO:", left, y);
      doc.setFont("helvetica", "normal");
      doc.text(folio, left + 48, y);

      y += lineH;
      doc.setFont("helvetica", "bold");
      doc.text("NOMBRE DEL ASPIRANTE:", left, y);
      doc.setFont("helvetica", "normal");
      doc.text(nombre, left + 62, y);

      y += lineH;
      doc.setFont("helvetica", "bold");
      doc.text("CARRERA:", left, y);
      doc.setFont("helvetica", "normal");
      doc.text(carrera, left + 22, y);

      y += lineH * 1.2;
      doc.setLineWidth(0.6);
      doc.setLineDashPattern([4, 3], 0);
      doc.line(left, y, pageW - right, y);
      doc.setLineDashPattern([], 0);

      // ====== Sección aspirante ======
      y += 12;
      doc.setFont("helvetica", "bold");
      doc.setFontSize(12);
      doc.text("DATOS DEL ASPIRANTE", pageW / 2, y, { align: "center" });

      y += 18;
      doc.setFontSize(11);

      // ====== Field pegado al ":" (sin offsets fijos) ======
      function fieldTight(x, yy, label, value = "", gapPx = 3) {
        doc.setFont("helvetica", "bold");
        doc.text(label, x, yy);
        const labelW = doc.getTextWidth(label);
        doc.setFont("helvetica", "normal");
        doc.text(_safe(value), x + labelW + gapPx, yy);
      }

      const col1X = left;
      const col2X = left + 62;
      const col3X = left + 120;
      const rightEdge = pageW - right;

      // === Nombre ===
      fieldTight(col1X, y, "Nombre:", aspNombreCompleto, 4);
      y += 10;

      // === Género / Fecha / Edad (MISMA LÍNEA, SIN HUECOS) ===
      fieldTight(col1X, y, "Género:", aspGenero, 4);

      doc.setFont("helvetica", "bold");
      const fechaLabel = "Fecha de Nacimiento:";
      doc.text(fechaLabel, col2X, y);
      const fechaLabelW = doc.getTextWidth(fechaLabel);

      doc.setFont("helvetica", "normal");
      const fechaGap = 7; // mover fecha unos puntos a la derecha
      const fechaX = col2X + fechaLabelW + fechaGap;
      doc.text(_safe(aspFechaNacFmt), fechaX, y);

      // Edad pegada al margen derecho (respetando margen)
      doc.setFont("helvetica", "bold");
      const edadLabel = "Edad:";
      const edadLabelW = doc.getTextWidth(edadLabel);
      doc.setFont("helvetica", "normal");
      const edadVal = _safe(aspEdad);
      const edadValW = doc.getTextWidth(edadVal);
      const edadGap = 3;

      const edadX = rightEdge - (edadLabelW + edadGap + edadValW);

      doc.setFont("helvetica", "bold");
      doc.text(edadLabel, edadX, y);
      doc.setFont("helvetica", "normal");
      doc.text(edadVal, edadX + edadLabelW + edadGap, y);

      y += 10; // ✅ SOLO una vez para evitar el hueco grande

      // === CURP / Tipo de sangre ===
      fieldTight(col1X, y, "CURP:", aspCurp, 4);
      fieldTight(col3X, y, "Tipo de sangre:", aspTipoSangre, 4);
      y += 10;

      // === Secundaria de procedencia (auto-fit) ===
      doc.setFont("helvetica", "bold");
      doc.setFontSize(11);
      const secLabel = "Secundaria de procedencia:";
      doc.text(secLabel, col1X, y);

      const secValueX = col1X + doc.getTextWidth(secLabel) + 4;
      const secMaxW = pageW - right - secValueX;

      doc.setFont("helvetica", "normal");
      textFitOneLine(doc, _safe(aspProcedencia), secValueX, y, secMaxW, 11, 7.5);

      doc.setFontSize(11);
      y += 10;

      // === Año concluyó / Promedio ===
      fieldTight(col1X, y, "Año en el que concluyo la secundaria:", anioConcluyo, 4);
      fieldTight(col3X, y, "Promedio:", aspPromedio, 4);
      y += 10;

      // === Domicilio ===
      fieldTight(col1X, y, "Domicilio del alumno:", domLinea, 4);
      y += 10;

      // === Ciudad / Estado ===
      fieldTight(col1X, y, "Ciudad:", domMunicipio, 4);
      fieldTight(col2X, y, "Estado:", domEstado, 4);
      y += 14;

      // ====== Divisor ======
      doc.setLineWidth(0.6);
      doc.setLineDashPattern([4, 3], 0);
      doc.line(left, y, pageW - right, y);
      doc.setLineDashPattern([], 0);

      y += 9;

      // ====== Documentos entregados ======
      doc.setFont("helvetica", "bold");
      doc.setFontSize(11);
      doc.text("DOCUMENTOS ENTREGADOS:", left, y);

      doc.setFont("helvetica", "normal");
      doc.setFontSize(9.5);

      let ly = y + 10;
      const bulletX = left;
      const textX = left + 6;
      const listStep = 7.0;

      const docs = [
        "Acta de nacimiento actualizada",
        "CURP actualizado",
        "Certificado de secundaria",
        "Comprobante de domicilio (3 meses)",
        "6 fotografías tamaño infantil papel mate.",
        "Carta de buena conducta",
        "PAGO COLEGIATURA.",
      ];

      docs.forEach((t) => {
        doc.text("•", bulletX, ly);
        doc.text(t, textX, ly);
        ly += listStep;
      });

      // ====== Caja certificado ======
      const boxW = 85;
      const boxH = 58;
      const boxX = pageW - right - boxW;
      const boxY = y + 2;

      doc.setLineWidth(0.4);
      safeRect(doc, boxX, boxY, boxW, boxH, "certificado");

      const pad = 5;
      const innerX = boxX + pad;
      const innerY = boxY + pad;
      const innerW = boxW - pad * 2;
      const innerH = boxH - pad * 2;

      doc.setFont("helvetica", "bold");
      doc.setFontSize(8);

      const header = "SOLO EN CASO DE FALTA DE CERTIFICADO DE SECUNDARIA";
      doc.text(
        doc.splitTextToSize(header, innerW),
        boxX + boxW / 2,
        innerY + 2.2,
        { align: "center" }
      );

      doc.text("Razón por la que no la tiene:", innerX, innerY + 11);

      doc.setLineWidth(0.3);
      doc.line(innerX, innerY + 18.5, innerX + innerW, innerY + 18.5);

      doc.setFont("helvetica", "normal");
      doc.setFontSize(8);

      const p1 =
        "por el motivo anterior me comprometo a entregar el certificado de mi hijo(a) el 12 septiembre 2025, por lo cual " +
        "me doy por enterado (a) de que en caso de no cumplir con dicho compromiso será dado de baja de manera automática " +
        "quedando mi lugar o espacio a disposición del plantel.";

      const paraTop = innerY + 22;
      const paraH = 16;
      drawJustifiedParagraph(doc, p1, innerX, paraTop, innerW, paraH, 8, 1.01);

      doc.setLineWidth(0.3);
      doc.line(innerX, innerY + 42.5, innerX + innerW, innerY + 42.5);

      doc.setFont("helvetica", "bold");
      doc.setFontSize(8);
      doc.text(
        "NOMBRE Y FIRMA COMPROMISO PADRE Y/O TUTOR",
        boxX + boxW / 2,
        innerY + innerH - 1.3,
        { align: "center" }
      );

      // ====== Cuadro inferior ======
      const bigW2 = pageW - left - right;
      const bigX2 = left;

      const leftBlockBottom2 = ly;
      const rightBlockBottom2 = boxY + boxH;

      const GAP_BELOW = 6;
      const EXTRA_DOWN = 1.5;
      let bigY2 =
        Math.max(leftBlockBottom2, rightBlockBottom2) + GAP_BELOW + EXTRA_DOWN;

      const t1b =
        "En caso de tener tramite de corrección, no coincidir tu CURP con acta de nacimiento y/o certificado de secundaria; " +
        "háganos saber su situación para brindarle la atención necesaria.";

      const t2b =
        "COMPROMISOS DEL ASPIRANTE Y PADRE DE FAMILIA: Ser estudiante regular de secundaria (No deber materias de secundaria), " +
        "respetar el grupo y turno que le sea asignado y mantener buena conducta. El ingreso sólo depende de aprobar el examen de admision.";

      const bigFontSize = 8;
      const bigLineGap = 1.0;

      const bigPad2 = 6;
      const bigInnerX2 = bigX2 + bigPad2;
      const bigInnerW2 = bigW2 - bigPad2 * 2;

      doc.setFont("helvetica", "normal");
      doc.setFontSize(bigFontSize);

      const lines1 = doc.splitTextToSize(t1b, bigInnerW2);
      const lines2 = doc.splitTextToSize(t2b, bigInnerW2);

      const boxLineH = bigFontSize * 0.3527777778 * bigLineGap;
      const linesTotal = lines1.length + 1 + lines2.length;
      const neededInnerH = linesTotal * boxLineH + 2;

      let bigH2 = neededInnerH + 9;

      const maxHInPage = pageH - 2 - bigY2;
      if (bigH2 > maxHInPage) bigH2 = maxHInPage;

      doc.setLineWidth(0.4);
      safeRect(doc, bigX2, bigY2, bigW2, bigH2, "cuadro-inferior");

      const bigInnerY2 = bigY2 + 7.5;
      const bigInnerH2 = bigH2 - 9;

      let yy2 = drawJustifiedParagraph(
        doc,
        t1b,
        bigInnerX2,
        bigInnerY2,
        bigInnerW2,
        bigInnerH2,
        bigFontSize,
        bigLineGap
      );

      yy2 += 2.0;

      drawJustifiedParagraph(
        doc,
        t2b,
        bigInnerX2,
        yy2,
        bigInnerW2,
        bigInnerY2 + bigInnerH2 - yy2,
        bigFontSize,
        bigLineGap
      );

      const filename = `Ficha_ITACE_${folio}.pdf`;
      doc.save(filename);
    } catch (err) {
      console.error("PDF: error al generar", err);
      alert("Error al generar PDF. Comuniquese con soporte para mayor detalles.");
    }
  };

  console.log("COMPROBANTE PDF listo", typeof window.convertirPdf);
})();