using Microsoft.Azure.Cosmos;

namespace FetchInfo;

public class SaveUsers(CosmosClient client)
{
    public async Task SaveUsersDataToCosmosAsync(List<User> users)
    {
        try
        {
            var dbResponse = await client.CreateDatabaseIfNotExistsAsync("GuessWho-ColleagueData");
            var containerResponse = await dbResponse.Database.CreateContainerIfNotExistsAsync("Users", "/id");
            var container = containerResponse.Container;
            
            foreach (var user in users.Select(u => new { id = u.Id, name = u.Name }))
            {
                await container.CreateItemAsync(user);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }
}