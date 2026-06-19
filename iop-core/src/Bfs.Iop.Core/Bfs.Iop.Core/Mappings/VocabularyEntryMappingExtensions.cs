using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Vocabularies;

namespace Bfs.Iop.Core.Mappings;

internal static class VocabularyEntryMappingExtensions
{
    public static VocabularyEntryModel MapToVocabularyEntryModel(
        this string code,
        VocabularyModel vocabulary)
    {
        ArgumentNullException.ThrowIfNull(code, nameof(code));
        ArgumentNullException.ThrowIfNull(vocabulary, nameof(vocabulary));

        return GetVocabularyEntryModel(code, vocabulary.Identifier, vocabulary.Entries);
    }

    public static VocabularyEntryModel MapToVocabularyEntryModel(
        this string code,
        IdentifiedVocabularyBase vocabulary)
    {
        // There are data errors in the database (example: code == "") and the code validation should not throw in this case.
        ArgumentNullException.ThrowIfNull(code, nameof(code));
        ArgumentNullException.ThrowIfNull(vocabulary, nameof(vocabulary));

        return GetVocabularyEntryModel(code, vocabulary.Identifier, vocabulary.Entries);
    }

    public static IEnumerable<VocabularyEntryModel> MapToVocabularyEntryModels(
        this IEnumerable<string> codes,
        IdentifiedVocabularyBase vocabulary)
    {
        ArgumentNullException.ThrowIfNull(codes, nameof(codes));
        ArgumentNullException.ThrowIfNull(vocabulary, nameof(vocabulary));

        return codes
            .Select(code => code.MapToVocabularyEntryModel(vocabulary))
            .Where(x => x.Code is not null);
    }

    private static VocabularyEntryModel GetVocabularyEntryModel(
        string code,
        string vocabularyIdentifier,
        IEnumerable<VocabularyEntryModel> entries)
    {
        var entry = entries.SingleOrDefault(e => e.Code == code);

        if (entry is null)
        {
            //TODO: how should we handle this? should we?
            Console.WriteLine($"Warning: Code '{code}' not found in '{vocabularyIdentifier}' vocabulary.");
            entry = new VocabularyEntryModel() { Code = code };
        }

        return entry;
    }
}
