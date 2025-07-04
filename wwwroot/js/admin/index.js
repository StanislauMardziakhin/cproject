import { initSortableTable } from '../shared/sortable-table.js';

document.addEventListener('DOMContentLoaded', () => {
    initSortableTable('.table', {
        sortableColumns: [1, 2, 3, 4],
        defaultSort: { column: 1, direction: 'asc' }
    });
    
    const selectAllCheckbox = document.getElementById('selectAll');
    if (selectAllCheckbox) {
        selectAllCheckbox.addEventListener('change', function () {
            document.querySelectorAll('input[name="userIds"]').forEach(checkbox => {
                checkbox.checked = this.checked;
            });
        });
    }
    
    document.querySelectorAll('.action-button').forEach(button => {
        button.addEventListener('click', function () {
            const form = document.querySelector('form[action="/Admin/ApplyAction"]');
            const actionInput = document.getElementById('action-input');
            actionInput.value = this.dataset.action;
            form.dispatchEvent(new Event('submit'));
        });
    });
    
    const form = document.getElementById('admin-form');
    if (form) {
        form.addEventListener('submit', function (e) {
            if (!document.querySelectorAll('input[name="userIds"]:checked').length) {
                e.preventDefault();
                const alert = document.getElementById('error-alert');
                alert.classList.remove('d-none');
            }
        });
    }
}); 