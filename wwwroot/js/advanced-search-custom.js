// Advanced Search UI Enhancements
$(document).ready(function () {
    // Hiệu ứng focus cho input/select
    $('.advanced-search-form .form-control, .advanced-search-form .form-select').on('focus', function () {
        $(this).closest('div').addClass('focus');
    }).on('blur', function () {
        $(this).closest('div').removeClass('focus');
    });

    // Hiệu ứng hover cho option (chỉ hỗ trợ tốt trên desktop)
    $('.advanced-search-form select[multiple] option').hover(
        function () { $(this).css('background', '#e3f0ff'); },
        function () { $(this).css('background', ''); }
    );

    // Tự động scroll đến kết quả khi submit
    $('.advanced-search-form').on('submit', function () {
        setTimeout(function () {
            $('html, body').animate({ scrollTop: $('.search-card-grid').offset().top - 60 }, 500);
        }, 300);
    });
});
