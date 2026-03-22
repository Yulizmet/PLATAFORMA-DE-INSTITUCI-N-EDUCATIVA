document.addEventListener("DOMContentLoaded", function () {
    const savedSort = localStorage.getItem(window.location.pathname + "_sort");
    if (savedSort) {
        document.getElementById('sortSelector').value = savedSort;
        applyFilters();
    }
});

const Toast = Swal.mixin({
    toast: true,
    position: 'bottom-end',
    showConfirmButton: false,
    timer: 3000,
    timerProgressBar: true,
    padding: '1rem',
    customClass: {
        popup: 'colored-toast'
    },
    didOpen: (toast) => {
        toast.addEventListener('mouseenter', Swal.stopTimer)
        toast.addEventListener('mouseleave', Swal.resumeTimer)

        const icon = Swal.getIcon();
        if (icon) {
            if (icon.classList.contains('swal2-success')) toast.style.backgroundColor = '#28a745'; // Verde
            if (icon.classList.contains('swal2-error')) toast.style.backgroundColor = '#dc3545';   // Rojo
            if (icon.classList.contains('swal2-warning')) toast.style.backgroundColor = '#ffc107'; // Amarillo
            if (icon.classList.contains('swal2-info')) toast.style.backgroundColor = '#17a2b8';    // Azul
        }

        const circles = toast.querySelectorAll('[class^="swal2-success-circular-line"], .swal2-success-fix');
        circles.forEach(el => {
            el.style.backgroundColor = 'rgba(255, 255, 255, 0.1)';
            el.style.opacity = '0.5';
        });
    }
});

function showToast(icon, message) {
    Toast.fire({
        icon: icon,
        title: `<span style="color: ${icon === 'warning' ? '#000' : '#fff'}; font-weight: 500;">${message}</span>`,
        showClass: {
            popup: 'animate__animated animate__fadeInRight animate__faster'
        },
        hideClass: {
            popup: 'animate__animated animate__fadeOutRight animate__faster'
        }
    });
}

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

window.initGlobalDataTable = function (tableSelector) {
    const $table = $(tableSelector);

    if ($table.length === 0) return;

    if ($.fn.DataTable.isDataTable($table)) {
        $table.DataTable().destroy();
    }

    const table = $table.DataTable({
        "language": { "url": "//cdn.datatables.net/plug-ins/1.10.24/i18n/Spanish.json" },
        "stateSave": true,
        "stateDuration": 60 * 60 * 24,
        "pageLength": 5,
        "lengthMenu": [5, 8, 10, 15, 20, 50, 100],
        "responsive": true,
        "autoWidth": false,
        "dom": '<"dt-header-container d-flex align-items-center justify-content-between gap-3"l <"central-tools flex-fill d-flex justify-content-center"> <"search-box">>rt<"dt-footer-container d-flex align-items-center justify-content-between px-0" <"footer-left"> p>',
        "columnDefs": [{ "targets": 'no-sort', "orderable": false }],
        "order": [[1, "asc"]],
        "initComplete": function () {
            $('#tools-container').appendTo('.central-tools');
            $('#tableSearch').parent().appendTo('.search-box');
            $('#staffCounter').appendTo('.footer-left').removeClass('d-none');
            $('.dataTables_info').remove();

            $('#countNumber').text(this.api().rows({ filter: 'applied' }).count());
        }
    });

    $('#tableSearch').off('keyup').on('keyup', function () {
        table.search(this.value).draw();
        $('#countNumber').text(table.rows({ filter: 'applied' }).count());
    });

    $(document).off('change', '#checkAll').on('change', '#checkAll', function () {
        const isChecked = $(this).is(':checked');
        $('.row-checkbox').prop('checked', isChecked);
        $('.row-checkbox').closest('tr').toggleClass('table-active', isChecked);
        if (typeof updateBulkBar === "function") updateBulkBar();
    });

    $(document).off('change', '.row-checkbox').on('change', '.row-checkbox', function () {
        const total = $('.row-checkbox').length;
        const selected = $('.row-checkbox:checked').length;
        $('#checkAll').prop('checked', total === selected);
        $(this).closest('tr').toggleClass('table-active', $(this).is(':checked'));
        if (typeof updateBulkBar === "function") updateBulkBar();
    });

    return table;
};