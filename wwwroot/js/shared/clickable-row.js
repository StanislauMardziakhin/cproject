export function initClickableRows() {
    document.querySelectorAll(".clickable-row").forEach(row => {
        row.addEventListener("click", e => {
            if (e.target.closest("input")) return;
            const url = row.getAttribute("data-href");
            if (url) window.location.href = url;
        });
    });
}