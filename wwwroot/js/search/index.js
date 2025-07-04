import { initSortableTable } from '../shared/sortable-table.js';

document.addEventListener('DOMContentLoaded', () => {
    initSortableTable('.table', {
        sortableColumns: [0, 1, 2, 3],
        defaultSort: { column: 0, direction: 'asc' }
    });
}); 