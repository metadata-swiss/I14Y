using AwesomeAssertions;
using Bfs.Iop.Admin.Business.Mappings;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.DataAccess.Abstractions;
using MapsterMapper;
using NUnit.Framework;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

[TestFixture(TestOf = typeof(PersonMappingRegister))]
public class PersonMappingRegisterTests
{
    private IMapper _mapper = null!;

    [Test]
    public void Map_Person_FromDcatPerson_Ok()
    {
        // Arrange
        var source = TestData.Core.IopPersonModel;

        // Act
        var result = _mapper.Map<Models.Person>(source);

        // Assert
        result.FirstName.Should().Be(source.GivenName);
        result.LastName.Should().Be(source.FamilyName);
        result.Name.Should().Be($"{source.GivenName} {source.FamilyName}");
        result.Identifier.Should().Be(source.Email);
    }

    [Test]
    public void Map_DcatPerson_FromPerson_Ok()
    {
        // Arrange
        var source = TestData.Model.Person;

        // Act
        var result = _mapper.Map<IopPersonModel>(source);

        // Assert
        result.GivenName.Should().Be(source.FirstName);
        result.FamilyName.Should().Be(source.LastName);
        result.Email.Should().Be(source.Identifier);
    }

    [SetUp]
    public void Setup() => _mapper = TestHelper.CreateMapper();
}