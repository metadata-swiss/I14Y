using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Contracts;
using CoreModels = Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.IndexSearch.Contracts.UnitTests;

[TestFixture]
public class EnumParityTests
{
    private static IEnumerable<string> NamesOf<T>() where T : struct, Enum =>
        Enum.GetNames<T>().OrderBy(x => x, StringComparer.Ordinal);

    private static void AssertSameNames<TContract, TCore>()
        where TContract : struct, Enum
        where TCore : struct, Enum =>
        NamesOf<TContract>().Should().Equal(NamesOf<TCore>());

    [Test]
    public void ResourceType_matches_core() =>
        AssertSameNames<IndexResourceType, CoreModels.SearchResourceType>();

    [Test]
    public void PublicationLevel_matches_core() =>
        AssertSameNames<IndexPublicationLevel, CoreModels.PublicationLevel>();

    [Test]
    public void RegistrationStatus_matches_core() =>
        AssertSameNames<IndexRegistrationStatus, CoreModels.RegistrationStatus>();

    [Test]
    public void ConceptType_matches_core() =>
        AssertSameNames<IndexConceptType, CoreModels.ConceptType>();

    [Test]
    public void CreationType_matches_core() =>
        AssertSameNames<IndexCreationType, CoreModels.CreationType>();

    // The languages are part of the index mapping, which declares one analyzed sub-field per language.
    [Test]
    public void Languages_are_the_five_the_index_maps() =>
        LocalizedText.Languages.Should().Equal("de", "en", "fr", "it", "rm");
}
