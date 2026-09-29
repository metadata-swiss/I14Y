using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Abstractions.Exceptions;
using Client = Bfs.Iop.IndexSearch.ApiClient;

namespace Bfs.Iop.Core.Mappings;


internal static class CatalogSearchFilterMappingExtensions
{
    public static Client.CatalogSearchFilter MapToIndexSearchFilter(this CatalogSearchFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        return new Client.CatalogSearchFilter
        {
            AccessRights = [.. filter.AccessRights],
            AttributedAgentIdentifiers = [.. filter.AttributedAgentIdentifiers],
            BusinessEvents = [.. filter.BusinessEvents],
            ConceptTypes = [.. Cross<ConceptType, Client.ConceptType>(filter.ConceptValueTypes)],
            Formats = [.. filter.Formats],
            LifeEvents = [.. filter.LifeEvents],
            PublicationLevels =
                [.. Cross<PublicationLevel, Client.PublicationLevel>(filter.PublicationLevels)],
            PublicationLevelProposals =
                [.. Cross<PublicationLevel, Client.PublicationLevel>(filter.PublicationLevelProposals)],
            PublisherIdentifiers = [.. filter.PublisherIdentifiers],
            RegistrationStatuses =
                [.. Cross<RegistrationStatus, Client.RegistrationStatus>(filter.RegistrationStatuses)],
            RegistrationStatusProposals =
                [.. Cross<RegistrationStatus, Client.RegistrationStatus>(filter.RegistrationStatusProposals)],
            Structure = Crossed<SearchStructureOption, Client.IndexStructureOption>(filter.Structure),
            Themes = [.. filter.Themes],
            Types = [.. Cross<SearchResourceType, Client.SearchResourceType>(filter.Types)],
        };
    }

    /// <summary>
    ///     A single optional selection. Null means the caller filtered on nothing; a value that will
    ///     not cross is refused rather than quietly becoming "no filter", which would widen the set.
    /// </summary>
    private static TTarget? Crossed<TSource, TTarget>(TSource? value)
        where TSource : struct, Enum
        where TTarget : struct, Enum =>
        value is null
            ? null
            : IndexSearchEnumCrossing.Cross<TSource, TTarget>(value.Value)
                ?? throw new BadRequestException(
                    $"'{value}' is not a value of {typeof(TSource).Name} that the search service accepts.");

    /// <summary>
    ///     Crosses each selected value, and refuses the request if any of them will not cross.
    ///     <para>
    ///         Dropping it instead would return results the caller did not ask for: a filter that
    ///         silently disappears widens the set. Both enums are generated from the same contracts,
    ///         so a value that cannot cross means the generated client has drifted from the service -
    ///         which needs to be loud, not absorbed.
    ///     </para>
    /// </summary>
    private static IEnumerable<TTarget> Cross<TSource, TTarget>(IEnumerable<TSource> values)
        where TSource : struct, Enum
        where TTarget : struct, Enum =>
        [
            .. values.Select(x => IndexSearchEnumCrossing.Cross<TSource, TTarget>(x)
                ?? throw new BadRequestException(
                    $"'{x}' is not a value of {typeof(TSource).Name} that the search service accepts.")),
        ];
}
