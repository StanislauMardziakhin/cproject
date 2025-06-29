const AUTO_CLOSE_DELAY = 4000;

document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.alert').forEach(alert => {
        setTimeout(() => {
            new bootstrap.Alert(alert).close();
        }, AUTO_CLOSE_DELAY);
    });
    const themeDropdown = document.getElementById('themeDropdown');
    if (themeDropdown) {
        const theme = document.cookie.split('; ').find(row => row.startsWith('theme='))?.split('=')[1] || 'light';
        document.documentElement.setAttribute('data-theme', theme);

        document.querySelectorAll('.theme-switch').forEach(button => {
            button.addEventListener('click', () => {
                const theme = button.dataset.theme;
                document.documentElement.setAttribute('data-theme', theme);
                document.cookie = `theme=${theme}; path=/; max-age=31536000`;
            });
        });
    }
});