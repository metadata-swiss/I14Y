using AwesomeAssertions;
using Bfs.Iop.Admin.Business.Mappings;
using Bfs.Iop.DataAccess.Abstractions;
using MapsterMapper;
using NUnit.Framework;
using System.Linq;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

[TestFixture(TestOf = typeof(PublicServiceInputMappingRegister))]
public class PublicServiceInputMappingRegisterTests
{
    private IMapper _mapper = null!;

    [Test]
    public void Map_PublicServiceModel_To_PublicServiceInput_Ok()
    {
        // Arrange
        var source = TestData.Core.PublicServiceModel;

        // Act
        var result = _mapper.Map<Models.PublicServiceInput>(source);

        // Assert
        result.BusinessEventsCodes.Should().BeEquivalentTo(source.BusinessEvents.Select(x => x.Code));
        result.CompetentAuthority.Identifier.Should().Be(source.Publisher.Identifier);
        result.Description.De.Should().Be(source.Description.De);
        result.Identifiers.Any().Should().Be(true);
        result.Identifiers.First().Should().Be(source.Identifiers.First());
        result.Keywords.First().Label!.De.Should().Be(source.Keywords.First().Label!.De);
        result.LanguageCodes.Should().BeEquivalentTo(source.Languages.Select(t => t.Code));
        result.LifeEventsCodes.Should().BeEquivalentTo(source.LifeEvents.Select(x => x.Code));
        result.Sectors.Select(x => new {x.Code, x.Uri, x.VocabularyIdentifier}).Should().BeEquivalentTo(source.Sectors.Select(x => new {x.Code, x.Uri, x.VocabularyIdentifier}));
        result.Spatial.Should().BeEquivalentTo(source.Spatial);
        result.ThematicAreas.Select(x => new {x.Code, x.Uri, x.VocabularyIdentifier}).Should().BeEquivalentTo(source.ThematicAreas.Select(x => new {x.Code, x.Uri, x.VocabularyIdentifier}));
        result.Title.De.Should().Be(source.Name.De);
    }

    [Test]
    public void Map_PublicServiceInput_To_PublicServiceInputModel_Ok()
    {
        // Arrange
        var source = TestData.Model.PublicServiceInput;

        // Act
        var result = _mapper.Map<PublicServiceInputModel>(source);

        // Assert
        result.BusinessEvents.Select(x => x.Code).Should().BeEquivalentTo(source.BusinessEventsCodes);   
        result.Publisher.Identifier.Should().Be(source.CompetentAuthority.Identifier);
        result.Description.De.Should().Be(source.Description.De);
        result.Keywords.First().Label!.De.Should().Be(source.Keywords.First().Label!.De);
        result.Languages.Select(t => t.Code).Should().BeEquivalentTo(source.LanguageCodes);
        result.LifeEvents.Select(x => x.Code).Should().BeEquivalentTo(source.LifeEventsCodes);
        result.Sectors.Should().BeEquivalentTo(source.Sectors);
        result.Spatial.Should().BeEquivalentTo(source.Spatial);
        result.ThematicAreas.Should().BeEquivalentTo(source.ThematicAreas);
        result.Name.De.Should().Be(source.Title.De);
    }

    [SetUp]
    public void Setup() => _mapper = TestHelper.CreateMapper();
}