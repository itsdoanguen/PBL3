// story-allstory.js
document.addEventListener('DOMContentLoaded', function () {
    const filter = document.getElementById('storyFilter');
    if (filter) {
        filter.addEventListener('change', function () {
            const query = filter.value;
            // Always reload with ?query=...&page=1
            window.location.href = `?query=${encodeURIComponent(query)}&page=1`;
        });
    }
    const pagination = document.getElementById('allStoriesPagination');
    if (pagination) {
        pagination.addEventListener('click', function (e) {
            if (e.target.tagName === 'A') {
                setTimeout(() => window.scrollTo({ top: 0, behavior: 'smooth' }), 100);
            }
        });
    }

    // Popup hover for story card
    const popup = document.getElementById('story-popup');
    let popupTimeout;
    document.querySelectorAll('.story-hover-card').forEach(function(card) {
        card.addEventListener('mouseenter', function (e) {
            clearTimeout(popupTimeout);
            const rect = card.getBoundingClientRect();
            const title = card.getAttribute('data-title') || '';
            const cover = card.getAttribute('data-cover') || '';
            const words = card.getAttribute('data-words') || '';
            const views = card.getAttribute('data-views') || '';
            const likes = card.getAttribute('data-likes') || '';
            const desc = card.getAttribute('data-description') || '';
            let html = `<h5>${title}</h5>`;
            if (cover) html = `<img src='${cover}' alt='${title}' />` + html;
            html += `<div class='popup-meta'>Số từ: ${words ? Number(words).toLocaleString() : 0}</div>`;
            html += `<div class='popup-meta'>Lượt xem: ${views ? Number(views).toLocaleString() : 0}</div>`;
            html += `<div class='popup-meta'>Lượt thích: ${likes ? Number(likes).toLocaleString() : 0}</div>`;
            html += `<div class='popup-desc'>${desc || ''}</div>`;
            popup.innerHTML = html;
            popup.style.display = 'block';
            popup.style.opacity = 1;
            // Vị trí popup: bên phải card, nếu chạm biên phải thì chuyển sang trái
            const popupRect = popup.getBoundingClientRect();
            let top = window.scrollY + rect.top;
            let left = window.scrollX + rect.right + 12; // bên phải card, cách 12px
            // Nếu popup vượt biên phải màn hình, chuyển sang bên trái card
            if (left + popupRect.width > window.innerWidth) {
                left = window.scrollX + rect.left - popupRect.width - 12;
            }
            // Nếu vẫn vượt biên trái, căn sát lề trái
            if (left < 0) left = 10;
            // Nếu popup vượt biên dưới, điều chỉnh top
            if (top + popupRect.height > window.scrollY + window.innerHeight) {
                top = window.scrollY + window.innerHeight - popupRect.height - 10;
            }
            if (top < window.scrollY) top = window.scrollY + 10;
            popup.style.top = top + 'px';
            popup.style.left = left + 'px';
        });
        card.addEventListener('mouseleave', function () {
            popupTimeout = setTimeout(function() {
                popup.style.opacity = 0;
                popup.style.display = 'none';
            }, 120);
        });
    });
});
