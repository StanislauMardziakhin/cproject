import {initTemplateBulkActions} from './bulk-actions.js';
import {initSlideMenu} from './slide-menu.js';
import {initSelectAll} from './select-all.js';
import {initModalLoader} from './modal-loader.js';
import {initQuestionForm} from './question-form.js';
import {initSortableList} from './sortable.js';

document.addEventListener('DOMContentLoaded', () => {
    initTemplateBulkActions();
    initSlideMenu();
    initSelectAll();
    initModalLoader();
    initQuestionForm();
    initSortableList();
});