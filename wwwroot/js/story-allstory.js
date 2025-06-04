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
            const follow = card.getAttribute('data-follow') || '';
            let html = `<h5>${title}</h5>`;
            if (cover) html = `<img src='${cover}' alt='${title}' />` + html;
            html += `<div class='popup-meta'>Số từ: ${words ? Number(words).toLocaleString() : 0}</div>`;
            html += `<div class='popup-meta'>Lượt xem: ${views ? Number(views).toLocaleString() : 0}</div>`;
            html += `<div class='popup-meta'>Lượt thích: ${likes ? Number(likes).toLocaleString() : 0}</div>`;
            html += `<div class='popup-meta'>Theo dõi: ${follow ? Number(follow).toLocaleString() : 0}</div>`;
            html += `<div class='popup-desc'>${desc || ''}</div>`;
            popup.innerHTML = html;
            popup.style.display = 'block';
            popup.style.opacity = 0;
            popup.style.transition = 'opacity 0.18s, left 0.18s, top 0.18s, box-shadow 0.18s';
            setTimeout(() => {
                popup.style.opacity = 1;
                popup.style.boxShadow = '0 12px 40px rgba(33,150,243,0.22)';
                popup.style.transform = 'scale(1.03)';
            }, 10);
            const popupRect = popup.getBoundingClientRect();
            let top = window.scrollY + rect.top;
            let left = window.scrollX + rect.right + 12; 
            if (left + popupRect.width > window.innerWidth) {
                left = window.scrollX + rect.left - popupRect.width - 12;
            }
            if (left < 0) left = 10;
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
                popup.style.transform = 'scale(0.98)';
                popup.style.boxShadow = '0 2px 10px rgba(33,150,243,0.13)';
                setTimeout(() => { popup.style.display = 'none'; }, 180);
            }, 120);
        });
    });
});
