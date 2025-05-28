$(document).ready(function () {
    $('.report-form').on('submit', function (e) {
        e.preventDefault();
        var form = $(this);
        var modal = form.closest('.modal');
        
        $.ajax({
            url: form.attr('action'),
            method: 'POST',
            data: form.serialize(),
            success: function (response) {
                // Đóng modal
                modal.modal('hide');
                
                // Reset form
                form.trigger('reset');
                
                // Hiển thị thông báo thành công
                toastr.success('Báo cáo đã được gửi thành công');
            },
            error: function (xhr) {
                // Hiển thị thông báo lỗi
                toastr.error(xhr.responseText || 'Đã có lỗi xảy ra khi gửi báo cáo');
            }
        });
    });
});
