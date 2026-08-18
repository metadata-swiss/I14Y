using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Data.EntityTypeConfigurations;
using Bfs.Iop.Core.Data.Exceptions;
using Bfs.Iop.Core.Data.Extensions;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;
using Microsoft.EntityFrameworkCore;

namespace Bfs.Iop.Core.Data;

internal sealed class IopDbContext : DbContext
{
    private readonly IUserContextService _userContextService;

    public IopDbContext(
        DbContextOptions<IopDbContext> options, 
        IUserContextService userContextService) : base(options)
    {
        _userContextService = userContextService ?? 
            throw new ArgumentNullException(nameof(userContextService));
    }

    public DbSet<Agent> Agents { get; init; }

    public DbSet<AgentSubAgentRelation> AgentSubAgentRelations { get; init; }

    public DbSet<Annotation> Annotations { get; init; }

    public DbSet<Channel> Channels { get; init; }

    public DbSet<ChannelOwnedBy> ChannelAgentRelations { get; init; }

    public DbSet<CheckSum> CheckSums { get; init; }

    public DbSet<CodeListEntry> CodeListEntries { get; init; }

    public DbSet<DataService> DataServices { get; init; }

    public DbSet<DataServiceDataset> DataServiceDatasetRelations { get; init; }

    public DbSet<Dataset> Datasets { get; init; }

    public DbSet<DatasetQualityQuestion> DatasetQualityQuestions { get; init; }

    public DbSet<DatasetQualityInformation> DatasetQualityInformation { get; init; }

    public DbSet<DatasetQualityInformationLink> DatasetQualityInformationLinks { get; init; }

    public DbSet<DcatCatalog> DcatCatalogs { get; init; }

    public DbSet<DcatCatalogRecord> DcatCatalogRecords { get; init; }

    public DbSet<Distribution> Distributions { get; init; }

    public DbSet<DistributionDataServiceRelation> DistributionDataServiceRelations { get; init; }

    public DbSet<IopConcept> IopConcepts { get; init; }

    public DbSet<IopPerson> IopPersons { get; init; }

    public DbSet<MappingTable> MappingTables { get; init; }

    public DbSet<MappingRelation> MappingRelations { get; init; }

    public DbSet<PublicService> PublicServices { get; init; }

    public DbSet<PublicServiceIsDescribedAt> PublicServiceDescribedAtDatasetRelations { get; init; }

    public DbSet<PublicServiceRelation> PublicServicesRelations { get; init; }

    public DbSet<QualifiedAttribution> QualifiedAttributions { get; init; }

    public DbSet<QualifiedRelation> QualifiedRelations { get; init; }

    public DbSet<VocabularyConfig> VocabularyConfigs { get; init; }

    public override int SaveChanges()
    {
        OnBeforeSaving();

        try
        {
            return base.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            throw DatabaseExceptionTranslator.Translate(ex);
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        OnBeforeSaving();

        try
        {
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
        catch (DbUpdateException ex)
        {
            throw DatabaseExceptionTranslator.Translate(ex);
        }
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        OnBeforeSaving();

        try
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw DatabaseExceptionTranslator.Translate(ex);
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        OnBeforeSaving();

        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw DatabaseExceptionTranslator.Translate(ex);
        }
    }

    /// <summary>
    /// Sets a <see cref="IMainEntity"/> state to Modified. 
    /// This method must be called before the <see cref="SaveChanges()"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity"></param>
    public void SetMainEntityStateToModified<T>(T entity) where T : EntityBase, IMainEntity
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        Entry(entity).State = EntityState.Modified;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("data");

        modelBuilder.ApplyConfiguration(new AgentEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new AgentSubAgentRelationEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new AnnotationEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new ChannelEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new ChannelOwnedByEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new CodeListEntryEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new CheckSumTypeConfiguration());

        modelBuilder.ApplyConfiguration(new DatasetEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new DatasetQualityAnswerOptionEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new DatasetQualityInformationEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new DatasetQualityInformationLinkEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new DatasetQualityQuestionEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new DataServiceEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new DataServiceDatasetEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new DcatCatalogEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new DcatCatalogRecordEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new DcatCatalogResourceEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new DcatCatalogThemeEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new DistributionEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new DistributionDataServiceRelationEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new IopConceptEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new IopPersonEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new KeywordTypeConfiguration());

        modelBuilder.ApplyConfiguration(new MappingRelationEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new MappingTableEntityConfiguration());

        modelBuilder.ApplyConfiguration(new PeriodOfTimeEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new PublicServiceEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new PublicServiceIsDescribedAtEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new PublicServiceRelationEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new PublicServiceRequiresEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new QualifiedAttributionEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new QualifiedRelationEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new ResourceEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new VCardEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new VocabularyConfigEntityTypeConfiguration());

        modelBuilder.ToSnakeCaseModel();
    }

    private void FindAndDeleteOrphanEntities()
    {
        foreach (var entry in ChangeTracker.Entries().Where(x => x.State == EntityState.Modified && x.Entity is not IMainEntity))
        {
            var modifiedForeignKeys = entry.Properties.Where(p => p.IsModified && p.Metadata.IsForeignKey());
            var allForeignKeys = entry.Properties.Where(x => x.Metadata.IsForeignKey());

            if (modifiedForeignKeys.Any() && allForeignKeys.All(p => p.CurrentValue is null))
            {
                entry.State = EntityState.Deleted;
            }
        }
    }

    private CreationType GetEntityCreationType()
    {
        var claimValue = _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.I14YClientTypeClaimType);

        return claimValue is IopClaimsHelper.ClaimValues.I14YClientTypeClaimTechnicalValue
            ? CreationType.Automated
            : CreationType.Manual;
    }

    private void OnBeforeSaving()
    {
        FindAndDeleteOrphanEntities();

        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not EntityBase entity)
            {
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    entity.CreatedAt = now;
                    entity.CreationType = GetEntityCreationType();
                    break;

                case EntityState.Modified:
                    entity.ModifiedAt = now;
                    break;

                default: break;
            }
        }
    }
}
