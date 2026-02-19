function initAutocomplete(inputSelector, type, onSelect, placeholder) {
    const $input = $(inputSelector);
    const inputId = $input.attr('id');

    $input.wrap('<div class="autocomplete-wrapper position-relative"></div>');
    $input.attr('placeholder', placeholder || 'Buscar...');
    $input.attr('autocomplete', 'off');
    $input.addClass('form-control');

    const $dropdown = $(`<div id="${inputId}_dropdown" class="autocomplete-dropdown"></div>`);
    const $hidden = $(`<input type="hidden" id="${inputId}_id" name="${inputId}_id">`);
    $input.after($dropdown).after($hidden);

    let searchTimeout;

    $input.on('input', function() {
        const term = $(this).val();
        clearTimeout(searchTimeout);
        $hidden.val('');

        if (term.length < 2) {
            $dropdown.hide();
            return;
        }

        searchTimeout = setTimeout(function() {
            $.ajax({
                url: '/api/Search',
                data: { term: term, type: type },
                success: function(data) {
                    if (data.length === 0) {
                        $dropdown.html('<div class="autocomplete-empty">No se encontraron resultados</div>').show();
                        return;
                    }

                    let html = '';
                    data.forEach(function(item) {
                        html += `
                            <div class="autocomplete-item" data-item='${JSON.stringify(item)}'>
                                <div class="autocomplete-item-title">${item.fullName || item.roleName || item.username}</div>
                                <div class="autocomplete-item-subtitle">${item.email || ''}</div>
                            </div>
                        `;
                    });
                    $dropdown.html(html).show();

                    $dropdown.find('.autocomplete-item').on('click', function() {
                        selectItem($(this), $input, $hidden, $dropdown, onSelect);
                    });
                }
            });
        }, 300);
    });

    $input.on('keydown', function(e) {
        const $items = $dropdown.find('.autocomplete-item');
        const $active = $dropdown.find('.autocomplete-item.active');

        if (e.key === 'ArrowDown') {
            e.preventDefault();
            if (!$active.length) {
                $items.first().addClass('active');
            } else {
                $active.removeClass('active').next('.autocomplete-item').addClass('active');
            }
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            if ($active.length) {
                $active.removeClass('active').prev('.autocomplete-item').addClass('active');
            }
        } else if (e.key === 'Enter' && $active.length) {
            e.preventDefault();
            selectItem($active, $input, $hidden, $dropdown, onSelect);
        } else if (e.key === 'Escape') {
            $dropdown.hide();
        }
    });

    $(document).on('click', function(e) {
        if (!$(e.target).closest('.autocomplete-wrapper').length) {
            $dropdown.hide();
        }
    });
}

function selectItem($item, $input, $hidden, $dropdown, onSelect) {
    const itemData = JSON.parse($item.attr('data-item'));
    $input.val(itemData.fullName || itemData.roleName || itemData.username);
    $hidden.val(itemData.id);
    $dropdown.hide();
    if (onSelect) onSelect(itemData);
}