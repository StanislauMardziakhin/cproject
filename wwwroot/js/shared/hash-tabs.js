export function initTabFromHash() {
    const hash = window.location.hash;
    if (!hash) return;

    const tabTrigger = document.querySelector(`[data-bs-toggle="tab"][href="${hash}"]`);
    if (tabTrigger) {
        tabTrigger.click();
    }
}