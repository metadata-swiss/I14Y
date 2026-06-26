using AutoMapper;
using AwesomeAssertions;
using NUnit.Framework;
using System.Linq;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

public class DatasetMappingProfileTests
{
    private IMapper _mapper = null!;

    [Test]
    public void Map_Dataset_DcatToIopAdminModel_Ok()
    {
        // Arrange
        var source = TestData.Core.DcatDatasetModel;

        // Act
        var result = _mapper.Map<Models.Dataset>(source);

        // Assert
        result.AccessRights.Code.Should().Be(source.AccessRights.Code);
        result.Frequency?.Code.Should().Be(source.Frequency.Code);
        result.ConformTos.First().Href.Should().Be(source.ConformsTo.First().Uri);
        result.ContactPoints.First().TelWorkVoice.Should().Be(source.ContactPoints.First().HasTelephone);
        result.Description.De.Should().Be(source.Description.De);
        result.Documents.First().Href.Should().Be(source.Documentation.First().Uri);
        result.Id.Should().Be(source.Id);
        result.Identifiers.Should().BeEquivalentTo(source.Identifiers);
        result.Keywords.First().Label.De.Should().Be(source.Keywords.First().Label.De);
        result.LandingPages.First().Href.Should().Be(source.LandingPages.First().Uri);
        result.Relations!.First().Href.Should().Be(source.Relations.First().Uri);
        result.Languages.First().Should().Be(source.Languages.First().Code);
        result.LastUpdated.Should().Be(source.Modified);
        result.Published.Should().Be(source.Issued);
        result.PublisherName.De.Should().Be(source.Publisher.Name.De);
        result.SpatialCoverages.Should().BeEquivalentTo(source.Spatial);
        result.PublicationLevelProposal.Should().Be(source.PublicationLevelProposal);
        result.PublicationLevel.Should().Be(source.PublicationLevel);
        result.RegistrationStatusProposal.Should().Be(source.RegistrationStatusProposal);
        result.RegistrationStatus.Should().Be(source.RegistrationStatus);
        result.TemporalCoverage.First().Start.Should().Be(source.TemporalCoverage.First().Start);
        result.Themes.First().Code.Should().Be(source.Themes.First().Code);
        result.Title.De.Should().Be(source.Title.De);
        result.Version.Should().Be(source.Version);
        result.VersionNotes.Fr.Should().Be(source.VersionNotes.Fr);
        result.Image.Count().Should().Be(source.Images!.Count());
        result.QualifiedRelation.Should().HaveCount(source.QualifiedRelations.Count());
        result.QualifiedAttribution.Should().HaveCount(source.QualifiedAttributions.Count());
    }

    [Test]
    public void Map_Dataset_To_DatasetVersionSummary_Ok()
    {
        // Arrange
        var source = TestData.Core.DcatDatasetModel;

        // Act
        var result = _mapper.Map<Models.DatasetVersionSummary>(source);

        // Assert
        result.Id.Should().Be(source.Id);
        result.Identifiers.Should().BeEquivalentTo(source.Identifiers);
        result.Title.De.Should().Be(source.Title.De);
        result.Version.Should().Be(source.Version);
    }

    [SetUp]
    public void Setup()
    {
        _mapper = TestHelper.GetFullMapperConfiguration().CreateMapper();
    }
}