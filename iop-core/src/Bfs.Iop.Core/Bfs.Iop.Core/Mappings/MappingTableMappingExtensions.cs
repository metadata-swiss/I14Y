using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Vocabularies;
using Bfs.Iop.Core.Tools;
using Bfs.Iop.Core.Services.Extensions;
using Bfs.Iop.Core.Services.Contracts;

namespace Bfs.Iop.Core.Mappings;

internal static class MappingTableMappingExtensions
{
    public static MappingTable MapToMappingTable(
        this MappingTableInputModel inputModel,
        Guid publisherId,
        Guid responsiblePersonId,
        Guid? responsibleDeputyId,
        IIdentifierGenerator identifierGenerator,
        MappingTable? entity = null)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));

        entity ??= new();

        entity.ConformsTo = inputModel.ConformsTo.MapToResources(entity.ConformsTo).ToList();
        entity.Description = inputModel.Description.MapToMultiLanguage();
        entity.Identifiers = inputModel.Identifiers.Any()
            ? inputModel.Identifiers.ToArray()
            : [identifierGenerator.GenerateIdentifier<MappingTable>(inputModel.Name)]; // Set the id as identifier, if none is provided
        entity.Keywords = inputModel.Keywords.MapToKeywords(entity.Keywords).ToList();
        entity.Name = inputModel.Name.MapToMultiLanguage();
        entity.PublisherId = publisherId;
        entity.ResponsibleDeputyId = responsibleDeputyId;
        entity.ResponsiblePersonId = responsiblePersonId;
        entity.SourceUri = inputModel.Source.Uri;
        entity.TargetUri = inputModel.Target.Uri;
        entity.Themes = inputModel.Themes.Select(x => x.Code).ToArray();
        entity.ValidFrom = inputModel.ValidFrom;
        entity.ValidTo = inputModel.ValidTo;
        entity.Version = inputModel.Version;

        return entity;
    }

    public static MappingTableModel MapToMappingTableModel(
        this MappingTable entity,
        IVocabulariesService vocabulariesService,
        MultiLanguage? sourceName = null,
        MultiLanguage? targetName = null)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        var themesVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<ThemesVocabulary>();

        return new()
        {
            ConformsTo = entity.ConformsTo.Select(x => x.MapToResourceModel()),
            Description = entity.Description.MapToMultiLanguageModel(),
            Id = entity.Id,
            Identifiers = [.. entity.Identifiers],
            Keywords = entity.Keywords.Select(k => k.MapToKeywordModel()),
            Name = entity.Name.MapToMultiLanguageModel(),
            PublicationLevel = entity.PublicationLevel,
            PublicationLevelProposal = entity.PublicationLevelProposal,
            Publisher = entity.Publisher.MapToAgentModel(vocabulariesService),
            RegistrationStatus = entity.RegistrationStatus,
            RegistrationStatusProposal = entity.RegistrationStatusProposal,
            ResponsibleDeputy = entity.ResponsibleDeputy?.MapToIopPersonModel(),
            ResponsiblePerson = entity.ResponsiblePerson?.MapToIopPersonModel(),
            System = entity.MapSystemInfoToSystemInfoModel(),
            Source = new() 
            { 
                Uri = entity.SourceUri,
                Name = sourceName?.MapToMultiLanguageModel()
             },
            Target = new()
            {
                Uri = entity.TargetUri,
                Name = targetName?.MapToMultiLanguageModel()
            },
            Themes = entity.Themes.Select(t => t.MapToVocabularyEntryModel(themesVocabulary)),
            ValidFrom = entity.ValidFrom,
            ValidTo = entity.ValidTo,
            Version = entity.Version,
        };
    }
}
