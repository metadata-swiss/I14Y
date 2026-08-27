namespace Bfs.Iop.Search.Elasticsearch;

/// <summary>
/// Connection + index settings for the Elasticsearch search PoC.
/// Bound from the "Elasticsearch" configuration section.
/// </summary>
public sealed class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    /// <summary>
    /// Base URL of the Elasticsearch node. Supplied by configuration ("Elasticsearch:Uri"): locally via
    /// appsettings.Development.json (http://localhost:9200); on Azure it is the "#{ELASTICSEARCH_URI}#"
    /// placeholder in appsettings.json, overridden by App Configuration (on DEV the key
    /// "ca-iop-core-dev:Elasticsearch:Uri"). Only read when the Elasticsearch engine is active.
    /// </summary>
    public string Uri { get; set; } = "";

    /// <summary>Name of the catalog index.</summary>
    public string CatalogIndexName { get; set; } = "catalog";

    /// <summary>Name of the codelist-entry index.</summary>
    public string CodeListIndexName { get; set; } = "codelist";

    /// <summary>Optional basic-auth user (leave empty for the local security-disabled dev node).</summary>
    public string? Username { get; set; }

    /// <summary>Optional basic-auth password.</summary>
    public string? Password { get; set; }

    /// <summary>
    /// Drop and recreate both indexes at startup, before building them. Defaults to <c>true</c> in
    /// every environment.
    /// <para>
    /// <b>Why a fresh index rather than building over the existing one.</b> A full build is
    /// upsert-only — it bulk-indexes every row it reads from Postgres and deletes nothing. Two things
    /// therefore never heal on their own:
    /// </para>
    /// <list type="bullet">
    /// <item><description>
    /// A resource deleted in Postgres whose de-index event was lost stays searchable forever. The
    /// forward queue is an in-memory channel that drops oldest and does not survive a restart, so
    /// losing one is routine — and the scheduled rebuild repairs additions and edits but cannot
    /// repair a deletion.
    /// </description></item>
    /// <item><description>
    /// A mapping change never takes effect, because <c>EnsureIndexAsync</c> creates an index only
    /// when it is absent. A newly mapped field stays silently unsearchable until someone deletes the
    /// index by hand.
    /// </description></item>
    /// </list>
    /// <para>
    /// <b>The accepted cost, stated plainly.</b> The delete happens <i>before</i> the build, so from
    /// that moment until the build finishes, searches return HTTP 200 with zero results — which reads
    /// as "the data is gone", not as "it is rebuilding". On Container Apps that window opens on every
    /// restart, revision rollout and scale event. Two mitigations are worth having and are not built
    /// yet: a readiness probe that reports unready while <c>IIndexBuildState.IsBuilding</c> is true,
    /// and — the real fix — building into a new concrete index and moving an alias atomically, which
    /// would give the same freshness with no empty window at all.
    /// </para>
    /// <para>
    /// Set to <c>false</c> to build over the existing index instead. <c>POST /api/Index/rebuild</c>
    /// rebuilds on demand either way, but note that it does <b>not</b> recreate — it is upsert-only,
    /// so it will not purge stale documents.
    /// </para>
    /// </summary>
    public bool RecreateIndexOnStartup { get; set; } = true;
}
