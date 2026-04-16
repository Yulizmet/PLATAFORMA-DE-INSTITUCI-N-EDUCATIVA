(() => {
  "use strict";

  function activarSidebarAutomatico() {
    const currentPage = window.location.pathname.split("/").pop() || "index.html";

    document.querySelectorAll(".sidebar-item").forEach(item => {
      const link = item.querySelector("a.sidebar-link");
      if (!link) return;

      const href = link.getAttribute("href") || "";
      const linkPage = href.split("/").pop(); // por si pones rutas

      item.classList.toggle("active", linkPage === currentPage);
    });
  }

  document.addEventListener("DOMContentLoaded", () => {
    activarSidebarAutomatico();
  });
})();