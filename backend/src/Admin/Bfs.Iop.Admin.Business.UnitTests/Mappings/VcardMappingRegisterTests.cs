using AwesomeAssertions;
using Bfs.Iop.Admin.Business.Mappings;
using Bfs.Iop.DataAccess.Abstractions;
using MapsterMapper;
using NUnit.Framework;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

[TestFixture(TestOf = typeof(VcardMappingRegister))]
public class VcardMappingRegisterTests
{
    private IMapper _mapper = null!;

    [Test]
    public void Map_Vcard_DcatToIopAdminModel_Ok()
    {
        // Arrange
        var source = TestData.Core.VCardModel;

        // Act
        var result = _mapper.Map<Models.Vcard>(source);

        // Assert
        result.AdrWork.De.Should().Be(source.HasAddress.De);
        result.EmailInternet.Should().Be(source.HasEmail);
        result.Fn.De.Should().Be(source.Fn.De);
        result.Note.De.Should().Be(source.Note.De);
        result.TelWorkVoice.Should().Be(source.HasTelephone);
    }

    [Test]
    public void Map_Vcard_IopAdminModelToDcat_Ok()
    {
        // Arrange
        var source = TestData.Model.ContactPoint;

        // Act
        var result = _mapper.Map<VCardModel>(source);

        // Assert
        result.HasAddress.De.Should().Be(source.AdrWork.De);
        result.Kind.Should().Be(VCardKind.Organization);
        result.HasEmail.Should().Be(source.EmailInternet);
        result.Fn.De.Should().Be(source.Fn.De);
        result.Note.De.Should().Be(source.Note.De);
        result.HasTelephone.Should().Be(source.TelWorkVoice);
    }

    [SetUp]
    public void Setup() => _mapper = TestHelper.CreateMapper();
}