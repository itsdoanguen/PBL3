using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

public class BlobService
{
    private readonly BlobContainerClient _containerClient;

    public BlobService(IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
        var containerName = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONTAINER_NAME");
        _containerClient = new BlobContainerClient(connectionString, containerName);

        Console.WriteLine(containerName);
        Console.WriteLine(connectionString);
        Console.WriteLine($"Container URI: {_containerClient.Uri}");

        string testBlobName = "avatars/man1.jpg";
        string sasUrl = _containerClient.GetBlobClient(testBlobName).GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1)).ToString();

        Console.WriteLine($"SAS URL: {sasUrl}");
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        var blobClient = _containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(fileStream, overwrite: true);
        return blobClient.Uri.ToString();
    }

    public async Task<String> GetBlobSasUrlAsync(string blobName)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        Console.WriteLine($"Checking blob: {blobName}");

        if (!await blobClient.ExistsAsync())
        {
            return null;
        }

        var sasBuilder = new BlobSasBuilder()
        {
            BlobName = blobName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow,
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasToken = blobClient.GenerateSasUri(sasBuilder);

        return sasToken.ToString();
    }
}
