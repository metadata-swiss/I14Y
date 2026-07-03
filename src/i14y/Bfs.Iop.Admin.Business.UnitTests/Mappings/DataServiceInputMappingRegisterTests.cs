using AwesomeAssertions;
using Bfs.Iop.Admin.Business.Mappings;
using Bfs.Iop.Core.Abstractions.Models;
using MapsterMapper;
using NUnit.Framework;
using System.Linq;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

[TestFixture(TestOf = typeof(DataServiceInputMappingRegister))]
public class DataServiceInputMappingRegisterTests
{
    private IMapper _mapper = null!;

    [Test]
    public void Map_DataServiceModel_To_DataServiceInput_Ok()
    {
        // Arrange
        var source = TestData.Core.DataServiceModel;

        // Act
        var result = _mapper.Map<Models.DataServiceInput>(source);

        // Assert
        result.License!.Code.Should().Be(source.License!.Code);
        result.AccessRightCode.Should().Be(source.AccessRights.Code);
        result.ConformTos.First().Href.Should().Be(source.ConformsTo.First().Uri);
        result.ContactPoints.First().TelWorkVoice.Should().Be(source.ContactPoints.First().HasTelephone);
        result.Description.De.Should().Be(source.Description.De);
        result.Documents.First().Href.Should().Be(source.Documentation.First().Uri);
        result.EndpointDescriptions.Should().BeEquivalentTo(source.EndpointDescriptions);
        result.EndpointUrls.Should().BeEquivalentTo(source.EndpointUrls);
        result.Id.Should().Be(source.Id);
        result.Keywords.First().Label!.De.Should().Be(source.Keywords.First().Label!.De);
        result.LandingPages.First().Href.Should().Be(source.LandingPages.First().Uri);
        result.Publisher.Id.Should().Be(source.Publisher.Id);
        result.Title.De.Should().Be(source.Title.De);
    }

    [Test]
    public void Map_DataServiceInput_To_DataServiceInputModel_Ok()
    {
        // Arrange
        var source = TestData.Model.DataServiceInput;

        // Act
        var result = _mapper.Map<DataServiceInputModel>(source);

        // Assert
        result.License!.Code.Should().Be(source.License!.Code);
        result.AccessRights.Code.Should().Be(source.AccessRightCode);
        result.ConformsTo.First().Uri.Should().Be(source.ConformTos.First().Href);
        result.ContactPoints.First().HasTelephone.Should().Be(source.ContactPoints.First().TelWorkVoice);
        result.Description.De.Should().Be(source.Description.De);
        result.Documentation.First().Uri.Should().Be(source.Documents.First().Href);
        result.EndpointDescriptions.First().Uri.Should().BeEquivalentTo(source.EndpointDescriptions.First().Href);
        result.EndpointUrls.First().Uri.Should().BeEquivalentTo(source.EndpointUrls.First().Href);
        result.Keywords.First().Label!.De.Should().Be(source.Keywords.First().Label!.De);
        result.LandingPages.First().Uri.Should().Be(source.LandingPages.First().Href);
        result.Publisher.Identifier.Should().BeEquivalentTo(source.Publisher.Identifier);
        result.Title.De.Should().Be(source.Title.De);
    }

    [SetUp]
    public void Setup() => _mapper = TestHelper.CreateMapper();
}