using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using Bfs.Iop.DataAccess.Abstractions.Exceptions;
using Bfs.Iop.IndexSearch.Contracts;

using IndexSearchFilter = Bfs.Iop.IndexSearch.Contracts.Search.CatalogSearchFilter;

namespace Bfs.Iop.Core.Mappings;

internal static class CatalogSearchFilterMappingExtensions
{
    public static IndexSearchFilter MapToIndexSearchFilter(this CatalogSearchFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        return new IndexSearchFilter
        {
            AccessRights = [.. filter.AccessRights],
            AttributedAgentIdentifiers = [.. filter.AttributedAgentIdentifiers],
            BusinessEvents = [.. filter.BusinessEvents],
            ConceptTypes = [.. filter.ConceptValueTypes],
            Formats = [.. filter.Formats],
            LifeEvents = [.. filter.LifeEvents],
            PublicationLevels = [.. filter.PublicationLevels],
            PublicationLevelProposals = [.. filter.PublicationLevelProposals],
            PublisherIdentifiers = [.. filter.PublisherIdentifiers],
            RegistrationStatuses = [.. filter.RegistrationStatuses],
            RegistrationStatusProposals = [.. filter.RegistrationStatusProposals],
            Structure = Structure(filter.Structure),
            Themes = [.. filter.Themes],
            Types = [.. filter.Types],
        };
    }


    private static IndexStructureOption? Structure(SearchStructureOption? structure) => structure switch
    {
        null => null,
        SearchStructureOption.WithStructure => IndexStructureOption.WithStructure,
        SearchStructureOption.WithoutStructure => IndexStructureOption.WithoutStructure,
        _ => throw new BadRequestException(
            $"'{structure}' is not a structure option that the search service accepts."),
    };
}
