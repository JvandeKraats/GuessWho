using System.Text.Json.Serialization;

namespace FetchInfo;

public class User
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}