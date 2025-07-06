import { showToast, handleAjaxError, removeListPlaceholder, buildUserListItem } from '../shared/utils.js';
$(function () {
    const $userSearch = $('#userSearch');
    const $userList = $('#userList');
    const templateId = $userList.data('template-id');
    const token = $('input[name="__RequestVerificationToken"]').val();

    $userSearch.select2({
        placeholder: $userSearch.data('placeholder'),
        ajax: {
            url: '/TemplateAccess/SearchUsers',
            dataType: 'json',
            data: params => ({query: params.term}),
            processResults: data => {
                const users = Array.isArray(data) ? data : (data && data.users ? data.users : []);
                return {
                    results: users.map(item => ({
                        id: item.Id || item.id,
                        text: `${item.Name || item.name || 'Unknown'} (${item.Email || item.email || 'No email'})`,
                        name: item.Name || item.name || 'Unknown',
                        email: item.Email || item.email || 'No email'
                    }))
                };
            }
        },
        minimumInputLength: 1,
        language: {
            inputTooShort: () => ''
        },
        templateResult: formatUser,
        templateSelection: formatUserSelection
    });

    function formatUser(user) {
        if (!user.id) {
            return user.text;
        }
        return $(`<span>${user.name} (<i>${user.email}</i>)</span>`);
    }

    function formatUserSelection(user) {
        return user.name || user.text;
    }

    $userSearch.on('select2:select', function (e) {
        const data = e.params.data;
        const existing = $userList.find(`[data-user-id="${data.id}"]`);
        if (existing.length > 0) {
            showToast(window.localization?.UserAlreadyAdded || 'User already added', 'warning');
            $userSearch.val(null).trigger('change');
            return;
        }
        $.post({
            url: '/TemplateAccess/AddUserAccess',
            data: {templateId, userId: data.id},
            headers: {'RequestVerificationToken': token},
            success: function () {
                removeListPlaceholder($userList);
                const li = buildUserListItem(data, window.localization);
                $userList.append(li);
                $userSearch.val(null).trigger('change');
                sortUserListByName();
            },
            error: handleAjaxError
        });
    });

    $userList.on('click', '.remove-user', function () {
        const userId = $(this).data('user-id');
        $.post({
            url: '/TemplateAccess/RemoveUserAccess',
            data: {templateId, userId},
            headers: {'RequestVerificationToken': token},
            success: function () {
                $userList.find(`[data-user-id="${userId}"]`).remove();
                if ($userList.children('li').length === 0) {
                    const noUsersText = window.localization?.NoUsersAdded || 'No users added';
                    $userList.append(`<li class="list-group-item text-muted">${noUsersText}</li>`);
                }
            },
            error: handleAjaxError
        });
    });

    function sortUserListByName() {
        const items = $userList.children('li').get();
        items.sort((a, b) => {
            const aName = ($(a).data('name') || '').toLowerCase();
            const bName = ($(b).data('name') || '').toLowerCase();
            return aName.localeCompare(bName);
        });
        $userList.append(items);
    }

    sortUserListByName();

    function sortUserList() {
        const sortBy = $('#sortOrder').val();
        const items = $userList.children('li').get();
        items.sort((a, b) => {
            const aValue = $(a).data(sortBy) || '';
            const bValue = $(b).data(sortBy) || '';
            return aValue.localeCompare(bValue);
        });
        $userList.append(items);
    }
});