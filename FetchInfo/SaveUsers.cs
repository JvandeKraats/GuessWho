using System.Text.Json;
using Azure.Storage.Blobs;

namespace FetchInfo;

public class SaveUsers(BlobServiceClient blobServiceClient)
{
    public async Task SaveUsersToBlobStorageAsync(List<User> users)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient("users");
        
        // Convert user list to users.json file
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var jsonString = JsonSerializer.Serialize(users, options);
        // Convert JSON string to stream
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonString));

        // Upload the JSON file to Blob Storage
        var blobClient = containerClient.GetBlobClient("users.json");
        await blobClient.UploadAsync(stream, true);
    }
}