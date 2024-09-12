using FetchInfo;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using User = FetchInfo.User;


var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.dev.json", optional: true)
    .Build();

// access token copied from graph explorer
var accessToken = configuration["GraphAPI:AccessToken"] ??
                  throw new InvalidOperationException("No access token found");

var graphUsers = await Fetcher.FetchUsersAsync(accessToken);
var usersWithPicture = new List<User>();
var photosFolderPath = Path.Combine(AppContext.BaseDirectory, "Photos");
Directory.CreateDirectory(photosFolderPath);

foreach (var user in graphUsers.Where(u => u.Id != null && u.DisplayName != null))
{
    Console.WriteLine(user.DisplayName);

    try
    {
        var client = GraphServiceClientFactory.Construct(accessToken);
        var photoStream = await client.Users[user.Id].Photo.Content.GetAsync();
        if (photoStream is null)
        {
            Console.WriteLine($"No photo found for {user.DisplayName}");
            continue;
        }

        var photoPath = await ImageManager.SaveImageOnDiskAsync(photoStream, $"{user.Id}.jpg");
        await ImageManager.SaveImageToBlobStorageAsync(configuration, photoPath);
        usersWithPicture.Add(new User(user.Id!, user.DisplayName!, photoPath));
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

ISaveUsers saveUsers = new SaveUsersToJsonFile();
await saveUsers.Save(usersWithPicture);
Console.WriteLine("Users saved");

Console.ReadLine();