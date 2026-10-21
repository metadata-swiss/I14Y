using System.Net.Http.Headers;
using System.Text;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Bfs.Iop.IndexSearch.Elasticsearch.CodeLists;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Elasticsearch.Extensions;

/// <summary>
///     How long one Elasticsearch client may take and how many times it may try. The timeout covers
///     every attempt and the backoff between them, so the two have to fit together: too many
///     attempts under a short timeout and a slow node is cut off mid-retry, turning a recoverable
///     503 into a TaskCanceledException nobody retries.
/// </summary>
internal sealed record HttpClientLimits(string Name, TimeSpan Timeout, int Attempts);

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIndexSearchElasticsearch(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        services
            .AddOptions<ElasticsearchOptions>()
            .Bind(configuration.GetSection(ElasticsearchOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<ElasticsearchOptions>, ElasticsearchOptionsValidation>();

        services.AddSingleton<IndexNames>();

        foreach (var limits in ClientLimits)
        {
            Configure(services, limits.Name, limits.Timeout, limits.Attempts);
        }

        services.AddScoped<IndexWriteTarget>();

        services.AddScoped(CreateBulkWriter);
        services.AddScoped(CreateProvisioner);
        services.AddScoped(CreateSearchExecutor);

        return services
            .AddScoped<ICatalogIndexReader>(CreateCatalogIndexReader)
            .AddScoped<ICatalogIndexWriter, ElasticsearchCatalogIndexWriter>()
            .AddScoped<ICodeListIndexWriter, ElasticsearchCodeListIndexWriter>()
            .AddScoped<ICodeListIndexReader, ElasticsearchCodeListIndexReader>()
            .AddScoped<ICatalogSearchEngine, ElasticsearchCatalogSearchEngine>()
            .AddScoped<ICodeListSearchEngine, ElasticsearchCodeListSearchEngine>();
    }

    internal static readonly IReadOnlyList<HttpClientLimits> ClientLimits =
    [
        new(ElasticsearchSearchExecutor.HttpClientName, TimeSpan.FromSeconds(5), 2),
        new(ElasticsearchCatalogIndexReader.HttpClientName, TimeSpan.FromSeconds(30), 4),
        new(ElasticsearchBulkWriter.HttpClientName, TimeSpan.FromMinutes(2), 3),
        new(ElasticsearchIndexProvisioner.HttpClientName, TimeSpan.FromSeconds(30), 1),
    ];

    private static void Configure(IServiceCollection services, string name, TimeSpan timeout, int attempts)
    {
        var builder = services.AddHttpClient(name, (provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<ElasticsearchOptions>>().Value;

            client.BaseAddress = new Uri(options.Uri);
            client.Timeout = timeout;

            if (!string.IsNullOrWhiteSpace(options.Username))
            {
                var credentials = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{options.Username}:{options.Password}"));

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", credentials);
            }
        });

        if (attempts > 1)
        {
            builder.AddHttpMessageHandler(provider => new TransientRetryHandler(
                provider.GetRequiredService<ILogger<TransientRetryHandler>>(),
                maxAttempts: attempts));
        }
    }

    private static ElasticsearchBulkWriter CreateBulkWriter(IServiceProvider provider) =>
        new(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient(ElasticsearchBulkWriter.HttpClientName),
            provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ElasticsearchBulkWriter>>());

    private static ElasticsearchCatalogIndexReader CreateCatalogIndexReader(IServiceProvider provider) =>
        new(
            provider.GetRequiredService<IHttpClientFactory>()
                .CreateClient(ElasticsearchCatalogIndexReader.HttpClientName),
            provider.GetRequiredService<IndexNames>());

    private static ElasticsearchSearchExecutor CreateSearchExecutor(IServiceProvider provider) =>
        new(provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient(ElasticsearchSearchExecutor.HttpClientName));

    private static ElasticsearchIndexProvisioner CreateProvisioner(IServiceProvider provider) =>
        new(
            provider.GetRequiredService<IHttpClientFactory>()
                .CreateClient(ElasticsearchIndexProvisioner.HttpClientName),
            provider.GetRequiredService<IndexNames>(),
            provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ElasticsearchIndexProvisioner>>(),
            provider.GetService<TimeProvider>());
}
