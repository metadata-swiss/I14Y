using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Models.Indexing;

namespace Bfs.Iop.Core.IndexForwarding.UnitTests;

/// <summary>
/// Guards the one structural risk in having two producers for <see cref="CatalogIndexEntry"/>.
/// <para>
/// A rebuild projects the entry from EF entities (<c>Bfs.Iop.Core.Data.Indexing.IndexEntryFactory</c>);
/// a single write projects it from a domain model (<see cref="IndexEntryProjection"/>). They live in
/// different assemblies, so nothing makes them agree — and if they disagree, a resource's indexed
/// content depends on whether it was last touched by a user edit or by a rebuild. That is invisible
/// until someone notices a field that searches correctly right after an edit and stops working after
/// the next rebuild.
/// </para>
/// <para>
/// This asserts the weaker, cheap property: every settable field on the projection is populated by
/// the model-side producer for at least one resource kind. It cannot see the entity-side producer
/// from here — that is in another assembly — so it catches "a field was added to the record and to
/// the document factory but never to a projection", which is the common half of the mistake.
/// </para>
/// </summary>
[TestFixture]
public sealed class IndexEntryProjectionShapeTests
{
    /// <summary>
    /// Fields that are legitimately never set by the forward path, with the reason. Anything else
    /// left unset is a projection gap, not a design choice.
    /// </summary>
    private static readonly Dictionary<string, string> _deliberatelyUnset = new()
    {
        // Resolved by the IndexSearch service from the object store; Core cannot know it.
        [nameof(CatalogIndexEntry.HasStructure)] = "resolved from the object store by IndexReconciler",
    };

    [Test]
    public void EveryProjectionFieldIsPopulatedBySomeResourceKind()
    {
        var populated = new HashSet<string>();

        foreach (var entry in AllKinds())
        {
            foreach (var property in typeof(CatalogIndexEntry).GetProperties())
            {
                var value = property.GetValue(entry);

                // "Populated" means present, not merely default: an empty collection is what an
                // absent one looks like, so it does not count as evidence the field is wired up.
                var isSet = value switch
                {
                    null => false,
                    string s => !string.IsNullOrEmpty(s),
                    System.Collections.IEnumerable e => e.GetEnumerator().MoveNext(),
                    _ => true,
                };

                if (isSet)
                {
                    populated.Add(property.Name);
                }
            }
        }

        var missing = typeof(CatalogIndexEntry).GetProperties()
            .Select(x => x.Name)
            .Where(name => !populated.Contains(name) && !_deliberatelyUnset.ContainsKey(name))
            .ToList();

        missing.Should().BeEmpty(
            "every field on CatalogIndexEntry must be filled in by at least one projection — an " +
            "unpopulated field is indexed as absent, which is not an error, just results that never match");
    }

    private static IEnumerable<CatalogIndexEntry> AllKinds() =>
    [
        IndexEntryProjection.FromDataset(TestModels.Dataset()),
        IndexEntryProjection.FromDataService(TestModels.DataService()),
        IndexEntryProjection.FromPublicService(TestModels.PublicService()),
        IndexEntryProjection.FromConcept(TestModels.Concept()),
        IndexEntryProjection.FromMappingTable(TestModels.MappingTable()),
    ];
}
