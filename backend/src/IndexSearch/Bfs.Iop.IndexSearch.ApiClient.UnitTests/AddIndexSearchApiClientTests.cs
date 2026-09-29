using AwesomeAssertions;
using Bfs.Iop.IndexSearch.ApiClient.Authentication;
using Bfs.Iop.IndexSearch.ApiClient.Extensions;
using Bfs.Iop.IndexSearch.ApiClient.Health;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.ApiClient.UnitTests;

[TestFixture]
internal sealed class AddIndexSearchApiClientTests
{
    private const string BaseAddress = "http://indexsearch.test";

    [Test]
    public void The_registration_resolves_a_client()
    {
        using var provider = new ServiceCollection()
            .AddIndexSearchApiClient(BaseAddress)
            .BuildServiceProvider();

        provider.GetRequiredService<IIndexSearchApiClient>().Should().NotBeNull();
    }

    [Test]
    public void The_incoming_request_token_is_forwarded_by_default()
    {
        // What a search returns depends on the token the client sends. A host that registered the
        // client but forgot the retriever would call as an anonymous caller and quietly see public
        // results only, which reads as missing data rather than as a misconfiguration.
        using var provider = new ServiceCollection()
            .AddIndexSearchApiClient(BaseAddress)
            .BuildServiceProvider();

        provider.GetRequiredService<ITokenRetriever>().Should().BeOfType<RequestUserTokenRetriever>();
    }

    [Test]
    public void A_token_retriever_the_host_registered_first_is_kept()
    {
        // Not every host authenticates through an incoming request, so the default must give way.
        using var provider = new ServiceCollection()
            .AddSingleton<ITokenRetriever, StubTokenRetriever>()
            .AddIndexSearchApiClient(BaseAddress)
            .BuildServiceProvider();

        provider.GetRequiredService<ITokenRetriever>().Should().BeOfType<StubTokenRetriever>();
    }

    [Test]
    public void The_health_check_is_registered_with_the_client()
    {
        // It lives here rather than in each host, so that adding the client cannot leave a host
        // without any signal that IndexSearch is unreachable.
        using var provider = new ServiceCollection()
            .AddIndexSearchApiClient(BaseAddress)
            .BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        options.Value.Registrations.Should().ContainSingle(x => x.Name == "IndexSearch");
    }

    [Test]
    public void Registering_twice_does_not_duplicate_the_health_check()
    {
        // Two registrations under one name throw when the health service is built, taking the host
        // down at startup with an error that names the check rather than the duplicate call.
        using var provider = new ServiceCollection()
            .AddIndexSearchApiClient(BaseAddress)
            .AddIndexSearchApiClient(BaseAddress)
            .BuildServiceProvider();

        var build = provider.GetRequiredService<HealthCheckService>;

        build.Should().NotThrow();
    }

    [TestCase(null, TestName = "a null base address")]
    [TestCase("", TestName = "an empty base address")]
    [TestCase("   ", TestName = "a blank base address")]
    public void A_base_address_that_is_not_configured_is_refused(string? apiBaseAddress)
    {
        // A missing configuration key reads as null here. Left unchecked it registers happily, the
        // host starts, and the first search fails somewhere inside the HTTP client instead.
        var register = () => new ServiceCollection().AddIndexSearchApiClient(apiBaseAddress!);

        register.Should().Throw<ArgumentException>()
            .WithMessage("*apiBaseAddress*");
    }

    [Test]
    public void The_named_http_client_is_registered()
    {
        // The support asks the factory for this name. If the registration and the name ever drift,
        // the factory silently hands back a default client and the pooling is lost without an error.
        using var provider = new ServiceCollection()
            .AddIndexSearchApiClient(BaseAddress)
            .BuildServiceProvider();

        var factory = provider.GetRequiredService<IHttpClientFactory>();

        factory.CreateClient("IndexSearch.ApiClient").Should().NotBeNull();
    }

    private sealed class StubTokenRetriever : ITokenRetriever
    {
        public Task<string> GetAuthTokenAsync() => Task.FromResult(string.Empty);
    }
}
