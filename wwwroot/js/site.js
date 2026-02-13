// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", function () {
    const savedSort = localStorage.getItem(window.location.pathname + "_sort");
    if (savedSort) {
        document.getElementById('sortSelector').value = savedSort;
        applyFilters();
    }
});

function applyFilters() {
    const searchTerm = document.getElementById('tableSearch').value.toLowerCase();
    const sortValue = document.getElementById('sortSelector').value;
    const table = document.querySelector("table tbody");
    const rows = Array.from(table.querySelectorAll("tr"));

    localStorage.setItem(window.location.pathname + "_sort", sortValue);

    rows.forEach(row => {
        const text = row.innerText.toLowerCase();
        row.style.display = text.includes(searchTerm) ? "" : "none";
    });

    const sortedRows = rows.sort((a, b) => {
        let valA, valB;

        if (sortValue.startsWith("name")) {
            valA = a.cells[0].innerText.trim();
            valB = b.cells[0].innerText.trim();
            return sortValue.endsWith("asc") ? valA.localeCompare(valB) : valB.localeCompare(valA);
        }
        else if (sortValue.startsWith("date")) {
            valA = new Date(a.querySelector(".small")?.innerText || 0);
            valB = new Date(b.querySelector(".small")?.innerText || 0);
            return sortValue.endsWith("asc") ? valA - valB : valB - valA;
        }
    });

    table.append(...sortedRows);
}

function copyTableToClipboard(tableId) {
    const table = document.getElementById(tableId);
    if (!table) return;

    let csv = [];
    const rows = table.querySelectorAll("tr");

    for (let i = 0; i < rows.length; i++) {
        let row = [];
        const cols = rows[i].querySelectorAll("td, th");

        for (let j = 0; j < cols.length; j++) {
            if (cols[j].innerText.trim() === "Acciones" || cols[j].querySelector('.btn-group') || cols[j].querySelector('button')) {
                continue;
            }

            let data = cols[j].innerText.replace(/(\r\n|\n|\r)/gm, " ").trim();
            row.push(data);
        }
        csv.push(row.join("\t"));
    }

    const tableString = csv.join("\n");
    navigator.clipboard.writeText(tableString).then(() => {
        const toastElement = document.getElementById('copyToast');
        const toast = new bootstrap.Toast(toastElement);
        toast.show();
    }).catch(err => {
        console.error('Error al copiar: ', err);
    });
}

function printReport() {
    window.print();
}