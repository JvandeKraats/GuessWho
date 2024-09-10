using FetchInfo.Exceptions;
using Microsoft.Graph;

namespace FetchInfo;

public static class Fetcher
{
    public static async Task<List<Microsoft.Graph.Models.User>> FetchUsersAsync(string accesstoken)
    {
        var client = GraphServiceClientFactory.Construct(accesstoken);
        var users = await client.Users.GetAsync((requestConfig) =>
        {
            requestConfig.Headers.Add("ConsistencyLevel", "eventual");
            requestConfig.QueryParameters.Filter = "mail ne null";
            requestConfig.QueryParameters.Count = true;
            requestConfig.QueryParameters.Top = 100;
        });
        
        if(users?.Value is null)
        {
            throw new NoUsersFoundException("No users found");
        }

        return users.Value.ToList();
    }
}