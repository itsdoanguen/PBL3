// history.js - Hiệu ứng hover cho thẻ truyện đã đọc

document.addEventListener('DOMContentLoaded', function () {
    // Hiệu ứng hover: làm nổi bật tên truyện khi hover vào card
    document.querySelectorAll('.card.history-card').forEach(function(card) {
        card.addEventListener('mouseenter', function () {
            const titleDiv = card.parentElement.querySelector('.mt-2.text-center.fw-semibold');
            if (titleDiv) {
                titleDiv.style.color = '#4fc3f7';
            }
            card.style.boxShadow = '0 12px 32px rgba(33,150,243,0.18)';
            card.style.transform = 'translateY(-4px) scale(1.025)';
        });
        card.addEventListener('mouseleave', function () {
            const titleDiv = card.parentElement.querySelector('.mt-2.text-center.fw-semibold');
            if (titleDiv) {
                titleDiv.style.color = '#1976d2';
            }
            card.style.boxShadow = '';
            card.style.transform = '';
        });
    });
});
