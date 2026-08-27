# Bfs.Iop.Search.Abstractions

Two different kinds of contract live here. They look alike from the outside — same project, same
`Bfs.Iop.Search.Abstractions` namespace — so the folders are the only signal. Check which one you are
adding to before adding anything.

The namespace is deliberately **flat**: files sit in `Indexing/` and `Query/` but all declare
`namespace Bfs.Iop.Search.Abstractions;`. That was chosen so the folders could be introduced without
changing a single `using` anywhere in the solution. Keep it that way.

## `Indexing/` — maintaining the index

| Type | Purpose |
|---|---|
| `ICatalogIndexService` | write the catalog; also raw reads (see the wart below) |
| `ICodeListEntryIndexService` | write the code-list index |
| `IIndexBuilderService`, `IndexBuildReport` | full rebuild |
| `CatalogSearchResultEntry`, `CatalogSearchCountResultEntry` | raw index hits |
| `CatalogFacetDimensions` | facet-dimension identifiers in a count response — **not** field names |

Implemented by `Bfs.Iop.Search.Elasticsearch`, and by nothing else. The Elasticsearch document field
names are its own business and live in its internal `EsCatalogFields`; the two sets differ in casing
and spelling on purpose, so do not treat `CatalogFacetDimensions` as an index schema.

**The wart, stated rather than hidden:** `ICatalogIndexService` does two jobs. Its writes take Core
domain models; its `SearchAsync` / `SearchCountAsync` return raw index entries.

That wart no longer leaks into IOP Core. Core does not implement this interface at all — it depends
on its own narrower `ICatalogIndexWriter` / `ICodeListEntryIndexWriter` (in
`Bfs.Iop.Core/Services/Contracts`, internal), which carry writes only, and
`Bfs.Iop.Core.IndexForwarding` implements those as **enqueue-only forwarders**. Core used to
implement the full interfaces here and stub out the four members it could not honour — two throwing,
two no-ops. If you are reaching for the read half from Core, you want `ICatalogSearchQueryService`.

## `Query/` — answering a user's query

| Type | Returns |
|---|---|
| `ICatalogSearchQueryService` | `PagedResult<SearchResultModel>` |
| `ICatalogSearchCountQueryService` | `SearchCountResultModel` |
| `ICodeListEntrySearchService` | `PagedResult<CodeListEntrySearchResultEntryModel>` |

These return **finished public models** from `Bfs.Iop.Core.Abstractions.Models`, not index entries.

Each has exactly **one** implementation, backed by Elasticsearch and living inside the search service
— `Bfs.Iop.IndexSearch.Api` for the two catalog contracts, the engine itself for
`ICodeListEntrySearchService`. They are the seam between that service's controllers and its engine,
and nothing outside that process implements them.

**IOP Core does not use these.** It once did: a `RemoteSearchQueryService` in
`Bfs.Iop.Core.IndexForwarding` implemented all three by calling the search service over HTTP, so
Core's MediatR handlers could bind the ports without knowing which implementation they got. That
polymorphism was never exercised — Core has no local engine to switch to — so the wrapper was deleted
and Core's four search handlers now inject `IIndexSearchSearchClient` from
`Bfs.Iop.IndexSearch.ApiClient` directly, the same way `Bfs.Iop.Admin.Business` injects
`IIopCoreApiClient`.

Consequence worth knowing before adding a member here: a change to a `Query/` contract no longer
reaches Core. The wire contract in `Bfs.Iop.IndexSearch.ApiClient` is what Core sees, and the two are
kept in step only by `SearchQueryContractTests`, which reflects over both controllers.

## Why not split by "who implements it"

Tempting, but it does not hold. `ICodeListEntrySearchService` is a `Query/` contract that the
**engine** implements directly, because code-list search needs no extra mapping. `ICatalogSearchQueryService`
is implemented by the **host**, because turning hits into `SearchResultModel` needs Core's vocabulary
service. Purpose is the reliable axis; implementer is not.
