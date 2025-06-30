export function initModalLoader() {
    const modalElement = document.getElementById('questionModal');
    if (!modalElement) return;

    const modalBody = modalElement.querySelector('.modal-body');
    const modal = new bootstrap.Modal(modalElement);

    document.querySelectorAll('.edit-question-link').forEach(link => {
        link.addEventListener('click', async function (e) {
            e.preventDefault();
            modalBody.innerHTML = `<div class="text-center text-muted">
                <i class="bi bi-hourglass-split"></i> ${window.localization.Loading}
            </div>`;
            modal.show();

            try {
                const response = await fetch(this.href);
                const html = await response.text();
                modalBody.innerHTML = html;
            } catch (error) {
                modalBody.innerHTML = `<div class="alert alert-danger">${window.localization.ErrorInvalidInput}</div>`;
                console.error('Error loading modal content', error);
            }
        });
    });
}