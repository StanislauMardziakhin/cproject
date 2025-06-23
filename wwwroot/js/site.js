document.addEventListener('DOMContentLoaded', function () {
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 4000);
    });
    const slideMenu = document.querySelector('.slide-menu');
    if (!slideMenu) {
        console.error('Slide menu not found in DOM');
        return;
    }

    const setupRowClick = (row) => {
        row.addEventListener('click', (e) => {
            if (e.target.tagName === 'INPUT') return;
            const id = row.dataset.id;
            if (!id) {
                console.error('Row ID not found:', row);
                return;
            }
            slideMenu.style.display = 'block';
            slideMenu.dataset.id = id;
            slideMenu.querySelector('.edit-btn').onclick = () => window.location.href = `/Templates/Edit/${id}`;
            slideMenu.querySelector('.view-btn').onclick = () => window.location.href = `/Templates/View/${id}`;
        });
    };

    const rows = document.querySelectorAll('tbody tr');
    if (rows.length === 0) {
        console.warn('No table rows found');
    } else {
        rows.forEach(setupRowClick);
    }
    
    const selectAll = document.querySelector('#selectAll');
    if (selectAll) {
        selectAll.addEventListener('change', () => {
            document.querySelectorAll('input[name="templateIds"]').forEach(checkbox => {
                checkbox.checked = selectAll.checked;
            });
        });
    } else {
        console.warn('Select all checkbox not found');
    }

    document.querySelectorAll('button[name="action"]').forEach(button => {
        button.addEventListener('click', (e) => {
            e.preventDefault();
            if (typeof window.localization === 'undefined') {
                console.error('Localization data not loaded');
                return;
            }
            const action = button.value.toLowerCase();
            const actionMessages = {
                delete: window.localization.delete,
                publish: window.localization.publish,
                unpublish: window.localization.unpublish
            };
            const message = `${window.localization.confirm} ${actionMessages[action] || ''}`;
            if (confirm(`${message}?`)) {
                const form = document.getElementById('templates-form');
                if (form) {
                    const checkedIds = Array.from(document.querySelectorAll('input[name="templateIds"]:checked'))
                        .map(cb => cb.value);
                    if (checkedIds.length === 0) {
                        alert('No templates selected');
                        return;
                    }
                    const templateIdsValue = checkedIds.join(',');
                    document.getElementById('selected-template-ids').value = templateIdsValue;
                    document.getElementById('action-input').value = button.value;
                    form.submit();
                } else {
                    console.error('Form not found');
                }
            }
        });
    });
    
    document.addEventListener('click', (e) => {
        if (!slideMenu.contains(e.target) && !e.target.closest('tbody tr')) {
            slideMenu.style.display = 'none';
        }
    });
});