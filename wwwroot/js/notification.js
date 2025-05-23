document.addEventListener('DOMContentLoaded', function() {
    initializeNotifications();
});

function initializeNotifications() {
    const tabButtons = document.querySelectorAll('.tab-button');
    tabButtons.forEach(button => {
        button.addEventListener('click', () => {
            // Update active tab
            tabButtons.forEach(btn => btn.classList.remove('active'));
            button.classList.add('active');

            // Filter notifications
            const type = button.getAttribute('data-type');
            filterNotifications(type);
        });
    });
    
    // Initial count update
    updateUnreadCounts();
}

function updateUnreadCounts() {
    const notifications = document.querySelectorAll('.notification-item');
    const counts = {
        all: 0,
        NewFollower: 0,
        NewStory: 0,
        NewChapter: 0,
        NewComment: 0,
        NewReplyComment: 0
    };

    // Count unread notifications by type
    notifications.forEach(notification => {
        if (notification.classList.contains('unread')) {
            const type = notification.getAttribute('data-type');
            counts[type]++;
            counts.all++;
        }
    });

    // Update count badges
    const tabButtons = document.querySelectorAll('.tab-button');
    tabButtons.forEach(button => {
        const type = button.getAttribute('data-type');
        const countBadge = button.querySelector('.unread-count');
        const count = counts[type] || 0;
        
        if (count > 0) {
            countBadge.textContent = count;
            countBadge.style.display = '';
        } else {
            countBadge.style.display = 'none';
        }
    });
}

function filterNotifications(type) {
    const notifications = document.querySelectorAll('.notification-item');
    notifications.forEach(notification => {
        const notificationType = notification.getAttribute('data-type');
        if (type === 'all' || type === notificationType) {
            notification.style.display = '';
        } else {
            notification.style.display = 'none';
        }
    });
}
