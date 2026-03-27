(function () {
    'use strict';
    const CONFIG = {
        debounceDelay: 1000,        // 1 segundo de espera después de dejar de escribir
        minCharsToSearch: 2,        // Mínimo de caracteres para iniciar búsqueda
        showLoadingIndicator: true   // Mostrar indicador de carga
    };
    let debounceTimer = null;
    let lastSearchValue = '';
    let isSearching = false;

    function init() {
        // Buscar el input de búsqueda y su formulario asociado.
        // Algunos archivos usan id="searchForm" y otros id="filterForm".
        const searchInput = document.getElementById('searchName');
        let searchForm = null;
        const searchingIndicator = document.getElementById('searchingIndicator');

        if (searchInput) {
            // Preferir el formulario más cercano al input
            searchForm = searchInput.closest('form') || document.getElementById('searchForm') || document.getElementById('filterForm');
        } else {
            // Intentar encontrar por id si no existe el input con id searchName
            searchForm = document.getElementById('searchForm') || document.getElementById('filterForm');
        }

        if (!searchInput || !searchForm) {
            console.warn('SocialServiceAutoSearch: Elementos de búsqueda no encontrados (input#searchName y/o form)');
            return;
        }
        lastSearchValue = searchInput.value.trim();
        searchInput.addEventListener('input', function () {
            handleInput(searchInput, searchForm, searchingIndicator);
        });
        searchForm.addEventListener('submit', function (e) {
            clearTimeout(debounceTimer);
            if (searchingIndicator) {
                searchingIndicator.style.display = 'none';
            }
            isSearching = false;
        });
        searchInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                clearTimeout(debounceTimer);
                if (searchingIndicator) {
                    searchingIndicator.style.display = 'none';
                }
                searchForm.submit();
            }
        });

        console.log('SocialServiceAutoSearch: Inicializado correctamente');
    }

    function handleInput(searchInput, searchForm, searchingIndicator) {
        const currentValue = searchInput.value.trim();
        clearTimeout(debounceTimer);
        if (isSearching) {
            return;
        }
        if (currentValue === lastSearchValue) {
            return;
        }
        if (searchingIndicator && CONFIG.showLoadingIndicator) {
            searchingIndicator.style.display = 'inline';
        }
        if (currentValue.length === 0) {
            debounceTimer = setTimeout(function () {
                lastSearchValue = currentValue;
                isSearching = true;
                if (searchingIndicator) {
                    searchingIndicator.style.display = 'none';
                }
                searchForm.submit();
            }, 300);
            return;
        }
        if (currentValue.length < CONFIG.minCharsToSearch) {
            if (searchingIndicator) {
                searchingIndicator.style.display = 'none';
            }
            return;
        }
        debounceTimer = setTimeout(function () {
            if (searchInput.value.trim() === currentValue) {
                lastSearchValue = currentValue;
                isSearching = true;
                
                if (searchingIndicator) {
                    searchingIndicator.style.display = 'none';
                }
                
                searchForm.submit();
            }
        }, CONFIG.debounceDelay);
    }
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();
