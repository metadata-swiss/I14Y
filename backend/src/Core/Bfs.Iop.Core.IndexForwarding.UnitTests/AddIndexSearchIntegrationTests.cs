using AwesomeAssertions;
using Bfs.Iop.Core.Services.Contracts;
using Bfs.Iop.Core.IndexForwarding.Extensions;
using Bfs.Iop.IndexSearch.ApiClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bfs.Iop.Core.IndexForwarding.UnitTests;

/// <summary>
/// Guards the highest-consequence failure mode introduced by removing Core's in-process index.
/// <para>
/// This registration used to be optional: with no IndexSearch base URL it quietly skipped itself and
/// an in-process index carried on serving reads and writes. Core has none any more, so skipping would
/// leave <c>ICatalogIndexWriter</c> unregistered — and every Core business service takes it in its
/// constructor. The host would start cleanly and then fail to resolve <c>IDatasetsService</c>,
/// <c>IIopConceptsService</c>, <c>IMappingTablesService</c>, <c>IPublicServicesService</c> and
/// <c>IDataServicesService</c>, returning 500 from nearly every endpoint — traced back to one absent
/// setting. Failing at startup instead is the entire point of these tests.
/// </para>
/// </summary>
[TestFixture]
public sealed class AddIndexSearchIntegrationTests
{
    private const string ValidBaseUrl = "http://localhost:8003";

    private static IServiceCollection Add(string? baseUrl) =>
        new ServiceCollection().AddIndexSearchIntegration(baseUrl, secret: "test-secret");

    [Test]
    public void WithBaseUrl_RegistersBothWritePorts()
    {
        var services = Add(ValidBaseUrl);

        services.Should().Contain(x => x.ServiceType == typeof(ICatalogIndexWriter));
        services.Should().Contain(x => x.ServiceType == typeof(ICodeListEntryIndexWriter));
    }

    /// <summary>
    /// The read client matters as much as the write ports: all four of Core's search handlers inject
    /// it, so an unregistered client leaves catalog search, facet counts, code-list search and
    /// code-list export unresolvable together.
    /// <para>
    /// There is no read port any more. Core's handlers take <see cref="IIndexSearchSearchClient"/>
    /// directly, which is also what guarantees a facet count and the result list it labels come from
    /// the same place — there is only one place.
    /// </para>
    /// </summary>
    [Test]
    public void WithBaseUrl_RegistersTheReadClient()
    {
        Add(ValidBaseUrl).Should().Contain(x => x.ServiceType == typeof(IIndexSearchSearchClient));
    }

    [Test]
    public void WithoutBaseUrl_ThrowsAtStartup()
    {
        var act = () => Add(null);

        act.Should().Throw<InvalidOperationException>().WithMessage("*not configured*");
    }

    /// <summary>
    /// A deployment whose token was never substituted must fail the same way as an absent one, rather
    /// than pointing every search at the literal host "#{INDEXSEARCH_BASE_URL}#".
    /// </summary>
    [Test]
    public void WithUnsubstitutedToken_ThrowsAtStartup()
    {
        var act = () => Add("#{INDEXSEARCH_BASE_URL}#");

        act.Should().Throw<InvalidOperationException>();
    }

    /// <summary>
    /// Scoped, not singleton: the write ports are resolved per request alongside the business services
    /// that take them in their constructors.
    /// <para>
    /// The read client is deliberately not asserted here. <c>AddHttpClient</c> registers a typed
    /// client as transient by design — its handler chain is pooled and reused across requests — and
    /// that is why the caller's token is fetched per request inside
    /// <c>IndexSearchBearerTokenHandler</c> rather than captured at construction. Asserting Scoped
    /// here would pin the wrong lifetime and read as though a scoped client were the safe choice.
    /// </para>
    /// </summary>
    [Test]
    public void WritePortsAreRegisteredAsScoped()
    {
        var services = Add(ValidBaseUrl);

        services.Single(x => x.ServiceType == typeof(ICatalogIndexWriter))
            .Lifetime.Should().Be(ServiceLifetime.Scoped);
        services.Single(x => x.ServiceType == typeof(ICodeListEntryIndexWriter))
            .Lifetime.Should().Be(ServiceLifetime.Scoped);
    }
}
