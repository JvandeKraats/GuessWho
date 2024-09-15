using Azure.Storage.Blobs;

namespace FetchInfo;

public class ImageManager(BlobServiceClient client)
{
    public async Task SaveImageToBlobStorageAsync(Stream stream, string fileName)
    {
        var containerClient = client.GetBlobContainerClient("photos");

        // Upload the file
        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(stream, true);
    }
}