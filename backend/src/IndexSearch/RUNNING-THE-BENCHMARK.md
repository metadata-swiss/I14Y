# Running the rebuild benchmark yourself

Measures a full Elasticsearch index rebuild against a real database. Results and the Lucene comparison
are in [REBUILD-BENCHMARK.md](REBUILD-BENCHMARK.md).

## 1. Start the backends

From the repository root:

```bash
docker compose up -d postgres elasticsearch
```

Wait for both to report healthy:

```bash
docker ps --format "table {{.Names}}\t{{.Status}}"
```

## 2. Make sure Postgres has data

```bash
docker exec i14y-postgres psql -U $POSTGRES_USER -d $POSTGRES_DB -c "select count(*) from data.code_list_entries;"
```

Zero rows means an empty volume. Restore a dump — read-only on the source, and the local copy is what
gets measured:

```bash
docker exec -e PGSSLMODE=require i14y-postgres pg_dump -h host.docker.internal -p 15432 -U <user> -d <db> --no-owner --no-privileges -Fc -f /tmp/i14y.dump
```

```bash
docker exec i14y-postgres pg_restore -U $POSTGRES_USER -d $POSTGRES_DB --no-owner --no-privileges -j 4 /tmp/i14y.dump
```

The dump needs a tunnel to the source database on port 15432. Do **not** point the application itself
at a shared server: `Core.Api` runs EF migrations unconditionally at startup and inserts sample data in
Development.

## 3. Set the connection string

The fixture only ever reads Postgres, so a restored copy is safe. It takes the local credentials from
`.env`:

```bash
export INDEXSEARCH_TEST_POSTGRES_READONLY="Host=localhost;Port=5432;Username=<user>;Password=<password>;Database=<db>"
```

`INDEXSEARCH_TEST_ELASTICSEARCH` is optional and defaults to `http://localhost:9200`. The fixture
refuses any host that is not local, because it drops its indices.

## 4. Run it

**Order matters.** The catalog test recreates *both* indices, so run it first; the code-list test fills
the index it left behind.

```bash
dotnet test src/IndexSearch/Bfs.Iop.IndexSearch.IntegrationTests -c Debug --filter "FullyQualifiedName~The_whole_catalog_is_read_mapped_and_indexed" --logger "console;verbosity=detailed"
```

```bash
dotnet test src/IndexSearch/Bfs.Iop.IndexSearch.IntegrationTests -c Debug --filter "FullyQualifiedName~The_whole_code_list_is_read_mapped_and_indexed" --logger "console;verbosity=detailed"
```

`verbosity=detailed` is required — without it the timings are written to the test output and never
shown. Each prints one line:

```
catalog: 2'935 of 2'935 documents in 00:03.3 (882/s), 0 batches failed, index 4.7 MB
code list: 490'754 of 490'754 documents in 02:57.0 (2'771/s), 0 batches failed, index 726.9 MB
```

The code-list run takes about three minutes. Watch it live from another shell:

```bash
curl -s "http://localhost:9200/_cat/indices/i14y-*-pipelinetest?h=index,docs.count,store.size&v"
```

`docs.count` reads about 7x the entry count, because each annotation is its own nested document.

## Comparing against Lucene

Lucene rebuilds its index inside `Core.Api` at startup. Clear it first, or the build is skipped:

```bash
rm -rf src/Core/Bfs.Iop.Core.Api/lucene-index
```

Then run `Core.Api` against the same local database and time the gap between these two log lines:

```
Starting Lucene index build in background...
Lucene index built successfully.
```

This only works while `Bfs.Iop.Core.Lucene` still exists in the tree.

## What the numbers do and do not mean

The **code-list figure and the total** are sound: a sustained rate over 490,754 items, and code lists
are 97% of the rebuild.

The **catalog figure is noisy**. 2,935 documents is too few to amortise fixed costs — EF query
compilation, connection setup, and on the Lucene side a full vocabulary-cache warm-up — so repeated
runs vary by a second or more. Two consecutive runs gave 3.3 s and 4.2 s, which is 882/s and 692/s for
identical work.

Absolute times are workstation numbers and do not transfer to AKS. The Lucene-versus-Elasticsearch
*ratio* does, because both read the same database on the same machine.
