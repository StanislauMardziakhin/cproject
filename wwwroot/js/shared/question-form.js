import { showToast } from './toast.js';

export function initQuestionForm() {
    const addForm = $('#add-question-form');
    const editForm = $('#edit-question-form');
    
    if (addForm.length) {
        initForm(addForm, 'add');
    }
    
    if (editForm.length) {
        initForm(editForm, 'edit');
    }
}

function initForm(form, type) {
    $.validator.unobtrusive.parse(form);

    $(document).off('submit', `#${form.attr('id')}`).on('submit', `#${form.attr('id')}`, function (e) {
        e.preventDefault();
        
        const typeSelect = form.find('select[name="Type"]');
        if (typeSelect.length && !typeSelect.val()) {
            showToast(window.localization.RequiredField || 'Required field', 'danger');
            typeSelect.focus();
            return;
        }

        if (!form.valid()) {
            return;
        }

        $.ajax({
            url: form.attr('action'),
            method: 'POST',
            data: form.serialize(),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val(),
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (data) {
                if (!data.succeeded) {
                    showToast(data.error || window.localization.ErrorInvalidInput, 'danger');
                    return;
                }

                if (type === 'edit') {
                    showToast(data.message || window.localization.SuccessQuestionUpdated, 'success');
                    const modal = form.closest('.modal');
                    if (modal.length) {
                        modal.modal('hide');
                    }
                    setTimeout(() => window.location.reload(), 1000);
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
                                <li><a class="dropdown-item edit-question-link" href="/TemplateQuestions/EditQuestion/${question.id}">${window.localization.Edit}</a></li>
                                <li><a class="dropdown-item text-danger" href="/TemplateQuestions/DeleteQuestion/${question.id}">${window.localization.Delete}</a></li>
                            </ul>
                        </div>
                    </li>
                `);

                form[0].reset();
                form.find('input[type="checkbox"]').prop('checked', true);
                form.find('select').val('');
                showToast(window.localization.SuccessQuestionAdded, 'success');
            },
            error: function (xhr, status, error) {
                showToast(window.localization.ErrorInvalidInput, 'danger');
            }
        });
    });
}