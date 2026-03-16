const LS_KEY = "preinscripcion_activa";

function guardarPreinscripcionLS(payload) {
  localStorage.setItem(LS_KEY, JSON.stringify(payload));
}

function leerPreinscripcionLS() {
  try {
    const raw = localStorage.getItem(LS_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch (e) {
    console.error("Error leyendo LocalStorage:", e);
    return null;
  }
}

function borrarPreinscripcionLS() {
  localStorage.removeItem(LS_KEY);
}

function generarHash(curp, fechaNacimiento) {
  const base = `${(curp || "").toUpperCase()}|${fechaNacimiento || ""}`;
  return btoa(unescape(encodeURIComponent(base)));
}
