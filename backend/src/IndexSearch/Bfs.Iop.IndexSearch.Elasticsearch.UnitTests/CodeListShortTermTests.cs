using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Elasticsearch.CodeList;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

// 663 entries carry a one-character code. Discarding short terms left the query with nothing but the
// concept filter, so a search for "1" answered with all 2,345 entries of its concept — not an empty
// result a caller could notice, but a wrong one that looks like a working search.
[TestFixture]
internal sealed class CodeListShortTermTests
{
    private static readonly Guid Concept = Guid.Parse("08dadd0b-a8bb-0855-8607-f4a10a21126f");

    [TestCase("1")]
    [TestCase("K")]
    public void A_one_character_code_is_still_searched_for(string query)
    {
        Query(query).Should().Contain("\"must\"", "a dropped term leaves the concept filter matching everything");

        Codes(query).Should().Contain(query.ToLowerInvariant());
    }

    [Test]
    public void A_one_character_term_is_matched_exactly_rather_than_by_prefix()
    {
        // A prefix query for "1" would sweep 1, 1.1, 10, 100 — the whole decimal subtree.
        Query("1").Should().Contain("\"term\"").And.NotContain("\"prefix\"");
    }

    [Test]
    public void A_one_character_term_carries_no_fuzziness()
    {
        // One edit on a single character matches every other character, so "1" would match the name
        // "2". That is why simply keeping short terms would have been the wrong fix.
        Query("1").Should().NotContain("fuzziness");
    }

    [Test]
    public void A_one_character_term_skips_the_ngram_copy_it_cannot_match()
    {
        // The ngram field is built from 2- and 3-grams, so a single character analyses to no tokens.
        Query("1").Should().NotContain(".ngram");
    }

    [Test]
    public void A_longer_term_keeps_the_ngram_and_fuzzy_clauses()
    {
        var query = Query("statis");

        query.Should().Contain(".ngram");
        query.Should().Contain("fuzziness");
        query.Should().Contain("\"prefix\"");
    }

    [Test]
    public void A_two_character_term_is_exact_and_still_reaches_the_ngram_copy()
    {
        var query = Query("1a");

        query.Should().Contain("\"term\"").And.NotContain("\"prefix\"");
        query.Should().Contain(".ngram");
    }

    private static string Query(string text) => JsonSerializer.Serialize(
        CodeListQueryBuilder.BuildSearchBody(Concept, text, "de", filter: null, from: 0, size: 10));

    private static IReadOnlyList<string> Codes(string text)
    {
        using var document = JsonDocument.Parse(Query(text));

        var found = new List<string>();

        Walk(document.RootElement, found);

        return found;

        static void Walk(JsonElement element, List<string> found)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (property.Name == "code" && property.Value.ValueKind == JsonValueKind.Object
                        && property.Value.TryGetProperty("value", out var value))
                    {
                        found.Add(value.GetString()!);
                    }

                    Walk(property.Value, found);
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    Walk(item, found);
                }
            }
        }
    }
}
