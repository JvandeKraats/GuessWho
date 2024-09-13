using Azure.Identity;
using Azure.Storage.Blobs;
using FetchInfo;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddTransient<ImageManager>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<BlobServiceClient>((provider)
        => new BlobServiceClient("UseDevelopmentStorage=true"));
}
else
{
    builder.Services.AddSingleton<BlobServiceClient>((provider)
        =>
    {
        var endpoint = provider.GetRequiredService<IConfiguration>()["StorageAccount:BlobServiceEndpoint"];
        return new BlobServiceClient(
            new Uri(endpoint),
            new DefaultAzureCredential());
    });
}

var host = builder.Build();

var tesrt = builder.Environment;
host.Run();