using System.Reflection;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Api.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

/// <summary>
///     Pins which endpoints of the index controller answer without a token.
/// </summary>
/// <remarks>
///     The four write endpoints are anonymous on purpose: Core notifies them from a background thread
///     that has no request and therefore no bearer token to forward, so requiring one would reject
///     every notification and the index would only ever be as fresh as the last nightly rebuild. What
///     protects them is that the service has no public ingress.
///     <para>
///         That is a deliberate trade, so it is worth failing a build over if it changes by accident
///         in either direction - someone adding a token requirement would silently stop every
///         notification, and someone opening up the rebuild would hand an anonymous caller a way to
///         start a full pass at will.
///     </para>
/// </remarks>
[TestFixture]
internal sealed class IndexWriteEndpointExposureTests
{
    [TestCase(nameof(IndexController.UpsertCatalogResource))]
    [TestCase(nameof(IndexController.RemoveCatalogResource))]
    [TestCase(nameof(IndexController.ReplaceCodeList))]
    [TestCase(nameof(IndexController.RemoveCodeList))]
    public void A_write_endpoint_answers_without_a_token(string action)
    {
        Method(action).GetCustomAttribute<AllowAnonymousAttribute>()
            .Should().NotBeNull("Core notifies this endpoint with no token to send");
    }

    [TestCase(nameof(IndexController.Reindex))]
    [TestCase(nameof(IndexController.Status))]
    public void Everything_else_still_needs_the_interoperability_service_role(string action)
    {
        Method(action).GetCustomAttribute<AllowAnonymousAttribute>()
            .Should().BeNull("only the notification endpoints were opened up");
    }

    [Test]
    public void The_controller_still_carries_the_rebuild_policy()
    {
        // The write endpoints opt out of it individually. If this went, they would all become
        // anonymous by default rather than by decision.
        typeof(IndexController).GetCustomAttribute<AuthorizeAttribute>()
            .Should().NotBeNull();
    }

    private static MethodInfo Method(string name) =>
        typeof(IndexController).GetMethod(name)
        ?? throw new InvalidOperationException($"'{name}' is not an action on the index controller.");
}
