using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace FetchInfo;

public class ImageManager(BlobServiceClient client)
{
    public async Task SaveImageToBlobStorageAsync(Stream stream, string fileName)
    {
        var containerClient = client.GetBlobContainerClient("photos");

        // Create the container if it does not exist
        await containerClient.CreateIfNotExistsAsync();

        // Upload the file
        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(stream, true);
    }
}