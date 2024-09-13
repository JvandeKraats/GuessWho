using FetchInfo;
using Microsoft.Graph;
using InvalidOperationException = System.InvalidOperationException;
using User = FetchInfo.User;

public class Worker(IConfiguration configuration, ImageManager imageManager) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // access token copied from graph explorer
        var accessToken = configuration["GraphAPI:AccessToken"] ??
                          throw new InvalidOperationException("No access token found");

        var graphUsers = await Fetcher.FetchUsersAsync(accessToken);
        var usersWithPicture = new List<User>();

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
                usersWithPicture.Add(new User(user.Id!, user.DisplayName!));
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Could not fetch photo for {user.DisplayName}: {ex.Message}");
            }
            catch (Microsoft.Graph.Models.ODataErrors.ODataError ex) when (ex.Message.Contains("ImageNotFoundException"))
            {
                Console.WriteLine($"No photo found for {user.DisplayName}");
            }
        }

        var saveUsers = new SaveUsers();
        var filePath = saveUsers.SaveToJsonFile(usersWithPicture);
        await saveUsers.SaveUsersDataToCosmos(configuration, filePath);

        Console.WriteLine("Users successfully saved locally and uploaded to blob storage.");

        Console.ReadLine();
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


