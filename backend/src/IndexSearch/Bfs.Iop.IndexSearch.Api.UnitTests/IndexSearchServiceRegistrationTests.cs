using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Api;
using Bfs.Iop.IndexSearch.Api.Controllers;
using Bfs.Iop.IndexSearch.Api.Indexing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

/// <summary>
/// Proves the host's container can actually construct everything it serves.
/// <para>
/// This fixture exists because of three real bugs, all of the same shape. When this host stopped
/// calling <c>AddIopCoreServices</c> it lost <c>ApiSettings</c>, <c>IDatasetsService</c> and the
/// dependencies of the code-list search service — registrations nothing named, because that one call
/// had supplied them as a side effect. Every one of them compiled, passed 600+ unit tests, and
/// surfaced only as a startup <c>AggregateException</c> on someone's machine.
/// </para>
/// <para>
/// <b>What makes this catch them:</b> <c>validateOnBuild</c> walks every registered descriptor and
/// reports every constructor it cannot satisfy — the same check
/// <c>WebApplication.CreateBuilder</c> turns on in Development, which is what found all three. The
/// controller resolutions below add the half <c>validateOnBuild</c> cannot see: controllers are
/// created by MVC's activator, not the container, so a missing controller dependency is not a
/// registration error and passes validation silently.
/// </para>
/// <para>
/// <b>Deliberately not a <c>WebApplicationFactory</c> test.</b> Booting the real host starts
/// <c>IndexBuilderHostedService</c>, which opens a database connection and talks to Elasticsearch —
/// neither is available in CI, so the test would fail for reasons that have nothing to do with the
/// defect it guards.
/// </para>
/// </summary>
[TestFixture(TestOf = typeof(IndexSearchServiceRegistration))]
public class IndexSearchServiceRegistrationTests
{
    /// <summary>
    /// Configuration sufficient to register, not to connect. The database is never opened here:
    /// <c>AddDbContext</c> only needs a well-formed connection string, and nothing in this fixture
    /// executes a query.
    /// </summary>
    private static IConfiguration Configuration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["postgresCredentialsSectionKey"] = "postgres:client",
                ["postgres:client:hostname"] = "127.0.0.1",
                ["postgres:client:port"] = "15432",
                ["postgres:client:database"] = "test",
                ["postgres:client:username"] = "test",
                ["postgres:client:password"] = "test",
                ["ObjectStoreConfigurationLocation:Endpoint"] = "ObjectStoreConfiguration:Endpoint",
                ["ObjectStoreConfigurationLocation:AccessKey"] = "ObjectStoreConfiguration:AccessKey",
                ["ObjectStoreConfigurationLocation:Secret"] = "ObjectStoreConfiguration:Secret",
                ["ObjectStoreConfiguration:Service"] = "test",
                ["ObjectStoreConfiguration:Endpoint"] = "localhost:9021",
                ["ObjectStoreConfiguration:AccessKey"] = "test",
                ["ObjectStoreConfiguration:Secret"] = "test",
                ["ObjectStoreConfiguration:UseSsl"] = "false",
                ["Elasticsearch:Uri"] = "http://localhost:9200",
                ["Elasticsearch:CatalogIndexName"] = "catalog",
                ["Elasticsearch:CodeListIndexName"] = "codelist",
                ["IndexSearch:Secret"] = "test-secret",
            })
            .Build();

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        // Stand-in for ASP.NET plumbing this fixture deliberately does not build. AddAuthorization
        // (reached through TryAddSecurity) registers AuthorizationPolicyCache, which takes an
        // EndpointDataSource that MapControllers supplies in the real host. Without it the validation
        // below reports a gap that does not exist in the running service — and a smoke test that cries
        // wolf is one nobody reads. Empty, because nothing here asserts on routes.
        services.AddSingleton<EndpointDataSource>(new DefaultEndpointDataSource());
        services.AddIndexSearchServices(Configuration(), "Development");

        // Both flags on, matching what WebApplication.CreateBuilder does in Development — which is
        // the environment where these bugs were found and the only place the host validates today.
        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });
    }

    [Test]
    public void Every_registration_can_be_satisfied()
    {
        var act = () => BuildProvider().Dispose();

        act.Should().NotThrow(
            because: "a service the container cannot construct is a startup crash, and every one of "
                   + "the three that got here compiled and passed the whole test suite first");
    }

    /// <summary>
    /// The half <c>ValidateOnBuild</c> misses. MVC activates controllers itself, so their
    /// constructor dependencies are never validated — a missing one is a 500 on first request rather
    /// than a failure to start.
    /// </summary>
    [TestCase(typeof(SearchController))]
    [TestCase(typeof(ConceptsController))]
    [TestCase(typeof(IndexController))]
    public void Every_controller_can_be_activated(Type controllerType)
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var act = () => ActivatorUtilities.CreateInstance(scope.ServiceProvider, controllerType);

        act.Should().NotThrow(
            because: $"{controllerType.Name} is activated by MVC rather than by the container, so "
                   + "ValidateOnBuild cannot see its dependencies");
    }

    /// <summary>
    /// The rebuild path, which no controller reaches: the hosted service resolves these, and it is
    /// excluded from the container above precisely because starting it would do real I/O.
    /// </summary>
    [Test]
    public void The_rebuild_path_can_be_activated()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var act = () =>
        {
            scope.ServiceProvider.GetServices<Bfs.Iop.Search.Abstractions.IIndexBuilderService>()
                 .Should().NotBeEmpty(because: "with no builder registered the index is never built");
            _ = ActivatorUtilities.CreateInstance<IndexBuilderHostedService>(scope.ServiceProvider);
            _ = ActivatorUtilities.CreateInstance<IndexEventProcessor>(scope.ServiceProvider);
        };

        act.Should().NotThrow();
    }
}
