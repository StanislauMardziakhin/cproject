const AUTO_CLOSE_DELAY = 4000;

document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.alert').forEach(alert => {
        setTimeout(() => {
            new bootstrap.Alert(alert).close();
        }, AUTO_CLOSE_DELAY);
    });
});