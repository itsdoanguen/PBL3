// user-settings.js - Hiệu ứng nhỏ cho trang settings

document.addEventListener('DOMContentLoaded', function () {
    // Hiệu ứng click nhẹ cho các option
    document.querySelectorAll('.settings-option').forEach(function(opt) {
        opt.addEventListener('mousedown', function() {
            opt.style.transform = 'scale(0.97)';
        });
        opt.addEventListener('mouseup', function() {
            opt.style.transform = '';
        });
        opt.addEventListener('mouseleave', function() {
            opt.style.transform = '';
        });
    });
});
