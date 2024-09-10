using FetchInfo;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;

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
        var client = GraphServiceClientFactory.Construct(accessToken);
        var photoStream = await client.Users[user.Id].Photo.Content.GetAsync();
        using (var memoryStream = new MemoryStream())
        {
            await photoStream.CopyToAsync(memoryStream);
            var photoBytes = memoryStream.ToArray();
            // Save or process the photoBytes as needed
            Console.WriteLine($"Fetched photo for {user.DisplayName}");
            usersWithPicture.Add(new User(user.Id!, user.DisplayName!, $"./Photos/{user.Id}.jpg"));
        }
    }
    catch (ServiceException ex)
    {
        Console.WriteLine($"Could not fetch photo for {user.DisplayName}: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Could not fetch photo for {user.DisplayName}: {ex.Message}");
    }
}

Console.ReadLine();


//Get photo of a single user: https://graph.microsoft.com/v1.0/users/andre.geuze@xebia.com/photo/$value
//Get photo of all users: https://graph.microsoft.com/v1.0/users?$select=id,displayName,photo