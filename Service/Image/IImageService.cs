namespace PBL3.Service.Image
{
    public interface IImageService
    {
        Task<(bool isSuccess, string? errorMessage, string? imageUrl)> UploadValidateImageAsync(IFormFile imageFile, string folderName, int maxSizeMB = 1);
    }
}
