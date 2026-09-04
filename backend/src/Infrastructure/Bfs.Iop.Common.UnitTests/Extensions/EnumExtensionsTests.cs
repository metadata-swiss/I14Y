using AwesomeAssertions;
using Bfs.Iop.Common.Extensions;
using System.ComponentModel;

namespace Bfs.Iop.Common.UnitTests.Extensions;

[TestFixture(TestOf = typeof(EnumExtensions))]
internal sealed class EnumExtensionsTests
{
    private enum TestEnum
    {
        One = 1,
        Two = 2
    }

    [TestCase((TestEnum)0)]
    [TestCase((TestEnum)(-23451))]
    [TestCase((TestEnum)10000)]
    public void Given_invalid_enum_value_When_EnsureValidIsValid_Then_throw_InvalidEnumArgumentException<T>(T subject) where T : struct, Enum
    {
        // Act
        var action = () => subject.EnsureValueIsValid();

        // Assert
        action.Should()
            .ThrowExactly<InvalidEnumArgumentException>()
            .WithParameterName(nameof(subject));
    }
}