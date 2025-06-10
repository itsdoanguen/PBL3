// admin-system.js
// Simple client-side pagination for tables
function paginateTable($table, pageSize) {
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

    $pagination.off('click').on('click', 'a', function (e) {
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
    // Chỉ phân trang cho bảng đang hiển thị trên mỗi tab khi load trang
    $('.user-table:visible').each(function () { paginateTable($(this), 10); });
    $('.story-table:visible').each(function () { paginateTable($(this), 10); });
    $('.genre-table:visible').each(function () { paginateTable($(this), 10); });

    // Re-paginate on tab shown, chỉ cho bảng trong tab vừa được hiển thị
    $('a[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
        setTimeout(function () {
            $('.user-table:visible').each(function () { paginateTable($(this), 10); });
            $('.story-table:visible').each(function () { paginateTable($(this), 10); });
        }, 100);
    });
});
