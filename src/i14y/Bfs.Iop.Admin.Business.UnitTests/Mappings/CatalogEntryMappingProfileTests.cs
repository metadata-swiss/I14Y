using AutoMapper;
using AwesomeAssertions;
using NUnit.Framework;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

public class CatalogEntryMappingProfileTests
{
    private IMapper _mapper = null!;

    [Test]
    public void Map_CatalogEntry_DcatToIopCatalogEntry_Ok()
    {
        // Arrange
        var source = TestData.Core.CatalogSearchResultItem;

        // Act
        var result = _mapper.Map<Models.CatalogEntry>(source);

        // Assert
        result.Id.Should().Be(source.Id);
        result.PublisherName.De.Should().Be(source.Publisher.Name.De);
        result.Title.De.Should().Be(source.Title.De);
        result.PublicationLevel.Should().Be(source.PublicationLevel);
        result.PublicationLevelProposal.Should().Be(source.PublicationLevelProposal);
        result.RegistrationStatus.Should().Be(source.RegistrationStatus);
        result.RegistrationStatusProposal.Should().Be(source.RegistrationStatusProposal);
        result.System?.CreatedAt.Should().Be(source.System?.CreatedAt);
        result.System?.CreationType.Should().Be(source.System?.CreationType);
        result.System?.ModifiedAt.Should().Be(source.System?.ModifiedAt);
    }

    [SetUp]
    public void Setup()
    {
        _mapper = TestHelper.GetFullMapperConfiguration().CreateMapper();
    }
}