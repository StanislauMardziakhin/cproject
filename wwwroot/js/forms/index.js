import { initSelectAll } from '../shared/select-all.js';
import { initClickableRows } from '../shared/clickable-row.js';
import { initFormBulkActions } from '../shared/form-bulk-actions.js';
import { initSortableTable } from '../shared/sortable-table.js';

document.addEventListener('DOMContentLoaded', () => {
    initSelectAll('formIds');
    initClickableRows();
    initFormBulkActions();
    
    initSortableTable('.table', {
        sortableColumns: [1, 2],
        defaultSort: { column: 2, direction: 'desc' }
    });
});