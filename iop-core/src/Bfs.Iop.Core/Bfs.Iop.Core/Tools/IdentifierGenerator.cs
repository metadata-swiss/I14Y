using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Validation.Extensions;
using Slugify;

namespace Bfs.Iop.Core.Tools;

internal sealed class IdentifierGenerator : IIdentifierGenerator
{
    private readonly IopDbContext _dbContext;
    private readonly SlugHelperConfiguration _slugHelperConfiguration;
    private readonly SlugHelper _slugHelper;

    public IdentifierGenerator(IopDbContext dbContext)
    {
        _dbContext = dbContext;

        _slugHelperConfiguration = new SlugHelperConfiguration()
        {
            MaximumLength = 80,
            TrimWhitespace = true,
        };

        _slugHelper = new SlugHelper(_slugHelperConfiguration);
    }

    public string GenerateIdentifier<T>(MultiLanguageModel model) where T : EntityBase
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        var text = TryGetFallbackLanguage(model) ?? 
            throw new InvalidOperationException("No valid text has been found to generate an identifier.");
        
        var baseIdentifier = _slugHelper.GenerateSlug(text);
        var identifier = baseIdentifier;
        var iteration = 0;

        while(IdentifierExists<T>(identifier))
        {
            identifier = GetAdjustedIdentifierAfterConflict(baseIdentifier, ++iteration);
        }

        if (!identifier.IsValidIdentifier())
        {
            throw new BadRequestException($"The generated value '{identifier}' is not a valid identifier.");
        }

        return identifier;
    }

    private string GetAdjustedIdentifierAfterConflict(string baseIdentifier, int iteration)
    {
        var sufix = $"-{iteration}";

        if ((baseIdentifier.Length + sufix.Length) > _slugHelperConfiguration.MaximumLength)
        {
            baseIdentifier = baseIdentifier[..(_slugHelperConfiguration.MaximumLength.Value - sufix.Length)];
        }

        return $"{baseIdentifier}{sufix}";
    }

    private static string? TryGetFallbackLanguage(MultiLanguageModel model)
    {
        // The order is important
        var fallbackLanguages = new string[] { "de", "fr", "it", "en", "rm" };

        var dic = model.ToDictionary();

        var languageWithText = fallbackLanguages
            .FirstOrDefault(language =>
                dic.TryGetValue(language, out var value) &&
                !string.IsNullOrWhiteSpace(value));

        if (languageWithText is null)
        {
            return null;
        }

        return dic[languageWithText];
    }

    private bool IdentifierExists<T>(string identifier) where T : EntityBase
    {
        // ToDo: Add the property "Identifiers" to the "PublishableEntity" in order to avoid this if case

        var exists = false;

        if (typeof(T) == typeof(Dataset))
        {
            exists = _dbContext.Set<Dataset>().Any(x => x.Identifier.Contains(identifier));
        }
        else if (typeof(T) == typeof(DataService))
        {
            exists = (_dbContext.Set<DataService>().Any(x => x.Identifiers.Contains(identifier)));
        }
        else if (typeof(T) == typeof(MappingTable))
        {
            exists = (_dbContext.Set<MappingTable>().Any(x => x.Identifiers.Contains(identifier)));
        }
        else if (typeof(T) == typeof(PublicService))
        {
            exists = (_dbContext.Set<PublicService>().Any(x => x.Identifiers.Contains(identifier)));
        }
        else if (typeof(T) == typeof(IopConcept))
        {
            exists = (_dbContext.Set<IopConcept>().Any(x => x.Identifiers.Contains(identifier)));
        }
        else
        {
            throw new NotSupportedException($"The type '{typeof(T)}' is not supported.");
        }

        return exists;
    }
}
