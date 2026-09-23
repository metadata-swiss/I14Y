using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Bfs.Iop.DataAccess.Relational.Extensions;

internal static class IopDbContextExtensions
{
    public static IQueryable<Agent> CreateGetAgentsQuery(
        this IopDbContext iopDbContext,
        bool asNoTracking,
        EntityIncludeLevel entityIncludeLevel,
        Expression<Func<Agent, bool>>? filter = null)
    {
        filter ??= x => true;

        var query = asNoTracking
            ? iopDbContext.Agents.AsNoTracking()
            : iopDbContext.Agents.AsQueryable();

        query = entityIncludeLevel switch
        {
            EntityIncludeLevel.All => buildEntityIncludeLevelAllQuery(query),
            _ => query.Include(d => d.Name),
        };

        static IQueryable<Agent> buildEntityIncludeLevelAllQuery(IQueryable<Agent> query)
        {
            query = query
                .Include(d => d.ContactPoint)
                .Include(d => d.Description)
                .Include(d => d.Images)
                .Include(d => d.Name)
                .Include(d => d.SubAgents)
                    .ThenInclude(s => s.SubAgent)
                ;

            return query;
        }

        return query
            .Where(filter)
            .OrderBy(x => x.Id);
    }

    public static IQueryable<DataService> CreateGetDataServicesQuery(
        this IopDbContext iopDbContext,
        bool asNoTracking,
        EntityIncludeLevel entityIncludeLevel,
        bool loadPersonalInformation,
        Expression<Func<DataService, bool>>? filter)
    {
        filter ??= x => true;

        var query = asNoTracking
             ? iopDbContext.DataServices.AsNoTracking()
             : iopDbContext.DataServices.AsQueryable();

        query = entityIncludeLevel switch
        {
            EntityIncludeLevel.Minimal => query.Include(d => d.Publisher),
            _ => buildEntityIncludeLevelAllQuery(query, loadPersonalInformation),
        };

        return query
            .Where(filter)
            .OrderBy(x => x.Id);

        static IQueryable<DataService> buildEntityIncludeLevelAllQuery(IQueryable<DataService> query, bool userHasValidToken)
        {
            query = query
                .Include(d => d.ConformsTo)
                .Include(d => d.ContactPoint)
                .Include(d => d.Datasets)
                .Include(d => d.Documentation)
                .Include(d => d.EndpointDescription)
                .Include(d => d.EndpointUrl)
                .Include(d => d.Keyword)
                .Include(d => d.LandingPage)
                .Include(d => d.Publisher);

            if (userHasValidToken)
            {
                query = query
                    .Include(d => d.ResponsiblePerson)
                    .Include(d => d.ResponsibleDeputy);
            }

            return query;
        }
    }

    public static IQueryable<Dataset> CreateGetDatasetsQuery(
        this IopDbContext iopDbContext,
        bool asNoTracking,
        EntityIncludeLevel entityIncludeLevel,
        bool loadPersonalInformation,
        Expression<Func<Dataset, bool>>? filter = null)
    {
        filter ??= x => true;

        var query = asNoTracking
            ? iopDbContext.Datasets.AsNoTracking()
            : iopDbContext.Datasets.AsQueryable();

        query = entityIncludeLevel switch
        {
            EntityIncludeLevel.All => buildEntityIncludeLevelAllQuery(query, loadPersonalInformation),
            _ => query.Include(d => d.Publisher),
        };

        return query
            .Where(filter)
            .OrderBy(x => x.Id);

        static IQueryable<Dataset> buildEntityIncludeLevelAllQuery(IQueryable<Dataset> query, bool loadPersonalInformation)
        {
            query = query
                .Include(d => d.ConformsTo)
                .Include(d => d.ContactPoint)
                .Include(d => d.Distributions)
                    .ThenInclude(di => di.AccessServices)
                .Include(d => d.Distributions)
                    .ThenInclude(di => di.AccessUrl)
                .Include(d => d.Distributions)
                    .ThenInclude(di => di.Checksum)
                .Include(d => d.Distributions)
                    .ThenInclude(di => di.ConformsTo)
                .Include(d => d.Distributions)
                    .ThenInclude(di => di.Coverage)
                .Include(d => d.Distributions)
                    .ThenInclude(di => di.Documentation)
                .Include(d => d.Distributions)
                    .ThenInclude(di => di.DownloadUrl)
                .Include(d => d.Distributions)
                    .ThenInclude(di => di.Image)
                .Include(d => d.Documentation)
                .Include(d => d.Image)
                .Include(d => d.IsReferencedBy)
                .Include(d => d.Keyword)
                .Include(d => d.LandingPage)
                .Include(d => d.Publisher)
                .Include(d => d.QualifiedAttribution)
                    .ThenInclude(a => a.Agent)
                .Include(d => d.QualifiedRelation)
                    .ThenInclude(r => r.Relation)
                .Include(d => d.Relation)
                .Include(d => d.TemporalCoverage)
                .AsSplitQuery();

            if (loadPersonalInformation)
            {
                query = query
                    .Include(d => d.ResponsiblePerson)
                    .Include(d => d.ResponsibleDeputy);
            }

            return query;
        }
    }

    public static IQueryable<DcatCatalog> CreateGetDcatCatalogsQuery(
        this IopDbContext iopDbContext,
        bool asNoTracking,
        EntityIncludeLevel entityIncludeLevel,
        Expression<Func<DcatCatalog, bool>>? filter = null)
    {
        filter ??= x => true;

        var query = asNoTracking
            ? iopDbContext.DcatCatalogs.AsNoTracking()
            : iopDbContext.DcatCatalogs.AsQueryable();

        query = query
            .Include(x => x.Publisher);

        return query
            .Where(filter);
    }

    public static IQueryable<DcatCatalogRecord> CreateGetDcatCatalogRecordsQuery(
        this IopDbContext iopDbContext,
        bool asNoTracking,
        EntityIncludeLevel entityIncludeLevel,
        Expression<Func<DcatCatalogRecord, bool>>? filter = null)
    {
        filter ??= x => true;

        var query = asNoTracking
            ? iopDbContext.DcatCatalogRecords.AsNoTracking()
            : iopDbContext.DcatCatalogRecords.AsQueryable();

        query = query
            .Include(x => x.PrimaryTopic)
            .Include(x => x.Themes);

        return query.Where(filter);
    }

    public static IQueryable<IopConcept> CreateGetIopConceptsQuery(
        this IopDbContext iopDbContext,
        bool asNoTracking,
        EntityIncludeLevel entityIncludeLevel,
        bool loadPersonalInformation,
        Expression<Func<IopConcept, bool>>? filter = null)
    {
        filter ??= x => true;

        var query = asNoTracking
            ? iopDbContext.IopConcepts.AsNoTracking()
            : iopDbContext.IopConcepts.AsQueryable();

        query = entityIncludeLevel switch
        {
            EntityIncludeLevel.All => buildEntityIncludeLevelAllQuery(query, loadPersonalInformation),
            _ => query.Include(c => c.Publisher)
        };

        return query.Where(filter);

        static IQueryable<IopConcept> buildEntityIncludeLevelAllQuery(
            IQueryable<IopConcept> query,
            bool userHasValidToken)
        {
            query = query
                .Include(c => c.ConformsTo)
                .Include(c => c.Replaces)
                .Include(c => c.Keywords)
                .Include(c => c.Publisher);

            if (userHasValidToken)
            {
                query = query
                    .Include(c => c.ResponsibleDeputy)
                    .Include(c => c.ResponsiblePerson);
            }

            return query;
        }
    }

    public static IQueryable<CodeListEntry> CreateGetCodeListEntriesQuery(
        this IopDbContext iopDbContext,
        bool asNoTracking,
        EntityIncludeLevel entityIncludeLevel,
        Expression<Func<CodeListEntry, bool>>? filter = null)
    {
        filter ??= x => true;

        var query = asNoTracking
           ? iopDbContext.CodeListEntries.AsNoTracking()
           : iopDbContext.CodeListEntries.AsQueryable();

        query = entityIncludeLevel switch
        {
            EntityIncludeLevel.All => query
                .Include(c => c.Annotations.OrderBy(a => a.Position))
                .Include(c => c.ParentCodeListEntry),
            _ => query,
        };

        return query.Where(filter);
    }

    public static IQueryable<MappingTable> CreateGetMappingTablesQuery(
        this IopDbContext iopDbContext,
        bool asNoTracking,
        EntityIncludeLevel entityIncludeLevel,
        bool loadPersonalInformation,
        Expression<Func<MappingTable, bool>>? filter = null)
    {
        filter ??= x => true;

        var query = asNoTracking
            ? iopDbContext.MappingTables.AsNoTracking()
            : iopDbContext.MappingTables.AsQueryable();

        query = entityIncludeLevel switch
        {
            EntityIncludeLevel.All => buildEntityIncludeLevelAllQuery(query, loadPersonalInformation),
            _ => query.Include(c => c.Publisher)
        };

        return query.Where(filter);

        static IQueryable<MappingTable> buildEntityIncludeLevelAllQuery(
            IQueryable<MappingTable> query,
            bool userHasValidToken)
        {
            query = query
                .Include(c => c.ConformsTo)
                .Include(c => c.Keywords)
                .Include(c => c.Publisher);

            if (userHasValidToken)
            {
                query = query
                    .Include(c => c.ResponsibleDeputy)
                    .Include(c => c.ResponsiblePerson);
            }

            return query;
        }
    }

    public static IQueryable<MappingRelation> CreateGetMappingRelationsQuery(
        this IopDbContext dbContext,
        bool asNoTracking,
        Expression<Func<MappingRelation, bool>>? filter = null)
    {
        filter ??= x => true;

        var query = asNoTracking
           ? dbContext.MappingRelations.AsNoTracking()
           : dbContext.MappingRelations.AsQueryable();

        return query.Where(filter);
    }

    public static IQueryable<PublicService> CreateGetPublicServicesQuery(
        this IopDbContext dbContext,
        bool asNoTracking,
        EntityIncludeLevel entityIncludeLevel,
        bool loadPersonalInformation,
        Expression<Func<PublicService, bool>>? filter = null)
    {
        filter ??= x => true;

        var query = asNoTracking
           ? dbContext.PublicServices.AsNoTracking()
           : dbContext.PublicServices.AsQueryable();

        query = entityIncludeLevel switch
        {
            EntityIncludeLevel.All => buildEntityIncludeLevelAllQuery(query, loadPersonalInformation),
            _ => query.Include(p => p.Publisher)
        };

        return query
            .Where(filter)
            .OrderBy(x => x.Id);

        static IQueryable<PublicService> buildEntityIncludeLevelAllQuery(IQueryable<PublicService> query, bool loadPersonalInformation)
        {
            query = query
                .Include(d => d.Channels)
                    .ThenInclude(c => c.OwnedBy)
                        .ThenInclude(o => o.OwnedBy)
                .Include(d => d.IsDescribedAt)
                    .ThenInclude(i => i.IsDescribedAt)
                .Include(d => d.Keyword)
                .Include(d => d.Publisher)
                .Include(d => d.Relation)
                .Include(d => d.Requires)
                    .ThenInclude(r => r.Requires)
                .Include(d => d.Relation)
                    .ThenInclude(r => r.Relation)
                .AsSplitQuery();

            if (loadPersonalInformation)
            {
                query = query
                    .Include(d => d.ResponsiblePerson)
                    .Include(d => d.ResponsibleDeputy);
            }

            return query;
        }
    }
}
