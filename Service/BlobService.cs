using Azure.Storage.Blobs;
using Azure.Storage.Sas;

public class BlobService
{
    private readonly BlobContainerClient _containerClient;

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
        //Tạo một blob mới để up ảnh lên
        var blobClient = _containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(fileStream, overwrite: true);

        return blobClient.Uri.ToString();
    }

    //Hàm tạo SAS token cho blob
    public async Task<String> GetBlobSasUrlAsync(string blobName)
    {

        //Tạo một blob để kiểm tra xem blob đang tìm kiếm có tồn tại trên container không
        var blobClient = _containerClient.GetBlobClient(blobName);
        if (!await blobClient.ExistsAsync())
        {
            return null;
        }

        //Tạo một SAS token với thời gian bắt đầu và kết thúc, ở đây -4 vì đang set Storage ở múi giờ UTC-4 nên phải trừ 4 giờ
        var sasBuilder = new BlobSasBuilder()
        {
            BlobName = blobName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddHours(-4),
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
        };

        //Cấp quyền đọc cho token
        sasBuilder.SetPermissions(BlobSasPermissions.Read);
        //Tạo SAS token
        var sasToken = blobClient.GenerateSasUri(sasBuilder);

        return sasToken.ToString();
    }
}
