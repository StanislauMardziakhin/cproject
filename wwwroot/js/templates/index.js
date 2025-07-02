import {initTemplateBulkActions} from '../shared/bulk-actions.js';
import {initSlideMenu} from '../shared/slide-menu.js';
import {initSelectAll} from '../shared/select-all.js';
import {initModalLoader} from '../shared/modal-loader.js';
import {initQuestionForm} from '../shared/question-form.js';
import {initSortableList} from '../shared/sortable.js';
import {initClickableRows} from '../shared/clickable-row.js';
import {initTabFromHash} from '../shared/hash-tabs.js';
import { initComments } from './comments.js';
import { initLikes } from './likes.js';
import { initTags } from './tags.js';

document.addEventListener('DOMContentLoaded', () => {
    initTemplateBulkActions();
    initSlideMenu();
    initSelectAll('templateIds');
    initModalLoader();
    initQuestionForm();
    initSortableList();
    initClickableRows();
    initTabFromHash();

    const commentTarget = document.querySelector('[data-template-id]');
    if (commentTarget) {
        const templateId = Number(commentTarget.dataset.templateId);
        if (!isNaN(templateId)) {
            initComments(templateId);
            initSlideMenu(templateId);
        }
    }
    const tagsInput = document.querySelector('#tags-input');
    if (tagsInput) {
        initTags();
    }
});