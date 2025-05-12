using Azure.Storage.Blobs;
using Azure.Storage.Sas;

public class BlobService
{
    private readonly BlobContainerClient _containerClient;
    private const string ContainerName = "pbl3container/";

    public BlobService(IConfiguration configuration)
    {
        //Lấy đường dẫn cho thằng Azure Blob Storage từ .env
        var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
        var containerName = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONTAINER_NAME");

        //Tạo một blob container 
        _containerClient = new BlobContainerClient(connectionString, containerName);
    }

    //Hàm upload file lên Azure Blob Storage
    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        //Reset stream trước khi upload
        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }

        var blobClient = _containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(fileStream, overwrite: true);

        return blobClient.Uri.ToString();
    }

    //Hàm tạo SAS token cho blob
    public async Task<string> GetBlobSasUrlAsync(string blobName)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        if (!await blobClient.ExistsAsync())
        {
            return null;
        }

        var sasBuilder = new BlobSasBuilder()
        {
            BlobName = blobName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddHours(-4),
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);
        var sasToken = blobClient.GenerateSasUri(sasBuilder);

        return sasToken.ToString();
    }

    // ✅ Hàm chuẩn: Lấy ảnh có SAS hoặc giữ nguyên nếu là ảnh default
    public async Task<string> GetSafeImageUrlAsync(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return null;

        // Nếu ảnh là default (vd: /image/default-cover.jpg) thì giữ nguyên
        if (!imageUrl.StartsWith("https://")) return imageUrl;

        // Extract blob name
        string blobName = ExtractBlobName(imageUrl);
        if (string.IsNullOrEmpty(blobName)) return imageUrl;

        // Tạo SAS URL
        var sasUrl = await GetBlobSasUrlAsync(blobName);
        return string.IsNullOrEmpty(sasUrl) ? imageUrl : sasUrl;
    }

    // Hàm extract blob name
    private string ExtractBlobName(string url)
    {
        int index = url.IndexOf(ContainerName);
        return (index != -1) ? url.Substring(index + ContainerName.Length) : url;
    }
}
