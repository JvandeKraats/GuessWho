using Microsoft.Graph;

namespace FetchInfo;

public static class GraphServiceClientFactory
{
    public static GraphServiceClient Construct(string accessToken)
    {
        var customAuthProvider = new CustomAuthProvider(accessToken);
        return new GraphServiceClient(customAuthProvider);
    }
}