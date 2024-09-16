using Azure.Storage.Blobs;
using FetchInfo;
using Microsoft.Graph;
using Microsoft.Graph.Models.ODataErrors;
using InvalidOperationException = System.InvalidOperationException;
using User = FetchInfo.User;

public class Worker(IConfiguration configuration, ImageManager imageManager, BlobServiceClient blobServiceClient, SaveUsers saveUsers)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // access token copied from graph explorer
        var accessToken = configuration["GraphAPI:AccessToken"] ??
                          throw new InvalidOperationException("No access token found");

        var graphUsers = await Fetcher.FetchUsersAsync(accessToken);
        var usersWithPicture = new List<User>();

        await MakeSureContainersExistAsync();

        foreach (var user in graphUsers.Where(u => u.Id != null && u.DisplayName != null))
        {
            Console.WriteLine(user.DisplayName);

            try
            {
                var client = GraphServiceClientFactory.Construct(accessToken);
                var photoRequestBuilder = client.Users[user.Id].Photo;
                var profilePhoto = await photoRequestBuilder.GetAsync(cancellationToken: stoppingToken);
                var mimeType = profilePhoto.AdditionalData["@odata.mediaContentType"] as string;
                //Convert mimetype to file extension
                var fileExtension = ConvertMimetypeToFileExtension(mimeType);
                var photoStream = await photoRequestBuilder.Content.GetAsync(cancellationToken: stoppingToken);
                if (photoStream is null)
                {
                    Console.WriteLine($"No photo found for {user.DisplayName}");
                    continue;
                }

                await imageManager.SaveImageToBlobStorageAsync(photoStream, $"{user.Id}.{fileExtension}");
                usersWithPicture.Add(new User{Id = user.Id!, Name = user.DisplayName!});
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Could not fetch photo for {user.DisplayName}: {ex.Message}");
            }
            catch (ODataError ex) when (ex.Message.Contains("ImageNotFoundException"))
            {
                Console.WriteLine($"No photo found for {user.DisplayName}");
            }
        }

        await saveUsers.SaveUsersToBlobStorageAsync(usersWithPicture);
        
        Console.WriteLine("Users' pictures uploaded to blob storage and users.json file uploaded to cosmos db.");
        Console.ReadLine();
    }

    private async Task MakeSureContainersExistAsync()
    {
        var photosBlobContainerClient = blobServiceClient.GetBlobContainerClient("photos");
        var usersBlobContainerClient = blobServiceClient.GetBlobContainerClient("users");

        await photosBlobContainerClient.CreateIfNotExistsAsync();
        await usersBlobContainerClient.CreateIfNotExistsAsync();
    }

    private static string ConvertMimetypeToFileExtension(string? mimeType)
    {
        return mimeType switch
        {
            "image/jpeg" => "jpg",
            "image/png" => "png",
            "image/gif" => "gif",
            _ => "jpg"
        };
    }
}