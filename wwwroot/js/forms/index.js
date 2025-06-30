import { initSelectAll } from '../shared/select-all.js';
import { initClickableRows } from '../shared/clickable-row.js';
import { initFormBulkActions } from '../shared/form-bulk-actions.js';

document.addEventListener('DOMContentLoaded', () => {
    initSelectAll();
    initClickableRows();
    initFormBulkActions();
});