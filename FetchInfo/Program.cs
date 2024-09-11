using FetchInfo;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using User = FetchInfo.User;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.dev.json", optional: true)
    .Build();

// access token copied from graph explorer
var accessToken = configuration["AzureStuff:TemporaryAccessToken"] ??
                  throw new InvalidOperationException("No access token found");

var graphUsers = await Fetcher.FetchUsersAsync(accessToken);
var usersWithPicture = new List<User>();

foreach (var user in graphUsers.Where(u => u.Id != null && u.DisplayName != null))
{
    Console.WriteLine(user.DisplayName);

    try
    {
        var photosFolderPath = Path.Combine(AppContext.BaseDirectory, "Photos");
        Directory.CreateDirectory(photosFolderPath);
        
        var client = GraphServiceClientFactory.Construct(accessToken);
        var photoStream = await client.Users[user.Id].Photo.Content.GetAsync();
        if (photoStream is null)
        {
            Console.WriteLine($"No photo found for {user.DisplayName}");
            continue;
        }

        await ImageSaver.SaveImageAsync(photoStream, $"{user.Id}.jpg");
        
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

Console.ReadLine();