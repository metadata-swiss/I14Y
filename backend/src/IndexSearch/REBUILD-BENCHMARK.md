# Index rebuild: Lucene vs Elasticsearch

Measured 2026-09-02. Recorded because the Lucene half becomes unrepeatable the moment
`Bfs.Iop.Core.Lucene` is deleted, and because "the new pipeline should be faster" was an expectation
until this was run.

## Result

Same database, same machine, same data.

Two runs of each, because the numbers move between runs.

| | Lucene | Elasticsearch | |
|---|---|---|---|
| catalog — 2,935 documents | 26 s | **3.3 s / 4.2 s** | ~6-8x faster |
| code lists — 490,754 entries | 13 m 23 s | **2 m 57 s / 2 m 26 s** | **4.5-5.5x faster** |
| **full rebuild** | **13 m 49 s** | **3 m 00 s / 2 m 30 s** | **4.6-5.5x faster** |
| code list throughput | ~610 entries/s | 2,771 / 3,356 entries/s | |
| index on disk, fully merged | **448 MB** | **547 MB** | **1.2x larger** |

Every document landed: 2,935 of 2,935 and 490,754 of 490,754 accepted, no failed batches. The counts
were read back from Elasticsearch afterwards and matched, so this is verified rather than self-reported.

## Which of these numbers to trust

**The code-list rate, and the total.** Two and a half to three minutes against 13 m 23 s is a
sustained rate over 490,754 items, and it decides the total because code lists are 97% of the rebuild.
Two runs came in 21% apart (2 m 57 s and 2 m 26 s), so treat the speedup as a range, not a point.

**Not the catalog rate.** 2,935 documents is far too few to amortise fixed costs, and both engines pay
several: Lucene warms its whole vocabulary cache once, EF compiles five multi-include queries,
connections open. Those land almost entirely on the catalog phase and are then divided by 2,935, where
the code-list phase divides its own by 167x more items. The catalog figure is mostly startup cost
wearing a rate's clothing: two runs of identical work gave 3.3 s and 4.2 s, which is 882/s and 692/s.
It is the weakest claim on this page.

That also explains the oddity that per-item throughput is ~5x *higher* for code lists than for the
catalog on both engines: catalog documents are genuinely more expensive each — ~40 fields, seven
multilingual fields across five languages, three collection includes, facet fields, and on the Lucene
side a vocabulary lookup per field plus a taxonomy writer — while a code-list entry is a code, a name,
a description and its annotations.

## Why the catalog gains the most

The Lucene path fed the index from `Core.Abstractions` models: it warmed a vocabulary cache up front,
then mapped every record through a model, resolving vocabulary entries field by field. 26 seconds for
2,935 records is 113 a second, which is slow for so few rows and was never about the search engine.
Whatever the exact multiple, the cause is that mapping layer rather than Elasticsearch.

The Elasticsearch path reads entities and copies fields. Vocabulary codes are already what the index
stores, so nothing is resolved during a rebuild — labels are looked up when a result is rendered.

## Why the index is larger, and how to measure it

Elasticsearch stores `_source`, the original JSON of every document, which Lucene did not. That is what
lets a search return a renderable hit without going back to the database.

**Measuring it is easy to get wrong.** Store size keeps moving for minutes after a rebuild while
segments flush and merge. Read immediately after indexing, the same 490,754 entries reported 727 MB on
one run and 563 MB on another, then 614 MB a minute later as more segments committed. None of those is
the index size.

The comparable figure is after a force merge to a single segment:

```bash
curl -XPOST "http://localhost:9200/<index>/_forcemerge?max_num_segments=1"
curl -s "http://localhost:9200/_cat/indices/<index>?h=index,pri.store.size&v"
```

That gives **542.5 MB** for the code list plus 4.6 MB for the catalog: **547 MB against Lucene's
448 MB, so about 1.2x**. A production index is not force-merged, so expect it to sit somewhat above
that — but the earlier "1.7x" in this document came from a mid-merge reading and was wrong.

## The shape of the data, which is the real story

| table | rows | on disk |
|---|---|---|
| `annotations` | 2,928,334 | 576 MB |
| `code_list_entries` | 490,754 | 186 MB |
| everything else combined | ~9,600 | ~8 MB |

**98% of the database is code lists and their annotations.** The entire catalog — every dataset, data
service, public service, concept and mapping table — is under 10 MB of 782 MB, and 26 seconds of a
14-minute rebuild. Any performance work on this system is code-list work.

Hierarchy: average depth 5.54, maximum 63. 413,241 entries have a parent, but only 66,436 distinct
rows are ever referenced as one.

## Document count is not an entry count

Annotations are separate documents on both engines — nested in Elasticsearch, parent/child joined in
Lucene — so the code-list index reports **3,419,087 documents for 490,754 entries**, almost exactly 7x.

This is required, not incidental. The configured annotation filters need one annotation to match
several criteria at once ("type is X and title is Y"); flattening them would match a type from one
annotation against a title from another and silently return the wrong entries.

## Caveats

- **Not a production prediction.** Both ran on a developer workstation against a local Postgres 16
  (pre-prod is 15.18), with Elasticsearch in a container limited to a 1 GB heap and no network between
  the application and either service. The *ratio* holds because both engines read the same database on
  the same box; the absolute seconds do not transfer.
- The catalog run had the dataset structure source stubbed to an empty set, because the object store is
  not part of this measurement. A real rebuild adds one object-store listing — a fixed cost, not a
  per-document one.
- Lucene was measured through `Core.Api` at startup, timed from its own log lines. Elasticsearch was
  measured through `RebuildPipelineTests`, timed around the rebuilders. Both include reading from
  Postgres, mapping, and writing to the index.

## Reproducing it

Step by step, including restoring a database and the ordering the two tests require:
[RUNNING-THE-BENCHMARK.md](RUNNING-THE-BENCHMARK.md).

Do not run the whole `Pipeline` category in one go — the catalog test recreates both indices and the
code-list search test needs a populated one, so they have to run in order.

Lucene: run `Core.Api` against the same database with an empty `lucene-index` directory and time the
gap between `Starting Lucene index build in background...` and `Lucene index built successfully.`
