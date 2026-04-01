const LS_KEY = "preinscripcion_activa";

function guardarPreinscripcionLS(payload) {
    sessionStorage.setItem(LS_KEY, JSON.stringify(payload));
}

function leerPreinscripcionLS() {
    try {
        const raw = sessionStorage.getItem(LS_KEY);
        return raw ? JSON.parse(raw) : null;
    } catch (e) {
        console.error("Error leyendo SessionStorage:", e);
        return null;
    }
}

function borrarPreinscripcionLS() {
    sessionStorage.removeItem(LS_KEY);
}

function generarHash(curp, fechaNacimiento) {
    const base = `${(curp || "").toUpperCase()}|${fechaNacimiento || ""}`;
    return btoa(unescape(encodeURIComponent(base)));
}