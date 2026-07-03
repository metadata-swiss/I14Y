using AwesomeAssertions;
using Bfs.Iop.Admin.Business.Mappings;
using MapsterMapper;
using NUnit.Framework;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

[TestFixture(TestOf = typeof(MultiLanguageMappingRegister))]
public class MultiLanguageMappingRegisterTests
{
    private IMapper _mapper = null!;

    [Test]
    public void Map_DcatMultiLanguage_FromMultiLanguage_Ok()
    {
        // Arrange
        var source = new Models.MultiLanguage { De = "de", En = "en", Fr = "fr", It = "it", Rm = "rm" };
        var exptectedResult = new Core.Abstractions.Models.MultiLanguageModel { De = "de", En = "en", Fr = "fr", It = "it", Rm = "rm" };

        // Act
        var result = _mapper.Map<Core.Abstractions.Models.MultiLanguageModel>(source);

        // Assert
        result.Should().BeEquivalentTo(exptectedResult);
    }

    [Test]
    public void Map_MultiLanguage_FromDcatMultiLanguage_Ok()
    {
        // Arrange
        var source = new Core.Abstractions.Models.MultiLanguageModel { De = "de", En = "en", Fr = "fr", It = "it", Rm = "rm" };
        var exptectedResult = new Models.MultiLanguage { De = "de", En = "en", Fr = "fr", It = "it", Rm = "rm" };

        // Act
        var result = _mapper.Map<Models.MultiLanguage>(source);

        // Assert
        result.Should().BeEquivalentTo(exptectedResult);
    }

    [SetUp]
    public void Setup() => _mapper = TestHelper.CreateMapper();
}