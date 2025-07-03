import Sortable from 'https://cdn.jsdelivr.net/npm/sortablejs@1.15.0/+esm';
import { showToast } from './toast.js';
export function initSortableList() {
    const questionList = document.getElementById('question-list');
    if (!questionList) return;

    Sortable.create(questionList, {
        animation: 150,
        handle: '.list-group-item',
        onEnd: function (evt) {
            const order = Array.from(evt.target.children).map((el, index) => ({
                id: parseInt(el.dataset.id),
                order: index + 1
            }));

            $.ajax({
                url: '/Templates/UpdateQuestionOrder',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({orders: order}),
                headers: {
                    "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
                },
                success: function () {
                    showToast(window.localization.SuccessOrderUpdated, 'success');
                }
            });
        }
    });
}