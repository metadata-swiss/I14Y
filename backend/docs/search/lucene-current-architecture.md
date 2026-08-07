# Current search architecture (Lucene.NET) — a .NET dev's overview

This document explains how I14Y catalog search works **today**, so the
[Elasticsearch overview](elasticsearch-overview.md) has something concrete to map onto.

## TL;DR

- Search runs on **Lucene.NET 4.8.0-beta00017** — an embedded, in-process library (no server).
- All of it lives in one project: **`src/Core/Bfs.Iop.Core.Lucene`**.
- There are **two independent indexes**:
  - **Catalog** — the cross-entity search (datasets, data services, concepts, mapping tables, public services).
  - **CodeListEntry** — searching the values inside a single concept's code list. *(Also ported to ES — see the ES overview's CodeList section.)*
- Every analyzer, query, facet and ranking rule is **hand-written C#**. There is no ops tooling.

## Where it plugs in

```
Bfs.Iop.Core.Api/Startup.cs  ── services.AddLuceneSearch();
                                    │
Bfs.Iop.Core.Lucene/ServiceCollectionExtensions.cs
   ├─ ICatalogIndexService        → CatalogIndexService        (singleton, holds the open index)
   ├─ ICodeListEntrySearchService → CodeListEntrySearchService (scoped)
   ├─ IIndexBuilderService        → CatalogIndexBuilderService (scoped)
   └─ LuceneHostedService         (IHostedService — builds the index at startup)
```

- The MediatR command handlers (`GetCatalogSearchCommandHandler`, `GetCatalogSearchCountCommandHandler`)
  depend only on **`ICatalogIndexService`** — they don't know it's Lucene. *(This is exactly why the ES
  PoC can be a drop-in: implement the same interface.)*
- Exposed via **`SearchController`**: `GET api/search`, `api/search/count`, `api/search/relations-count`.

## Storage

- On-disk `FSDirectory` by default (config `Lucene:IndexDirectory` / `Lucene:CatalogDirectory`), or an
  in-memory `RAMDirectory` when `Lucene:UseRamDirectory=true`.
- The catalog additionally maintains a **separate taxonomy index** used only for faceting.
- Every query opens a fresh `DirectoryReader` (no cached near-real-time reader).

## Indexing lifecycle

1. **Startup rebuild** — `LuceneHostedService.StartAsync` fires `Task.Run(() => builder.RebuildIndex())`
   (fire-and-forget). `CatalogIndexBuilderService` pulls each entity type in **batches of 100** from the
   domain services (`IDatasetsService`, `IDataServicesService`, `IPublicServicesService`,
   `IIopConceptsService`, `IMappingTablesService`) and calls `ICatalogIndexService.UpdateIndex(...)`.
2. **Live upserts** — on entity CRUD, the domain services call `UpdateIndex(...)` / `DeIndex(...)`
   directly. Upsert is keyed on the `Id` term (`writer.UpdateDocument(new Term(Id, guid), doc)`).

There is **no scheduled reindex** — only the startup build plus incremental upserts.

## Analyzers

- Catalog uses a `PerFieldAnalyzerWrapper`: a `KeywordAnalyzer` (exact/unanalyzed) as the default, and a
  **`LanguageDependentAnalyzer`** bound to each `{field}_{lang}` multilingual field.
- `LanguageDependentAnalyzer` pipeline: `StandardTokenizer → StandardFilter → elision (en/fr) →
  lowercase → language stop-words (de/en/fr/it) → German normalization → ASCIIFolding`, plus an optional
  **n-gram (2–3)** filter for partial-match fields.
- Five languages: **de, en, fr, it, rm** (Romansh has no stop-word set).

## Ranking rules (the important part)

These are the business rules the ES PoC must reproduce:

| Rule | How it's done in Lucene | Where |
|---|---|---|
| **Field weights** | Index-time boosts: **Title/Name 2.0, Keyword 1.75, Description 1.5, Identifier 1.5**, everything else 1.0 | `CatalogIndexService.FillDocumentWith*`, `BuildDocument` |
| **Partial match < exact** | Each term becomes `(exact) OR (*term* wildcard)`; the wildcard boost is scaled by **0.75** | `PartialTermMultiFieldQueryParser`, `BuildFreeTextQuery` |
| **Registration-status ranking** | Score × (weight/100), weight = PreferredStandard 110, Standard 105, Qualified 102, Recorded 100, Candidate 98, Incomplete 95, Superseded 90, Retired 85. **Only when there is a text query.** | `RegistrationStatusBoostQuery` (a `CustomScoreQuery`) + `NumericDocValuesField` |
| **Email queries** | A query that is a single email address matches the exact keyword email fields only (avoids tokenisation noise — bug #725) | `TryBuildExactEmailQuery` |
| **Authorization** | Non-privileged users see only `PublicationLevel=Public` OR their own agencies (`InteroperabilityService`/`SwissDataSteward` bypass) | `TryGetUsersAuthorizationFilter` (a `BooleanFilter`) |
| **Sort** | Pure relevance score, descending. No secondary sort. Browse (no text) = index order. | `TopScoreDocCollector` |

Notes:
- **"More-reused concepts at the top" does not exist today.** Reuse counts are computed by
  `RelationsCountService` but only for display badges — they never feed the score. Adding reuse-count
  boosting would be a **new** rule (easy to express as an ES `function_score`, see the ES doc).
- Registration status is *also* an indexed numeric field + facet used for **filtering** (separate from
  the ranking multiplier above).

## Faceting

- Facet counts come from the separate **taxonomy index** via Lucene **`DrillSideways`**: for each filter
  category that has a selection, counts are computed as if that category's own filter were removed (OR
  within a category, AND across categories). This keeps every filter dropdown populated.
- The ES equivalent is `aggregations` + `post_filter` (see the ES doc).

## Key files

- `Index/CatalogIndexService.cs` — the heart: document building, boosts, query construction, status
  weight, auth filter, faceting.
- `Search/RegistrationStatusBoostQuery.cs` — the status score multiplier.
- `Index/Parsers/PartialTermMultiFieldQueryParser.cs` — exact-vs-partial (0.75) handling.
- `Index/Analyzers/LanguageDependentAnalyzer.cs` — per-language analysis + n-gram.
- `LuceneFields.cs` — all field-name constants.
- `IndexBuilders/CatalogIndexBuilderService.cs` + `LuceneHostedService.cs` — the (re)build pipeline.
