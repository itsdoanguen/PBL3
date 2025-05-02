namespace PBL3.Service
{
    public interface IImageService
    {
        Task<(bool isSuccess, string? errorMessage, string? imageUrl)> UploadValidateImageAsync(IFormFile imageFile, string folerName, int maxSizeMB = 1);
    }
}
