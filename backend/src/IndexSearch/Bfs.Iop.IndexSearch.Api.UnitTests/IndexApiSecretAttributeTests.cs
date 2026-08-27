using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Api.Filters;
using Bfs.Iop.IndexSearch.Api.Indexing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

/// <summary>
/// The shared secret is the only thing standing between the internet and the index write API.
/// <para>
/// The case that matters most is the unconfigured one. A missing secret is a deployment mistake, and
/// the tempting implementation — "no secret configured, so skip the check" — turns that mistake into
/// an open endpoint that behaves perfectly, so nothing reveals it. These tests pin the opposite:
/// unconfigured means everything is rejected.
/// </para>
/// </summary>
[TestFixture(TestOf = typeof(IndexApiSecretAttribute))]
public class IndexApiSecretAttributeTests
{
    private const string ConfiguredSecret = "the-real-secret";

    private static ActionExecutingContext CreateContext(string? configuredSecret, string? suppliedHeader)
    {
        var services = new ServiceCollection();

        services.AddSingleton<IOptions<IndexSearchOptions>>(
            new OptionsWrapper<IndexSearchOptions>(new IndexSearchOptions
            {
                Secret = configuredSecret ?? string.Empty,
            }));

        services.AddSingleton(NullLoggerFactory.Instance);
        services.AddLogging();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider(),
        };

        if (suppliedHeader is not null)
        {
            httpContext.Request.Headers[IndexApiSecretAttribute.HeaderName] = suppliedHeader;
        }

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        return new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), controller: null!);
    }

    /// <summary>Runs the filter and reports whether the action was allowed to execute.</summary>
    private static async Task<(bool NextCalled, IActionResult? Result)> InvokeAsync(
        string? configuredSecret,
        string? suppliedHeader)
    {
        var context = CreateContext(configuredSecret, suppliedHeader);
        var nextCalled = false;

        await new IndexApiSecretAttribute().OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(context, [], controller: null!));
        });

        return (nextCalled, context.Result);
    }

    private static void ShouldBeForbidden(bool nextCalled, IActionResult? result)
    {
        nextCalled.Should().BeFalse("the action must not run when the secret check fails");

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Test]
    public async Task An_unconfigured_secret_rejects_even_a_request_that_supplies_one()
    {
        // Fail closed. This is the whole point of the fixture: an unconfigured secret must never be
        // read as "no check required".
        var (nextCalled, result) = await InvokeAsync(configuredSecret: null, suppliedHeader: "anything");

        ShouldBeForbidden(nextCalled, result);
    }

    [TestCase("")]
    [TestCase("   ")]
    public async Task An_empty_or_whitespace_secret_counts_as_unconfigured(string configured)
    {
        var (nextCalled, result) = await InvokeAsync(configured, suppliedHeader: configured);

        // Note the header matches the configured value exactly here — a naive equality check would
        // let this through.
        ShouldBeForbidden(nextCalled, result);
    }

    [Test]
    public async Task A_missing_header_is_rejected()
    {
        var (nextCalled, result) = await InvokeAsync(ConfiguredSecret, suppliedHeader: null);

        ShouldBeForbidden(nextCalled, result);
    }

    [Test]
    public async Task An_empty_header_is_rejected()
    {
        var (nextCalled, result) = await InvokeAsync(ConfiguredSecret, suppliedHeader: string.Empty);

        ShouldBeForbidden(nextCalled, result);
    }

    [TestCase("wrong-secret")]
    [TestCase("the-real-secre")]
    [TestCase("the-real-secrets")]
    [TestCase("THE-REAL-SECRET")]
    public async Task A_wrong_header_is_rejected(string supplied)
    {
        // Includes a prefix, a longer value and a case variant: the comparison must be exact, and
        // must not be fooled by a value that merely starts the same.
        var (nextCalled, result) = await InvokeAsync(ConfiguredSecret, supplied);

        ShouldBeForbidden(nextCalled, result);
    }

    [Test]
    public async Task The_correct_header_lets_the_action_run()
    {
        var (nextCalled, result) = await InvokeAsync(ConfiguredSecret, ConfiguredSecret);

        nextCalled.Should().BeTrue();

        // No short-circuit result: the filter must leave the response to the action.
        result.Should().BeNull();
    }

    [Test]
    public async Task Rejection_answers_403_directly_rather_than_throwing()
    {
        // Regression guard. This filter used to throw ForbiddenException, which relied on exception
        // mapping being configured in the host — and this host had none, so the security check
        // answered 500 instead of 403. A 500 reads as "the service is broken", not "you are not
        // allowed", and would send whoever hit it looking in entirely the wrong place.
        var (_, result) = await InvokeAsync(ConfiguredSecret, "wrong");

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;

        objectResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        objectResult.Value.Should().BeOfType<ProblemDetails>()
            .Which.Status.Should().Be(StatusCodes.Status403Forbidden);
    }
}
