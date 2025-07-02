export function initSelectAll(name) {
    const selectAll = document.getElementById('selectAll');
    if (!selectAll) return;

    selectAll.addEventListener('change', () => {
        const checkboxes = document.querySelectorAll(`input[name="${name}"]`);
        checkboxes.forEach(cb => {
            cb.checked = selectAll.checked;
        });
    });
}