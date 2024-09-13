using System.Text.Json;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace FetchInfo;

public class SaveUsers
{
    public string SaveToJsonFile(List<User> users)
    {
        var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "users.json");
        File.WriteAllText(filePath, json);

        return filePath;
    }
    
    public async Task SaveUsersDataToCosmos(IConfiguration configuration, string filePath)
    {
        var blobEndpoint = configuration["StorageAccount:BlobServiceEndpoint"];
        var tenantId = configuration["AzureAd:TenantId"];
        var clientId = configuration["AzureAd:ClientId"];
        var clientSecret = configuration["AzureAd:ClientSecret"];
        if (blobEndpoint is null)
            throw new InvalidOperationException("BlobServiceEndpoint not found in configuration");

        var client = new BlobServiceClient(new Uri(blobEndpoint), new ClientSecretCredential(tenantId, clientId, clientSecret));
        var containerClient = client.GetBlobContainerClient("users");

        // Create the container if it does not exist
        await containerClient.CreateIfNotExistsAsync();

        // Upload the file
        var blobClient = containerClient.GetBlobClient("users.json");
        await blobClient.UploadAsync(filePath, true);
    }
}