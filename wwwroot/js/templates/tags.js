export function initTags() {
    const tagsInput = document.querySelector('#tags-input');
    const tagsHidden = document.querySelector('#tags-hidden');
    
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
                    id: tag.id,
                    text: tag.text
                }))
            }),
            cache: true
        },
        minimumInputLength: 1,
        language: {
            inputTooShort: () => MinimumInputMessage
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