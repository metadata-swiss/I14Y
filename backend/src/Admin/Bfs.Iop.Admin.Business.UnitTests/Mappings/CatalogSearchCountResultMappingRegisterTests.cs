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

    [Test]
    public void Map_Themes_To_FilterCountResult_Uses_The_Search_Key_As_Reference()
    {
        // Arrange
        var source = new Core.Abstractions.Models.SearchCountResultModel
        {
            Themes = [new() {Count = 3, Value = new VocabularyEntryModel {Code = "101", VocabularyIdentifier = "VOCAB_EU_DATA_THEME"}, SearchKey = "VOCAB_EU_DATA_THEME!101"}],
            Formats = [new() {Count = 2, Value = new VocabularyEntryModel {Code = "CSV", VocabularyIdentifier = "VOCAB_I14Y_FILE_TYPE"}}]
        };

        // Act
        var result = _mapper.Map<Models.FilterCountResult>(source);

        // Assert
        result.Themes[0].Reference.Should().Be("VOCAB_EU_DATA_THEME!101");
        result.Formats[0].Reference.Should().Be("CSV");
    }

    [SetUp]
    public void Setup()
    {
        _mapper = TestHelper.CreateMapper();
    }
}