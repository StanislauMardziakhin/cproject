export function initSortableTable(tableSelector, options = {}) {
    const table = document.querySelector(tableSelector);
    if (!table) return;

    const defaultOptions = {
        sortableColumns: [],
        defaultSort: { column: 0, direction: 'asc' },
        preserveRowData: false
    };

    const config = { ...defaultOptions, ...options };
    const tbody = table.querySelector('tbody');
    const headers = table.querySelectorAll('thead th');
    
    headers.forEach((header, index) => {
        if (config.sortableColumns.length === 0 || config.sortableColumns.includes(index)) {
            header.style.cursor = 'pointer';
            header.classList.add('sortable');
            
            const icon = document.createElement('i');
            icon.className = 'bi bi-arrow-down-up ms-1';
            icon.style.opacity = '0.5';
            header.appendChild(icon);
            
            header.addEventListener('click', () => {
                sortTable(index);
            });
        }
    });

    function sortTable(columnIndex) {
        const rows = Array.from(tbody.querySelectorAll('tr'));
        const currentDirection = getCurrentSortDirection(columnIndex);
        const newDirection = currentDirection === 'asc' ? 'desc' : 'asc';
        
        updateSortIcons(columnIndex, newDirection);
        
        rows.sort((a, b) => {
            const aValue = getCellValue(a, columnIndex);
            const bValue = getCellValue(b, columnIndex);
            
            let comparison = 0;
            if (aValue < bValue) comparison = -1;
            if (aValue > bValue) comparison = 1;
            
            return newDirection === 'asc' ? comparison : -comparison;
        });
        
        rows.forEach(row => tbody.appendChild(row));
    }

    function getCellValue(row, columnIndex) {
        const cell = row.cells[columnIndex];
        if (!cell) return '';
        
        let text = cell.textContent || cell.innerText || '';
        
        text = text.trim();
        
        const number = parseFloat(text);
        if (!isNaN(number)) return number;
        
        const date = new Date(text);
        if (!isNaN(date.getTime())) return date.getTime();
        
        return text.toLowerCase();
    }

    function getCurrentSortDirection(columnIndex) {
        const header = headers[columnIndex];
        const icon = header.querySelector('i');
        
        if (icon.classList.contains('bi-arrow-up')) return 'asc';
        if (icon.classList.contains('bi-arrow-down')) return 'desc';
        return 'asc';
    }

    function updateSortIcons(activeColumnIndex, direction) {
        headers.forEach((header, index) => {
            const icon = header.querySelector('i');
            if (!icon) return;
            
            if (index === activeColumnIndex) {
                icon.className = direction === 'asc' 
                    ? 'bi bi-arrow-up ms-1' 
                    : 'bi bi-arrow-down ms-1';
                icon.style.opacity = '1';
            } else {
                icon.className = 'bi bi-arrow-down-up ms-1';
                icon.style.opacity = '0.5';
            }
        });
    }
    
    if (config.defaultSort) {
        sortTable(config.defaultSort.column);
    }
} 