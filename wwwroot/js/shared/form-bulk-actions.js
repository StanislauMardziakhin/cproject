export function initFormBulkActions() {
    const form = document.getElementById('forms-form');
    const checkboxes = document.querySelectorAll('input[name="formIds"]');
    const selectedIdsInput = document.getElementById('selected-form-ids');
    const errorAlert = document.getElementById('error-alert');

    if (!form) return;

    form.addEventListener('submit', (e) => {
        const selectedIds = Array.from(checkboxes)
            .filter(cb => cb.checked)
            .map(cb => cb.value);

        if (selectedIds.length === 0) {
            e.preventDefault();
            errorAlert.classList.remove('d-none');
            return;
        }

        errorAlert.classList.add('d-none');
        selectedIdsInput.value = selectedIds.join(',');
    });
}