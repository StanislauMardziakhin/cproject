export function initTemplateBulkActions() {
    const form = document.getElementById('templates-form');
    const actionInput = document.getElementById('action-input');
    const selectedIdsInput = document.getElementById('selected-template-ids');
    if (!form || !actionInput || !selectedIdsInput) return;

    document.querySelectorAll('button[name="action"]').forEach(button => {
        button.addEventListener('click', (e) => {
            e.preventDefault();
            const action = button.value.toLowerCase();

            const labels = {
                delete: window.localization.Delete,
                publish: window.localization.Publish,
                unpublish: window.localization.Unpublish
            };

            if (!confirm(`${window.localization.ConfirmDelete} ${labels[action]}?`)) return;

            const selected = Array.from(document.querySelectorAll('input[name="templateIds"]:checked')).map(cb => cb.value);
            if (selected.length === 0) {
                document.getElementById("error-alert")?.classList.remove("d-none");
                return;
            }

            selectedIdsInput.value = selected.join(',');
            actionInput.value = action;
            form.submit();
        });
    });
}