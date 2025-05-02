
namespace PBL3.Service
{
    public class ImageService : IImageService
    {
        private readonly BlobService _blobService;
        public ImageService(BlobService blobService)
        {
            _blobService = blobService;
        }

        public async Task<(bool isSuccess, string? errorMessage, string? imageUrl)> UploadValidateImageAsync(IFormFile imageFile, string folerName, int maxSizeMB = 1)
        {
            if (imageFile.Length > maxSizeMB * 1024 * 1024)
            {
                return (false, "Size ảnh phải nhỏ hơn 1MB", null);
            }
            var fileExtension = Path.GetExtension(imageFile.FileName);
            if (fileExtension != ".jpg" && fileExtension != ".jpeg" && fileExtension != ".png")
            {
                return (false, "Ảnh phải thuộc định dạng jpg, jpeg hoặc png", null);
            }
            var fileName = $"{folerName}/{Guid.NewGuid()}_image{fileExtension}";
            using (var stream = imageFile.OpenReadStream())
            {
                var result = await _blobService.UploadFileAsync(stream, fileName);
                return (true, null, result);
            }
        }
    }
}
