// admin-system.js
// Simple client-side pagination for tables
function paginateTable(tableSelector, pageSize) {
    const $table = $(tableSelector);
    const $rows = $table.find('tbody tr');
    const $pagination = $table.next('nav').find('.pagination');
    let currentPage = 1;
    const totalRows = $rows.length;
    const totalPages = Math.ceil(totalRows / pageSize);

    function showPage(page) {
        $rows.hide();
        $rows.slice((page - 1) * pageSize, page * pageSize).show();
        $pagination.empty();
        for (let i = 1; i <= totalPages; i++) {
            $pagination.append(`<li class="page-item${i === page ? ' active' : ''}"><a class="page-link" href="#">${i}</a></li>`);
        }
    }

    $pagination.on('click', 'a', function (e) {
        e.preventDefault();
        const page = parseInt($(this).text());
        if (!isNaN(page)) {
            currentPage = page;
            showPage(currentPage);
        }
    });

    showPage(currentPage);
}

$(document).ready(function () {
    paginateTable('.user-table', 10);
    paginateTable('.story-table', 10);
    paginateTable('.genre-table', 10);
    // Re-paginate on tab shown
    $('a[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
        setTimeout(function () {
            paginateTable('.user-table:visible', 10);
            paginateTable('.story-table:visible', 10);
        }, 100);
    });
});
