export function showToast(msg, type = 'info') {
    if (window.showToast) {
        window.showToast(msg, type);
        return;
    }
    alert(msg);
}

export function handleAjaxError(jqXHR, textStatus, errorThrown) {
    let msg = 'An error occurred.';
    if (jqXHR && jqXHR.responseJSON && jqXHR.responseJSON.error) {
        msg = jqXHR.responseJSON.error;
    } else if (errorThrown) {
        msg = errorThrown;
    }
    showToast(msg, 'danger');
}

export function removeListPlaceholder($list, placeholderClass = 'text-muted') {
    $list.find('.' + placeholderClass).remove();
}

export function buildUserListItem(user, localization = window.localization) {
    const removeText = localization?.Remove || 'Remove';
    return `
        <li class="list-group-item d-flex justify-content-between align-items-center"
            data-user-id="${user.id}" data-name="${user.name}" data-email="${user.email}">
            <span>${user.name} (${user.email})</span>
            <button type="button"
                class="btn btn-outline-danger btn-sm remove-user"
                title="${removeText}"
                aria-label="${removeText}"
                data-user-id="${user.id}">
            <i class="bi bi-x-lg"></i>
        </button>
        </li>`;
} 