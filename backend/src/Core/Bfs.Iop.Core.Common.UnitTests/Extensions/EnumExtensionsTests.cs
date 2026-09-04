using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Extensions;
using AwesomeAssertions;
using System.ComponentModel;
using Bfs.Iop.Infrastructure.Security;

namespace Bfs.Iop.Core.Common.UnitTests.Extensions;

[TestFixture(TestOf = typeof(EnumExtensions))]
internal sealed class EnumExtensionsTests
{
    [TestCase((RegistrationStatus)0)]
    [TestCase((PublicationLevel)(-23451))]
    [TestCase((BusinessRole)10000)]
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
