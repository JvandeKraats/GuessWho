using FetchInfo;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.dev.json", optional: true)
    .Build();

var scopes = new[] { "https://graph.microsoft.com/.default" };
var tenantId = configuration["AzureStuff:TenantId"];
var clientId = configuration["AzureStuff:ClientId"];
var clientSecret = configuration["AzureStuff:ClientSecret"];
var authority = $"https://login.microsoftonline.com/{tenantId}";
var clientObjectId = configuration["AzureStuff:ClientObjectId"];

// access token copied from graph explorer
var accessToken = configuration["AzureStuff:TemporaryAccessToken"];

var customAuthProvider = new CustomAuthProvider(accessToken);
var client = new GraphServiceClient(customAuthProvider);
var users = await client.Users.GetAsync((requestConfig) =>
{
    requestConfig.Headers.Add("ConsistencyLevel", "eventual");
    requestConfig.QueryParameters.Filter = "mail ne null";
    requestConfig.QueryParameters.Count = true;
});

users.Value.ForEach(u => Console.WriteLine(u.DisplayName));

Console.ReadLine();


//Get photo of a single user: https://graph.microsoft.com/v1.0/users/andre.geuze@xebia.com/photo/$value
//Get photo of all users: https://graph.microsoft.com/v1.0/users?$select=id,displayName,photo