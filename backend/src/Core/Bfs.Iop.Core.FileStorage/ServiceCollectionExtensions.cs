using Azure.Identity;
using Azure.Storage.Blobs;
using Bfs.Iop.Core.FileStorage.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bfs.Iop.Core.FileStorage;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        if (services.Any(x => x.ServiceType == typeof(IFileStorageService)))
        {
            return services;
        }

        services.AddAzureBlobStorage(configuration);

        return services;
    }

    private static IServiceCollection AddAzureBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var acctName = configuration["Azure:StorageAccount:AccountName"];
        var connectionString = configuration["Azure:StorageAccount:ConnectionString"];

        BlobServiceClient blobServiceClient;

        if (!string.IsNullOrWhiteSpace(acctName))
        {
            // Managed Identity / DefaultAzureCredential on Azure
            var serviceUri = new Uri($"https://{acctName}.blob.core.windows.net");
            blobServiceClient = new BlobServiceClient(serviceUri, new DefaultAzureCredential());
        }
        else if (!string.IsNullOrWhiteSpace(connectionString))
        {
            //explicit account key (from Key Vault or config)
            bool isAzurite = connectionString.Contains("UseDevelopmentStorage=true", StringComparison.OrdinalIgnoreCase)
              || connectionString.Contains("127.0.0.1:10000", StringComparison.OrdinalIgnoreCase)
              || connectionString.Contains("devstoreaccount1", StringComparison.OrdinalIgnoreCase);

            blobServiceClient = isAzurite
                ? new BlobServiceClient(connectionString, new BlobClientOptions(BlobClientOptions.ServiceVersion.V2021_12_02))
                : new BlobServiceClient(connectionString);
        }
        else
        {
            throw new InvalidOperationException("Blob Storage not configured. Set Azure:StorageAccount:AccountName and AccessKey for key auth or AccountName for managed identity auth.");
        }

        services.AddSingleton(blobServiceClient);
        services.AddSingleton<IFileStorageService, AzureFileStorageService>();

        return services;
    }
}
