export function initTags() {
    const tagsInput = document.querySelector('#tags-input');
    const tagsHidden = document.querySelector('#tags-hidden');
    
    if (!tagsInput) return;
    
    $(tagsInput).select2({
        tags: true,
        tokenSeparators: [',', ' '],
        placeholder: tagsInput.dataset.placeholder,
        allowClear: true,
        ajax: {
            url: '/Templates/SearchTags',
            dataType: 'json',
            delay: 250,
            data: params => ({ term: params.term || '' }),
            processResults: data => ({
                results: data.map(tag => ({
                    id: tag,
                    text: tag
                }))
            }),
            cache: true,
            error: function() {
                console.error('Failed to load tags');
            }
        },
        minimumInputLength: 1,
        language: {
            inputTooShort: () => tagsInput.dataset.minimumInputMessage
        }
    });
    
    $(tagsInput).on('change', function() {
        const selectedTags = $(this).val() || [];
        tagsHidden.value = selectedTags.join(', ');
    });
    
    if (tagsHidden.value) {
        const tags = tagsHidden.value.split(',').map(tag => tag.trim()).filter(tag => tag);
        tags.forEach(tag => {
            const option = new Option(tag, tag, true, true);
            tagsInput.appendChild(option);
        });
        $(tagsInput).trigger('change');
    }
    
    const form = tagsInput.closest('form');
    if (form) {
        form.addEventListener('submit', () => {
            const selectedTags = $(tagsInput).val() || [];
            tagsHidden.value = selectedTags.join(', ');
        });
    }
}