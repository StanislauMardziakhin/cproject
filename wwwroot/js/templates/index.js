import {initTemplateBulkActions} from '../shared/bulk-actions.js';
import {initSlideMenu} from '../shared/slide-menu.js';
import {initSelectAll} from '../shared/select-all.js';
import {initModalLoader} from '../shared/modal-loader.js';
import {initQuestionForm} from '../shared/question-form.js';
import {initSortableList} from '../shared/sortable.js';
import {initClickableRows} from '../shared/clickable-row.js';

document.addEventListener('DOMContentLoaded', () => {
    initTemplateBulkActions();
    initSlideMenu();
    initSelectAll();
    initModalLoader();
    initQuestionForm();
    initSortableList();
    initClickableRows()
});