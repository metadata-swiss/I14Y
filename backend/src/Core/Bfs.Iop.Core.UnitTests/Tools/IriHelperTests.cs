using AwesomeAssertions;
using Bfs.Iop.Core.Tools;

namespace Bfs.Iop.Core.UnitTests.Tools;

[TestFixture(TestOf = typeof(IriHelper))]
internal sealed class IriHelperTests
{
    [TestCase("https://register.ld.admin.ch/i14y/concept/X/version/1.0.0", "X", "1.0.0")]
    [TestCase("https://iri.i14y.d.c.bfs.admin.ch/concept/X/version/1.0.0", "X", "1.0.0")]
    [TestCase("https://public.i14y.d.c.bfs.admin.ch/concept/X/version/1.0.0", "X", "1.0.0")]
    [TestCase("https://iri.i14y.a.c.bfs.admin.ch/concept/DV_VERSICHERUNGSBRANCHEN/version/2.3.0", "DV_VERSICHERUNGSBRANCHEN", "2.3.0")]
    public void TryExtractConceptIdentifierAndVersion_Succeeds_ForAnyHost(string uri, string expectedId, string expectedVersion)
    {
        var success = IriHelper.TryExtractConceptIdentifierAndVersion(uri, out var identifier, out var version);

        success.Should().BeTrue();
        identifier.Should().Be(expectedId);
        version.Should().Be(expectedVersion);
    }

    [TestCase("https://example.org/source")]
    [TestCase("https://register.ld.admin.ch/i14y/dataset/MY_DATASET")]
    [TestCase("")]
    public void TryExtractConceptIdentifierAndVersion_Fails_ForNonConceptUri(string uri)
    {
        var success = IriHelper.TryExtractConceptIdentifierAndVersion(uri, out var identifier, out var version);

        success.Should().BeFalse();
        identifier.Should().Be(string.Empty);
        version.Should().Be(string.Empty);
    }

    [TestCase("https://register.ld.admin.ch/i14y/concept/X/CH/version/1.0.0", "CH")]
    [TestCase("https://iri.i14y.d.c.bfs.admin.ch/concept/X/DE/version/1.0.0", "DE")]
    [TestCase("https://public.i14y.d.c.bfs.admin.ch/concept/nogaCode/A01/version/1.0.0", "A01")]
    public void TryExtractCodeFromConceptCodeIri_ExtractsCode_ForAnyHost(string uri, string expectedCode)
    {
        var success = IriHelper.TryExtractCodeFromConceptCodeIri(uri, out var code);

        success.Should().BeTrue();
        code.Should().Be(expectedCode);
    }

    [TestCase("https://register.ld.admin.ch/i14y/concept/X/version/1.0.0", "1.0.0")]
    [TestCase("https://iri.i14y.a.c.bfs.admin.ch/mappingtable/MT/version/2.5.1", "2.5.1")]
    [TestCase("https://any.host/concept/X/Y/version/9.9.9", "9.9.9")]
    public void ExtractVersion_ReturnsVersion_ForAnyUriWithVersionSegment(string uri, string expected)
    {
        IriHelper.ExtractVersion(uri).Should().Be(expected);
    }

    [TestCase("https://register.ld.admin.ch/i14y/dataset/MY_DATASET")]
    [TestCase("https://example.org/source")]
    public void ExtractVersion_ReturnsNull_WhenNoVersionSegment(string uri)
    {
        IriHelper.ExtractVersion(uri).Should().BeNull();
    }
}
