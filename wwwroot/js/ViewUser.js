// ViewUser.js
// Thêm các script tương tác nếu cần
// Ví dụ: highlight section, xử lý sự kiện, v.v.
// Phân trang bình luận trong ViewUser

document.addEventListener('DOMContentLoaded', function () {
    var table = document.querySelector('.user-comments-section table');
    if (!table) return;
    var tbody = table.querySelector('tbody');
    if (!tbody) return;
    var rows = Array.from(tbody.querySelectorAll('tr'));
    var pageSize = 10;
    var currentPage = 1;
    var totalPages = Math.ceil(rows.length / pageSize);

    function renderPage(page) {
        rows.forEach(function(row, idx) {
            row.style.display = (idx >= (page - 1) * pageSize && idx < page * pageSize) ? '' : 'none';
        });
        // Update pagination info
        var info = document.getElementById('comment-pagination-info');
        if (info) info.textContent = 'Trang ' + page + ' / ' + totalPages;
        // Update button state
        var prevBtn = document.getElementById('comment-prev-btn');
        var nextBtn = document.getElementById('comment-next-btn');
        if (prevBtn) prevBtn.disabled = page === 1;
        if (nextBtn) nextBtn.disabled = page === totalPages;
    }

    // Tạo nút phân trang nếu cần
    if (rows.length > pageSize) {
        var nav = document.createElement('div');
        nav.className = 'd-flex justify-content-center align-items-center gap-2 mt-2';
        nav.innerHTML = '<button id="comment-prev-btn" class="btn btn-outline-secondary btn-sm">&laquo; Trước</button>' +
            '<span id="comment-pagination-info">Trang 1 / ' + totalPages + '</span>' +
            '<button id="comment-next-btn" class="btn btn-outline-secondary btn-sm">Sau &raquo;</button>';
        table.parentElement.appendChild(nav);
        document.getElementById('comment-prev-btn').onclick = function () {
            if (currentPage > 1) {
                currentPage--;
                renderPage(currentPage);
            }
        };
        document.getElementById('comment-next-btn').onclick = function () {
            if (currentPage < totalPages) {
                currentPage++;
                renderPage(currentPage);
            }
        };
    }
    renderPage(currentPage);
});
