using Amazon.S3;
using Azure.Identity;
using Azure.Storage.Blobs;
using Bfs.Iop.Core.FileStorage.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bfs.Iop.Core.FileStorage;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration, string environmentName)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName, nameof(environmentName));

        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CONTAINER_APP_NAME"))
            && string.IsNullOrWhiteSpace(configuration["Azure:StorageAccount:AccountName"])
            && string.IsNullOrWhiteSpace(configuration["Azure:StorageAccount:ConnectionString"])) //decision to use either AWS or Azure
        {
            services.AddS3Storage(configuration, environmentName);
        }
        else
        {
            services.AddAzureBlobStorage(configuration);
        }

        return services;
    }

    private static IServiceCollection AddS3Storage(this IServiceCollection services, IConfiguration configuration, string environmentName)
    {
        // two layer configuration
        // 1st layer get the location strings
        var endpointConfigurationLocation = configuration["ObjectStoreConfigurationLocation:Endpoint"] ?? throw new Exception($"ObjectStoreConfigurationLocation:Endpoint configuration value is null.");
        var accessKeyConfigurationLocation = configuration["ObjectStoreConfigurationLocation:AccessKey"] ?? throw new Exception($"ObjectStoreConfigurationLocation:AccessKey configuration value is null.");
        var secretKeyConfigurationLocation = configuration["ObjectStoreConfigurationLocation:Secret"] ?? throw new Exception($"ObjectStoreConfigurationLocation:Secret configuration value is null.");

        // 2nd layer retrieves the values from configuration based on the location string
        var endpoint = configuration[endpointConfigurationLocation] ??
            throw new InvalidOperationException($"Could not load '{endpointConfigurationLocation}' from environment '{environmentName}'.");

        var accessKey = configuration[accessKeyConfigurationLocation] ??
            throw new InvalidOperationException($"Could not load '{accessKeyConfigurationLocation}' from environment '{environmentName}'.");

        var secretKey = configuration[secretKeyConfigurationLocation] ??
            throw new InvalidOperationException($"Could not load '{secretKeyConfigurationLocation}' from environment '{environmentName}'.");

        var useSsl = configuration.GetValue("ObjectStoreConfiguration:UseSsl", false);

        var schema = useSsl ? "https://" : "http://";

        var endpointWithSchema = $"{schema}{endpoint}";

        var awsConfig = new AmazonS3Config
        {
            ServiceURL = endpointWithSchema,
            ForcePathStyle = true
        };

        services.AddSingleton<IAmazonS3>(new AmazonS3Client(accessKey, secretKey, awsConfig));
        services.AddSingleton<IFileStorageService, S3FileStorageService>();

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
