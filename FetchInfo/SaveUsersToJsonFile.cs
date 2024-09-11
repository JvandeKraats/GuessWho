using System.Text.Json;

namespace FetchInfo;

public class SaveUsersToJsonFile : ISaveUsers
{
    public Task Save(List<User> users)
    {
        var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "users.json");
        File.WriteAllText(filePath, json);
        return Task.CompletedTask;
    }
}