using System.Net;
using System.Net.Http.Json;
using System.Text;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Elasticsearch;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture]
internal sealed class TransientRetryHandlerTests
{
    [TestCase(HttpStatusCode.RequestTimeout)]
    [TestCase(HttpStatusCode.TooManyRequests)]
    [TestCase(HttpStatusCode.BadGateway)]
    [TestCase(HttpStatusCode.ServiceUnavailable)]
    [TestCase(HttpStatusCode.GatewayTimeout)]
    public void The_statuses_worth_retrying_are_the_ones_a_busy_cluster_answers(HttpStatusCode status) =>
        TransientRetryHandler.IsTransient(status).Should().BeTrue();

    [TestCase(HttpStatusCode.OK)]
    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.Unauthorized)]
    [TestCase(HttpStatusCode.NotFound)]
    [TestCase(HttpStatusCode.Conflict)]
    [TestCase(HttpStatusCode.InternalServerError)]
    public void A_rejected_request_is_not_retried(HttpStatusCode status) =>
        TransientRetryHandler.IsTransient(status).Should().BeFalse();

    [Test]
    public async Task A_post_with_a_body_is_resent_and_the_body_survives()
    {
        // The trap in any hand-rolled retry: a request message that cannot be sent twice, or content
        // that is consumed on the first attempt and arrives empty on the second.
        var inner = new SequenceHandler(HttpStatusCode.ServiceUnavailable, HttpStatusCode.OK);

        using var client = Client(inner);

        var response = await client.PostAsJsonAsync("/i14y-catalog/_search", new { size = 1 });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        inner.Attempts.Should().Be(2);
        inner.Bodies.Should().AllBe("{\"size\":1}");
    }

    [Test]
    public async Task A_transient_answer_is_retried_up_to_three_times_and_then_returned()
    {
        var inner = new SequenceHandler(
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.ServiceUnavailable);

        using var client = Client(inner);

        var response = await client.GetAsync("/i14y-catalog/_count");

        // Returned rather than thrown: the caller decides what a failed search or batch means.
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        inner.Attempts.Should().Be(3);
    }

    [Test]
    public async Task An_unreachable_node_is_retried_and_the_last_failure_surfaces()
    {
        var inner = new ThrowingHandler(3);

        using var client = Client(inner);

        var call = async () => await client.GetAsync("/i14y-catalog/_count");

        await call.Should().ThrowAsync<HttpRequestException>();

        inner.Attempts.Should().Be(3);
    }

    [Test]
    public async Task A_successful_answer_costs_one_attempt()
    {
        var inner = new SequenceHandler(HttpStatusCode.OK);

        using var client = Client(inner);

        await client.GetAsync("/i14y-catalog/_count");

        inner.Attempts.Should().Be(1);
    }

    private static HttpClient Client(HttpMessageHandler inner)
    {
        var retry = new TransientRetryHandler(
            NullLogger<TransientRetryHandler>.Instance,
            baseDelay: TimeSpan.Zero)
        {
            InnerHandler = inner,
        };

        return new HttpClient(retry) { BaseAddress = new Uri("http://elasticsearch.test") };
    }

    private sealed class SequenceHandler(params HttpStatusCode[] statuses) : HttpMessageHandler
    {
        private readonly List<string> _bodies = [];

        public int Attempts { get; private set; }

        public IReadOnlyList<string> Bodies => _bodies;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.Content is not null)
            {
                _bodies.Add(await request.Content.ReadAsStringAsync(cancellationToken));
            }

            var status = statuses[Math.Min(Attempts, statuses.Length - 1)];

            Attempts++;

            return new HttpResponseMessage(status)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json"),
            };
        }
    }

    private sealed class ThrowingHandler(int times) : HttpMessageHandler
    {
        public int Attempts { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Attempts++;

            return Attempts <= times
                ? throw new HttpRequestException("the node refused the connection")
                : Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
