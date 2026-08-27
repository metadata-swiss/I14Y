using Bfs.Iop.Core.Data.Contracts;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Bfs.Iop.Core.LinkedData.UnitTests;

/// <summary>
/// Supplies the service provider the linked-data services now resolve <c>IDatasetsService</c> from.
/// <para>
/// It is resolved lazily rather than injected so that a read-only host — the search service — can
/// construct these services without registering the business layer. These tests exercise the
/// object-store paths, which never touch it, but the provider must still be able to hand one over if
/// a test ever reaches an authorization path.
/// </para>
/// </summary>
internal static class TestServiceProvider
{
    public static IServiceProvider DatasetsServiceProvider() =>
        new ServiceCollection()
            .AddScoped(_ => Substitute.For<IDatasetsService>())
            .BuildServiceProvider();
}
