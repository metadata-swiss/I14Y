# iop-core gaps

mds-ui reads its data from iop-core. Some features of the piveau version need things iop-core doesn't offer yet.
For now iop-core isn't changed: each gap is worked around or the feature is hidden, and the code is marked with
`iop-core gap [G…]`. To find all places:

```bash
grep -rn "iop-core gap" app pages composables
```

## Missing in iop-core

| ID | Missing in iop-core | Needed for | Until then | Code |
|---|---|---|---|---|
| G1 | Filter by DCAT catalog in `GET /api/Search` and `GET /api/Search/count` | Showing only the datasets of the opendata.swiss catalog; the piveau filter "catalog" | Only datasets with access rights `PUBLIC` are shown. The catalog filter is gone. | `app/composables/useDatasets.ts` |
| G2 | Search and counts by the categories of the DCAT catalog records (EU data themes, vocabulary `VOCAB_EU_DATA_THEME`) | The category filter, the categories on the dataset pages, subscriptions to a category | The I14Y themes of the datasets are used instead. | `app/composables/useDatasets.ts`, `dcat-ap-ch-v2-dataset-adapter.ts` |
| G3 | Filter and counts by license in `GET /api/Search` | The piveau filter "license" | The filter is gone. | `app/composables/useDatasets.ts` |
| G4 | Filter and counts by keyword in `GET /api/Search` | The piveau filter "keywords", searching by a keyword tag | The filter is gone. | `app/composables/useDatasets.ts` |
| G5 | Sorting in `GET /api/Search` (by title, by modification date) | The sort select of the dataset search | The select is hidden; results are sorted by relevance. | `app/composables/useDatasets.ts`, `app/pages/datasets/index.vue` |
| G6 | DCAT export of a single dataset or distribution (JSON-LD, Turtle, RDF/XML) | Metadata download on the dataset page; later a `<link rel="alternate">` for search engines | The download isn't shown. | `dcat-ap-ch-v2-dataset-adapter.ts`, `dcat-ap-ch-v2-distribution-adapter.ts`, `app/pages/datasets/[datasetId]/index.vue` |
| G7 | Search over agents: text query and classification counts in `GET /api/Agents` | The organization search | All organizations are loaded (`pageSize` 1000) and filtered in mds-ui. Fine for a few hundred organizations. | `app/composables/useOrganizations.ts` |
| G8 | URI of an agent | The link to the organization's resource on the organization page | The link isn't shown. | `app/pages/organizations/[id].vue` |
| G9 | Keywords, licenses, `dct:issued` and `dct:modified` in the search results (`SearchResultModel`) | Keyword tags and the "modified on" date in the dataset list | Not shown in the list; the dataset page has them. | `dcat-ap-ch-v2-dataset-adapter.ts` (`fromSearchResult`) |
| G10 | Filter `modifiedSince` in `GET /api/Search` | Daily and weekly digest emails (S4) | The digest still uses piveau; it's migrated in phase 5. | `server/` (phase 5) |

## Not iop-core, still open

- **URLs.** Datasets are addressed by their DCAT identifier (`/datasets/{identifier}`), organizations by their
  identifier (`/organizations/{identifier}`), distributions by their iop-core id. Whether the old opendata.swiss
  URLs still work, or need redirects, depends on whether these identifiers are the ones piveau used. The Hyvor
  comment threads (`dataset-{id}`) depend on the same.
- **Showcase counts per organization** come from Nuxt Content in phase 4. Until then they show 0.
