using System.Reflection;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Serialization.Json;
using Bfs.Iop.Core.Data.Entities;

namespace Bfs.Iop.Core.Data.Samples;

internal static class IopConceptSamples
{
    public static IEnumerable<IopConcept> Generate()
    {
        foreach (VocabularyConfig config in VocabularyConfigSamples.Generate())
        {
            IopConceptModel model = LoadConceptModel(config);

            var concept = new IopConcept
            {
                CodeListEntries = MapCodeListEntries(model.CodeListEntries),
                CodeListEntryDefaultSortProperty = model.CodeListEntryDefaultSortProperty,
                CodeListEntryValueMaxLength = model.CodeListEntryValueMaxLength,
                CodeListEntryValueType = model.CodeListEntryValueType,
                ConceptType = model.ConceptType,
                Description = ToMultiLanguage(model.Description),
                Identifiers = model.Identifiers.Any()
                    ? [.. model.Identifiers]
                    : [config.ConceptIdentifier],
                IsLocked = true,
                MaxLength = model.MaxLength,
                MaxValue = model.MaxValue,
                MeasurementUnit = model.MeasurementUnit,
                MinLength = model.MinLength,
                MinValue = model.MinValue,
                Name = ToMultiLanguage(model.Name),
                NumberDecimals = model.NumberDecimals,
                Pattern = model.Pattern,
                PublicationLevel = model.PublicationLevel,
                RegistrationStatus = model.RegistrationStatus,
                PublisherId = AgentSamples.I14YTestId,
                ResponsiblePersonId = IopPersonSamples.MaxMusterId,
                ResponsibleDeputyId = null,
                Themes = model.Themes
                    .Select(x => x.Code)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList(),
                ValidFrom = model.ValidFrom,
                ValidTo = model.ValidTo,
                Version = string.IsNullOrWhiteSpace(model.Version)
                    ? config.ConceptVersion
                    : model.Version
            };

            foreach (CodeListEntry entry in concept.CodeListEntries ?? [])
            {
                entry.IopConcept = concept;
            }

            yield return concept;
        }
    }

    private static IopConceptModel LoadConceptModel(VocabularyConfig config)
    {
        string resourceSuffix =
            $".Samples.Resources.Vocabularies.{config.ConceptIdentifier}__{config.ConceptVersion}.json";

        using Stream stream = OpenEmbeddedResource(resourceSuffix);

        return IopJsonSerializer.DeserializeStreamData<IopConceptModel>(
            stream,
            setRequiredPropertiesToDefaultValueIfNull: true);
    }

    private static Stream OpenEmbeddedResource(string resourceSuffix)
    {
        Assembly assembly = typeof(IopConceptSamples).Assembly;

        string resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(x => x.EndsWith(resourceSuffix, StringComparison.Ordinal))
            ?? throw new InvalidOperationException(
                $"Embedded resource ending with '{resourceSuffix}' was not found.");

        return assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{resourceName}' could not be opened.");
    }

    private static ICollection<CodeListEntry> MapCodeListEntries(
        IEnumerable<CodeListEntryModel>? models)
    {
        if (models is null)
        {
            return [];
        }

        return models
            .Select(x => new CodeListEntry
            {
                Id = x.Id,
                Code = x.Code,
                Name = ToMultiLanguage(x.Name),
                Description = x.Description is null
                    ? null
                    : ToMultiLanguage(x.Description),
                ValidFrom = x.ValidFrom,
                ValidTo = x.ValidTo
            })
            .ToList();
    }

    private static MultiLanguage ToMultiLanguage(MultiLanguageModel model)
    {
        return new MultiLanguage
        {
            De = model.De,
            En = model.En,
            Fr = model.Fr,
            It = model.It,
            Rm = model.Rm
        };
    }
}