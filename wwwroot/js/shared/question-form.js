import { showToast } from './toast.js';
export function initQuestionForm() {
    const form = $('#add-question-form');
    if (!form.length) return;

    $.validator.unobtrusive.parse(form);

    $(document).off('submit', '#add-question-form').on('submit', '#add-question-form', function (e) {
        e.preventDefault();

        if (!form.valid()) return;

        $.ajax({
            url: form.attr('action'),
            method: 'POST',
            data: form.serialize(),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (data) {
                if (!data.succeeded) {
                    showToast(data.error || window.localization.ErrorInvalidInput, 'danger');
                    return;
                }

                const q = data.question;
                const question = {
                    id: q.id,
                    title: $('<div>').text(q.title).html(),
                    type: window.localization.QuestionTypes[q.type],
                    description: q.description ? $('<div>').text(q.description).html() : '',
                    isVisibleInResults: q.isVisibleInResults
                };

                $('#question-list').append(`
                    <li class="list-group-item d-flex justify-content-between align-items-start" data-id="${question.id}">
                        <div>
                            <strong>${question.title}</strong>
                            ${question.isVisibleInResults ? '' : `<span class="badge bg-secondary">${window.localization.HiddenInResults}</span>`}
                            <div class="text-muted small">${question.description}</div>
                        </div>
                        <div class="dropdown">
                            <button class="btn btn-sm btn-light" data-bs-toggle="dropdown" aria-expanded="false">
                                <i class="bi bi-three-dots-vertical"></i>
                            </button>
                            <ul class="dropdown-menu dropdown-menu-end">
                                <li><a class="dropdown-item edit-question-link" href="/Templates/EditQuestion/${question.id}">${window.localization.Edit}</a></li>
                                <li><a class="dropdown-item text-danger" href="/Templates/DeleteQuestion/${question.id}">${window.localization.Delete}</a></li>
                            </ul>
                        </div>
                    </li>
                `);

                form[0].reset();
                form.find('input[type="checkbox"]').prop('checked', true);
                form.find('select').val('');
                showToast(window.localization.SuccessQuestionAdded, 'success');
            },
            error: function () {
                showToast(window.localization.ErrorInvalidInput, 'danger');
            }
        });
    });
}