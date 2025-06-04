namespace PBL3.Service.Image
{
    public class ImageService : IImageService
    {
        private readonly BlobService _blobService;
        public ImageService(BlobService blobService)
        {
            _blobService = blobService;
        }

        public async Task<(bool isSuccess, string? errorMessage, string? imageUrl)> UploadValidateImageAsync(IFormFile imageFile, string folderName, int maxSizeMB = 1)
        {
            try
            {
                // Validate file exists
                if (imageFile == null || imageFile.Length == 0)
                {
                    return (false, "Vui lòng chọn file ảnh", null);
                }

                // Validate file size
                if (imageFile.Length > maxSizeMB * 1024 * 1024)
                {
                    return (false, $"Kích thước file phải nhỏ hơn {maxSizeMB}MB", null);
                }

                // Validate file extension
                var fileExtension = Path.GetExtension(imageFile.FileName)?.ToLower();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                
                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                {
                    return (false, "Ảnh phải thuộc định dạng JPG, JPEG hoặc PNG", null);
                }

                // Validate content type
                var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
                if (!allowedContentTypes.Contains(imageFile.ContentType?.ToLower()))
                {
                    return (false, "Định dạng file không hợp lệ", null);
                }

                // Generate unique filename
                var fileName = $"{folderName}/{Guid.NewGuid()}_image{fileExtension}";
                
                // Upload to blob storage
                var uploadResult = await _blobService.UploadFileAsync(imageFile.OpenReadStream(), fileName);
                
                if (string.IsNullOrEmpty(uploadResult))
                {
                    return (false, "Lỗi khi tải ảnh lên server. Vui lòng thử lại", null);
                }

                return (true, null, uploadResult);
            }
            catch (Exception ex)
            {
                // Log exception here if you have logging
                return (false, "Có lỗi xảy ra khi xử lý ảnh. Vui lòng thử lại", null);
            }
        }
    }
}
