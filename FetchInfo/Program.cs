using Azure.Identity;
using Azure.Storage.Blobs;
using FetchInfo;
using Microsoft.Azure.Cosmos;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddTransient<ImageManager>();
builder.Services.AddTransient<SaveUsers>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<BlobServiceClient>(provider
        => new BlobServiceClient("UseDevelopmentStorage=true"));
    builder.Services.AddSingleton<CosmosClient>(provider
        => new CosmosClient("AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="));
}

else
{
    builder.Services.AddSingleton<BlobServiceClient>(provider
        =>
    {
        var endpoint = provider.GetRequiredService<IConfiguration>()["StorageAccount:BlobServiceEndpoint"];
        return new BlobServiceClient(
            new Uri(endpoint),
            new DefaultAzureCredential());
    });
    builder.Services.AddSingleton<CosmosClient>(provider =>
    {
        var endpoint = provider.GetRequiredService<IConfiguration>()["CosmosDb:Endpoint"];
        return new CosmosClient(endpoint, new DefaultAzureCredential());
    });
}

var host = builder.Build();

host.Run();