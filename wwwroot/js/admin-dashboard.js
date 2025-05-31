document.addEventListener('DOMContentLoaded', function () {
    // Hiệu ứng highlight khi hover vào các box
    document.querySelectorAll('.overview-item, .stat-box').forEach(function (el) {
        el.addEventListener('mouseenter', function () {
            el.style.boxShadow = '0 4px 16px rgba(0,0,0,0.10)';
            el.style.transform = 'translateY(-2px) scale(1.03)';
        });
        el.addEventListener('mouseleave', function () {
            el.style.boxShadow = '';
            el.style.transform = '';
        });
    });
    // Có thể thêm hiệu ứng hoặc chart ở đây nếu cần
});
