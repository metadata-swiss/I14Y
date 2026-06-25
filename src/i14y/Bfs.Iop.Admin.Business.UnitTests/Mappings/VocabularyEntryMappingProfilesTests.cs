using AutoMapper;
using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Models;
using NUnit.Framework;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

public class VocabularyEntryMappingProfilesTests
{
    private IMapper _mapper = null!;

    [Test]
    public void Map_VocabularyEntry_DcatToIopAdminModel_Ok()
    {
        // Arrange
        var source = TestData.Core.VocabularyEntryModel;

        // Act
        var result = _mapper.Map<Models.VocabularyEntry>(source);

        // Assert
        result.Code.Should().Be(source.Code);
        result.Name.De.Should().Be(source.Name.De);
    }

    [Test]
    public void Map_VocabularyEntry_IopAdminModelToDcat_Ok()
    {
        // Arrange
        var source = TestData.Model.AccessRights;

        // Act
        var result = _mapper.Map<VocabularyEntryModel>(source);

        // Assert
        result.Code.Should().Be(source.Code);
        result.Name.De.Should().Be(source.Name.De);
    }

    [SetUp]
    public void Setup()
    {
        _mapper = TestHelper.GetFullMapperConfiguration().CreateMapper();
    }
}