using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace FetchInfo;

public static class ImageManager
{
    public static async Task<string> SaveImageOnDiskAsync(Stream imageStream, string fileName)
    {
        // Ensure the 'photos' directory exists
        var photosDirectory = Path.Combine(Directory.GetCurrentDirectory(), "photos");
        Directory.CreateDirectory(photosDirectory);

        // Combine the directory path with the file name
        var filePath = Path.Combine(photosDirectory, fileName);

        if(File.Exists(filePath))
        {
            Console.WriteLine($"File {fileName} already exists");
            return filePath;
        }
        
        // Save the image stream to the specified file
        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await imageStream.CopyToAsync(fileStream);
        return filePath;
    }


    public static async Task SaveImageToBlobStorageAsync(IConfigurationRoot configuration, string localFilePath)
    {
        var blobEndpoint = configuration["StorageAccount:BlobServiceEndpoint"];
        var tenantId = configuration["AzureAd:TenantId"];
        var clientId = configuration["AzureAd:ClientId"];
        var clientSecret = configuration["AzureAd:ClientSecret"];
        if (blobEndpoint is null)
            throw new InvalidOperationException("BlobServiceEndpoint not found in configuration");

        var client = new BlobServiceClient(new Uri(blobEndpoint), new ClientSecretCredential(tenantId, clientId, clientSecret));
        var containerClient = client.GetBlobContainerClient("photos");

        // Create the container if it does not exist
        await containerClient.CreateIfNotExistsAsync();

        // Upload the file
        var blobClient = containerClient.GetBlobClient(Path.GetFileName(localFilePath));
        await blobClient.UploadAsync(localFilePath, true);
    }
}