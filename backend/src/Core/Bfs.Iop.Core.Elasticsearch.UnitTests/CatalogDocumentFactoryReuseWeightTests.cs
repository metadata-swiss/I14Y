using Bfs.Iop.Core.Elasticsearch;

namespace Bfs.Iop.Core.Elasticsearch.UnitTests;

/// <summary>
/// Tests for <see cref="CatalogDocumentFactory.ReuseCountToWeight"/> — the "most reused concept"
/// scoring boost. The exact scale/cap are a placeholder pending product input (the story marks the
/// relative weight vs. other ranking criteria as TBD); these tests pin down the shape of the
/// formula (neutral at zero, monotonically increasing, capped) rather than exact numbers.
/// </summary>
[TestFixture(TestOf = typeof(CatalogDocumentFactory))]
public class CatalogDocumentFactoryReuseWeightTests
{
    [TestCase(0)]
    [TestCase(-5)]
    public void ReuseCountToWeight_NoReuse_ReturnsNeutralWeight(int reuseCount) =>
        Assert.That(CatalogDocumentFactory.ReuseCountToWeight(reuseCount), Is.EqualTo(100));

    [Test]
    public void ReuseCountToWeight_IsMonotonicallyIncreasingWithReuseCount()
    {
        var weights = new[] { 0, 1, 5, 20, 100, 1000 }
            .Select(CatalogDocumentFactory.ReuseCountToWeight)
            .ToArray();

        for (var i = 1; i < weights.Length; i++)
        {
            Assert.That(weights[i], Is.GreaterThanOrEqualTo(weights[i - 1]),
                $"Weight for a higher reuse count must not be lower (index {i}).");
        }
    }

    [Test]
    public void ReuseCountToWeight_NeverExceedsSameOrderOfMagnitudeAsRegistrationStatusWeight()
    {
        // RegistrationStatusWeight ranges 85-110 (span 25); the reuse boost is capped to a comparable
        // span so the two multipliers compose without either one drowning out the other.
        var weight = CatalogDocumentFactory.ReuseCountToWeight(1_000_000);

        Assert.That(weight, Is.LessThanOrEqualTo(150));
    }

    [Test]
    public void ReuseCountToWeight_SmallReuseCount_IsAboveNeutral()
    {
        Assert.That(CatalogDocumentFactory.ReuseCountToWeight(1), Is.GreaterThan(100));
    }
}
