using AwesomeAssertions;
using Bfs.Iop.Admin.Business.Mappings;
using Bfs.Iop.DataAccess.Abstractions;
using MapsterMapper;
using NUnit.Framework;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

[TestFixture(TestOf = typeof(CatalogSearchCountResultMappingRegister))]
public class CatalogSearchCountResultMappingRegisterTests
{
    private IMapper _mapper = null!;

    [Test]
    public void Map_DcatCatalogSearchCountResult_To_IopCatalogSearchCountResult_Ok()
    {
        // Arrange
        var source = TestData.Core.CatalogSearchCountResult;

        // Act
        var result = _mapper.Map<Models.FilterCountResult>(source);

        // Assert
        result.PublicationLevels.Count.Should().Be(2);
        result.PublicationLevels[0].Reference.Should().Be(PublicationLevel.Internal.ToString());
        result.PublicationLevels[0].Label?.De.Should().Be(PublicationLevel.Internal.ToString());
        result.PublicationLevels[0].Label?.En.Should().Be(PublicationLevel.Internal.ToString());
        result.PublicationLevels[0].Label?.Fr.Should().Be(PublicationLevel.Internal.ToString());
        result.PublicationLevels[0].Label?.It.Should().Be(PublicationLevel.Internal.ToString());
        result.PublicationLevels[0].Label?.Rm.Should().Be(PublicationLevel.Internal.ToString());
        result.Publishers.Count.Should().Be(1);
        result.RegistrationStatuses.Count.Should().Be(3);
        result.Types.Count.Should().Be(2);
    }

    [SetUp]
    public void Setup()
    {
        _mapper = TestHelper.CreateMapper();
    }
}