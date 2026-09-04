using Bfs.Iop.DataAccess.Relational.Entities;
using System.Reflection;
using System.Text.Json;

namespace Bfs.Iop.DataAccess.Relational.Samples;

internal static class VocabularyConfigSamples
{
    private const string VocabularyResourceSuffix = ".Samples.Resources.vocabularies.json";

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static IEnumerable<VocabularyConfig> Generate()
    {
        using Stream stream = OpenEmbeddedResource(VocabularyResourceSuffix);

        return JsonSerializer.Deserialize<VocabularyConfig[]>(stream, _jsonOptions)
            ?? [];
    }

    private static Stream OpenEmbeddedResource(string resourceSuffix)
    {
        Assembly assembly = typeof(VocabularyConfigSamples).Assembly;

        string resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(x => x.EndsWith(resourceSuffix, StringComparison.Ordinal))
            ?? throw new InvalidOperationException(
                $"Embedded resource ending with '{resourceSuffix}' was not found.");

        return assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{resourceName}' could not be opened.");
    }
}