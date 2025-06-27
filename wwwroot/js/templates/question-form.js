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
                console.log("data.question:", data.question);
                console.log("q.type:", data.question.type);
                console.log("localized:", window.localization.QuestionTypes[data.question.type]);
                if (!data.succeeded) {
                    alert(data.error || window.localization.ErrorInvalidInput);
                    return;
                }

                const q = data.question;
                const question = {
                    id: q.id,
                    title: $('<div>').text(q.title).html(),
                    type: $('<div>').text(window.localization.QuestionTypes[q.type]),
                    description: q.description ? $('<div>').text(q.description).html() : '',
                    isVisibleInResults: q.isVisibleInResults
                };

                $('#question-list').append(`
                    <li class="list-group-item" data-id="${question.id}">
                        ${question.title} (${question.type})${question.isVisibleInResults ? '' : ' [' + window.localization.HiddenInResults + ']'}
                        <small>${question.description}</small>
                        <div class="float-end">
                            <a href="/Templates/EditQuestion/${question.id}" class="btn btn-sm btn-primary">${window.localization.Edit}</a>
                            <a href="/Templates/DeleteQuestion/${question.id}" class="btn btn-sm btn-danger" onclick="return confirm('${window.localization.ConfirmDelete}')">${window.localization.Delete}</a>
                        </div>
                    </li>
                `);

                form[0].reset();
                form.find('input[type="checkbox"]').prop('checked', true);
                form.find('select').val('');
                alert(window.localization.SuccessQuestionAdded);
            },
            error: function () {
                alert(window.localization.ErrorInvalidInput);
            }
        });
    });
}