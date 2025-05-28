// report-index.js - Enhance moderator report view UX

document.addEventListener('DOMContentLoaded', function () {
    // Highlight row on hover
    document.querySelectorAll('.table-report tbody tr').forEach(function (row) {
        row.addEventListener('mouseenter', function () {
            row.classList.add('table-primary');
        });
        row.addEventListener('mouseleave', function () {
            row.classList.remove('table-primary');
        });
    });

    // Smooth scroll to tab if hash present
    const url = new URL(window.location.href);
    const hash = url.hash;
    if (hash && hash.startsWith("#tab-pane-")) {
        const tabTrigger = document.querySelector(`button[data-bs-target='${hash}']`);
        if (tabTrigger) {
            new bootstrap.Tab(tabTrigger).show();
            setTimeout(() => {
                document.querySelector(hash)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }, 200);
        }
    }
});
