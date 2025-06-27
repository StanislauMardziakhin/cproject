export function initSlideMenu() {
    const menu = document.querySelector('.slide-menu');
    if (!menu) return;

    document.querySelectorAll('tbody tr').forEach(row => {
        row.addEventListener('click', (e) => {
            if (e.target.tagName === 'INPUT') return;

            const id = row.dataset.id;
            if (!id) return;

            menu.dataset.id = id;
            menu.querySelector('.edit-btn').onclick = () => window.location.href = `/Templates/Edit/${id}`;
            menu.querySelector('.view-btn').onclick = () => window.location.href = `/Templates/View/${id}`;

            const rect = row.getBoundingClientRect();
            menu.style.top = `${rect.top + window.scrollY}px`;
            menu.style.left = `${rect.right + 10}px`;
            menu.classList.remove('d-none');
        });
    });

    document.addEventListener('click', (e) => {
        if (!e.target.closest('tbody tr') && !e.target.closest('.slide-menu')) {
            menu.classList.add('d-none');
        }
    });
}