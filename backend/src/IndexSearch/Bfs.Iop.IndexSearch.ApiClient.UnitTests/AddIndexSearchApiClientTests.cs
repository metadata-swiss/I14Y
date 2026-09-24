using AwesomeAssertions;
using Bfs.Iop.IndexSearch.ApiClient.Extensions;
using Microsoft.Extensions.DependencyInjection;

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
    public void A_client_resolves_without_a_token_retriever()
    {
        // Nothing registers one by default, and the search endpoints do not need one. Requiring it
        // would make the common case the one that fails at startup.
        using var provider = new ServiceCollection()
            .AddIndexSearchApiClient(BaseAddress)
            .BuildServiceProvider();

        var resolve = provider.GetRequiredService<IIndexSearchApiClient>;

        resolve.Should().NotThrow();
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
}
