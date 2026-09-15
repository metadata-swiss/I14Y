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

        // The timeout covers every attempt and the backoff between them. Two attempts cost at most one
        // ~300ms sleep, which a 5s interactive search can afford; three would leave a slow node timing
        // out mid-retry, turning a recoverable 503 into a TaskCanceledException that is not retried.
        Configure(services, ElasticsearchSearchExecutor.HttpClientName, TimeSpan.FromSeconds(5), attempts: 2);
        Configure(services, ElasticsearchBulkWriter.HttpClientName, TimeSpan.FromMinutes(2), attempts: 3);
        Configure(services, ElasticsearchIndexProvisioner.HttpClientName, TimeSpan.FromSeconds(30), attempts: 1);

        services.AddScoped<IndexWriteTarget>();

        services.AddScoped(CreateBulkWriter);
        services.AddScoped(CreateProvisioner);
        services.AddScoped(CreateSearchExecutor);

        return services
            .AddScoped<ICatalogIndexWriter, ElasticsearchCatalogIndexWriter>()
            .AddScoped<ICodeListIndexWriter, ElasticsearchCodeListIndexWriter>()
            .AddScoped<ICatalogSearchEngine, ElasticsearchCatalogSearchEngine>()
            .AddScoped<ICodeListSearchEngine, ElasticsearchCodeListSearchEngine>();
    }

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
