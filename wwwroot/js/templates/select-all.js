export function initSelectAll() {
    const selectAll = document.getElementById('selectAll');
    if (!selectAll) return;

    selectAll.addEventListener('change', () => {
        document.querySelectorAll('input[name="templateIds"]').forEach(cb => {
            cb.checked = selectAll.checked;
        });
    });
}