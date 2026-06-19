using Bfs.Iop.Core.Common.Extensions;
using AwesomeAssertions;

namespace Bfs.Iop.Core.Common.UnitTests.Extensions;

[TestFixture(TestOf = typeof(CollectionExtensionsTests))]
internal sealed class CollectionExtensionsTests
{
    [Test]
    public void Given_enumerable_and_action_When_ForEach_Then_expected()
    {
        // Arrange
        ICollection<Toto> subject = [new Toto(), new Toto()];

        // Act
        subject.ForEach(x => x.Int = 3);

        // Assert
        subject.All(x => x.Int == 3).Should().BeTrue();
    }

    public record Toto
    {
        public int Int { get; set; }
    }
}
