using AwesomeAssertions;
using Bfs.Iop.Admin.Business.Mappings;
using MapsterMapper;
using NUnit.Framework;
using System.Linq;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

[TestFixture(TestOf = typeof(DataServiceMappingRegister))]
public class DataServiceMappingRegisterTests
{
    private IMapper _mapper = null!;

    [Test]
    public void Map_DataServiceModel_DcatToIopAdminModel_Ok()
    {
        // Arrange
        var source = TestData.Core.DataServiceModel;

        // Act
        var result = _mapper.Map<Models.DataService>(source);

        // Assert

        result.AccessRights.Code.Should().Be(source.AccessRights.Code);
        result.License!.Code.Should().Be(source.License!.Code);
        result.ConformTos.First().Href.Should().Be(source.ConformsTo.First().Uri);
        result.ContactPoints.First().TelWorkVoice.Should().Be(source.ContactPoints.First().HasTelephone);
        result.Description.De.Should().Be(source.Description.De);
        result.Documents.First().Href.Should().Be(source.Documentation.First().Uri);
        result.EndpointDescriptions.Should().BeEquivalentTo(source.EndpointDescriptions);
        result.EndpointUrls.Should().BeEquivalentTo(source.EndpointUrls);
        result.Id.Should().Be(source.Id);
        result.Keywords.First().Label!.De.Should().Be(source.Keywords.First().Label!.De);
        result.LandingPages.First().Href.Should().Be(source.LandingPages.First().Uri);
        result.PublisherName.De.Should().Be(source.Publisher.Name.De);
        result.Status.Should().Be("UNDEFINED");
        result.ServesDatasets.Should().BeEmpty();
        result.Themes.First().Code.Should().Be(source.Themes.First().Code);
        result.Title.De.Should().Be(source.Title.De);
        result.Version.Should().Be(source.Version);
        result.VersionNotes.Fr.Should().Be(source.VersionNotes.Fr);
    }

    [Test]
    public void Map_DataServiceModel_To_DataServiceVersionSummary_Ok()
    {
        // Arrange
        var source = TestData.Core.DataServiceModel;

        // Act
        var result = _mapper.Map<Models.DataServiceVersionSummary>(source);

        // Assert
        result.Id.Should().Be(source.Id);
        result.EndpointUrls.Should().BeEquivalentTo(source.EndpointUrls);
        result.Title.De.Should().Be(source.Title.De);
        result.Version.Should().Be(source.Version);
    }

    [SetUp]
    public void Setup() => _mapper = TestHelper.CreateMapper();
}