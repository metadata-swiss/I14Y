# How search works, and how the new engine differs from the old one

**Audience:** business users testing the new search. No technical background assumed.

**What changed:** search used to run *inside* the I14Y backend using a library called Lucene. It now
runs in a *separate service* called IndexSearch, built on Elasticsearch. What you search for and what
you see should feel the same. Where it does not, this document tells you whether that is expected.

The numbers that decide the ranking are settings, not code. Section 7 explains each one and how to
change it to test a different behaviour. Section 8 explains how code lists are stored and kept up to
date.

---

## 1. How a search works, step by step

When you type into the search box, four things happen in order.

**Step 1 — your words are broken up and simplified.**
The text is split into words, lowercased, and accents are removed, so `Gebäude`, `gebaeude` and
`GEBÄUDE` are all treated the same. Very common words (`der`, `the`, `le`, `il`…) are dropped,
because they match everything and mean nothing.

**Step 2 — your words are matched against the stored text.**
Every resource in the catalogue has its title, name, description, keywords, identifier, version,
data owner, responsible person and contact details stored in the index, in each of the five
languages. Your words are compared against all of them.

Matching happens two ways at once:

- **Whole-word match** — your word matches a word in the text. This scores highest.
- **Partial match** — your word matches *part* of a word. This is how `wetter` finds
  `Wetterdaten`. Partial matches count for less (75% of a whole-word match), so a resource that
  genuinely contains your word ranks above one that merely contains it as a fragment.

**Step 3 — results are scored and ordered.**
Each match gets a relevance score, and results are ordered by four things in turn:

1. **How well the text matched.** What a resource is called counts far more than what it says about
   itself, so an exact title beats a description that merely repeats your word.
2. **Registration status**, for resources that matched equally well — better-established resources
   rise, in this order: preferred standard, standard, qualified, recorded, candidate, incomplete,
   superseded, retired. A resource indexed before this field existed ranks as *recorded*.
3. **Version, newest first**, for resources that matched equally well *and* share a registration
   status — typically the versions of one concept, such as *Kantone, Bezirke und Gemeinden*. Each part
   of the version is compared as a number, so `2024.1.0` comes before `2022.10.0`, which comes before
   `2022.9.1`. A resource without a version, or whose version has no digits, comes last.
4. **How often the resource is reused**, for resources that tie on all of the above.

**How versions are compared.** The index sorts the version as text, one character at a time, so on
its own `2022.9.1` would come before `2022.10.0` (because `9` is greater than `1`), and `9.0.0` before
`2024.1.0`. To avoid this, each resource also stores a sort key in which every part of its version is
padded with leading zeros to the same width of 10 digits:

| Version | Sort key |
|---|---|
| `2022.10.0` | `0000002022.0000000010.0000000000` |
| `2022.9.1` | `0000002022.0000000009.0000000001` |

With every part the same width, text order is the same as number order. Each part counts by its
first run of digits (`v2` counts as 2, `0-beta` as 0), and a part without digits counts as 0. A
version with no digits at all, such as `draft`, gets no sort key.

Rebuild the catalogue index after deploying to populate the version order for existing resources.
Until then, resources without it keep the order they had.

Status is applied after scoring rather than folded into it. Previously, a search with no text and
only filters had a zero score, so multiplying it could not change the order. Authorization filters
made this affect most roles even without selected facets. Text searches still produced relevance
scores for every role. Ordering after the score now makes status effective in both cases.

Because status now breaks ties rather than scaling the score, a resource that matches your words
noticeably better ranks above a better-established one that matches them less well.

**Choosing another order.** A caller of the Core search (`GET /api/Search?sort=...`) can ask for a
different order instead of relevance:

| `sort` | Results ordered by |
|---|---|
| `Relevance` (default) | the four steps above |
| `TitleAscending` / `TitleDescending` | the title in the requested language, A–Z or Z–A, ignoring case and accents |
| `ModifiedNewest` / `ModifiedOldest` | the date the resource was last modified |

Resources that tie on the chosen order (the same title, the same date) fall back to the relevance
order above. The title order is one list over the whole result: a resource with no title in the
requested language takes its place in that list by its German title, or, without one, by the first
title it has in English, French, Italian or Romansh. A resource with no title or no modification date
at all comes last.

Rebuild the catalogue index after deploying to populate sort titles for existing resources. New and
updated resources receive sort titles through incremental writes; resources without sort titles come
after those with them, in relevance order. If all matching resources lack sort titles, the whole result
falls back to relevance order.

**Step 4 — filters and counts are applied.**
Your filter selections (publisher, type, theme, status…) narrow the results. The counts shown next
to each filter option are calculated *as if that one filter group were not applied* — so you can
always see what you would get by also ticking another box in the same group, and the number never
shows zero for an option you could actually pick. This is unchanged from the old search.

---

## 2. What is exactly the same

You should see no difference in any of these. If you do, it is a bug worth reporting.

- **Changes appear in search straight away.** Create, edit, publish, unpublish or delete something,
  and the search results reflect it within seconds, without you doing anything.
- **Which fields are searched** — title, name, description, keywords, identifier, version, data
  owner, responsible person and deputy, contact point name, address, note and telephone.
- **The five languages** — German, English, French, Italian, Romansh. A catalogue search looks in
  all five at once, whichever language you have selected in the interface, exactly as before. A code
  list search uses the selected language, also as before.
- **Accent and case insensitivity** — `Zürich` = `zurich` = `ZURICH`.
- **Partial-word matching**, and the 75% penalty applied to it.
- **Registration status ordering** — the order in step 3, though it now separates results that
  matched equally well instead of adjusting the score, and it now applies to every role.
- **Filter behaviour and filter counts**, including the counting rule described in step 4.
- **Who can see what** — see section 5.
- **Email search** — typing a full email address finds resources where that address is a contact,
  responsible person or deputy, and nothing else. (This was a specific fix in the old search; it is
  carried over deliberately.)
- **Code list search weighting** — code counts most, then name, then description, then annotations,
  in exactly the same proportions as before (20 / 16 / 12 / 8).

---

## 3. What is genuinely different

These are real differences, either intended or accepted. Testing should confirm they behave as
described here, rather than treat them as defects.

### 3.1 Word endings are now understood (finds *more*)

The new engine knows that `Daten` and `Datens` are forms of the same word, and likewise for English,
French and Italian. The old engine matched only the exact form or a fragment of it.

**Expect:** slightly more results for the same query, and results that look right but do not contain
your word letter for letter. This is an improvement, not a bug.

### 3.2 Partial matching is stricter (finds *fewer junk results*)

The old engine effectively treated every word as "contains this fragment anywhere". The new engine
requires a partial match to cover at least three quarters of what you typed before it counts as a
match at all.

This is a *different* rule from the scoring penalty in step 2, which happens to use the same number.
One decides whether a partial match counts; the other decides how much it is worth once it does.

**Expect:** long or unusual search words return noticeably fewer, more relevant results. Measured
against the live catalogue, the loose approach returned **2 389 of 2 935 resources** for
`Wetterdaten` — essentially everything. That no longer happens.

### 3.3 Advanced search syntax no longer works

The old engine exposed a technical query language. These no longer do anything special — they are
now treated as ordinary text:

| What you used to be able to type | What it did | What happens now |
|---|---|---|
| `"exact phrase"` | matched those words in that order | quotes ignored, words matched separately |
| `wetter AND daten` | both words required | `AND` treated as a search word |
| `titel:wetter` | searched one field only | treated as one long word |
| `wetter~2` | tolerated up to 2 typos | `~2` treated as text |
| `[2020 TO 2024]` | range | treated as text |
| `*etterdate*` | wildcard | treated as text |

**This is worth testing deliberately**, because anyone who learned the old syntax will now get wrong
results silently rather than an error message. If any user group relies on these, flag it — it is a
decision that can be revisited.

Typing several plain words still works as before: a catalogue search returns resources matching
**any** of your words, best matches first. A code list search requires **all** of your words.

### 3.4 Code list search now tolerates typos

A code list search automatically allows one typo in short words and two in longer ones. The old
search only did this if you explicitly typed `~`.

**Expect:** `Zürch` finds `Zürich`. More forgiving, occasionally looser.

**Words with a digit are codes and are never typo-tolerant.** `42` does not find `49`, and `41.42` does
not find `41.43`. From two characters on, such a word finds the codes that start with it, and the
codes with a part that is exactly the word: `42` finds `42`, `42.0`, then `7A.42` and `37.6A.42`.
It never looks inside a code, so `42` does not find `41.42.00` or the CHOP_LV4 code `4142`, and
`9112156` does not find `29112156`; An entry whose code is exactly one of
the typed words is sorted before every other result, whatever its score.

### 3.5 The ordering of results will not match exactly

Two reasons:

1. The underlying relevance formula is a newer, better-regarded one; the old engine used a formula
   from 2013. Even for identical matches, the numbers differ.
2. **The fields are weighted differently.** The old engine gave extra weight to matches in the title
   (× 2), in keywords (× 1.75) and in the description (× 1.5). The new engine weights what a resource
   is called much more strongly, and the text around it much less:

   | Field | Old engine | New engine |
   |---|---|---|
   | Title, name | × 2 (title only) | × 5 |
   | Identifier, version | × 1 | × 5 |
   | Keywords | × 1.75 | × 1 |
   | Description | × 1.5 | × 0.3 |
   | Contact point name, address, note, telephone; data owner; responsible person and deputy | × 1 | × 0.2 |

Point 2 is the one you are most likely to notice. A resource whose *title* is your word now ranks
clearly above one whose *description* merely mentions it, more so than before.

### 3.6 Page size and very deep paging

- There is **no separate cap on page size**: a caller may ask for any `pageSize`.
- What bounds a request is the **100 000-result window**: the page number times the page size cannot go
  past 100 000. Asking for more than the window allows returns what fits (a shorter last page), and a
  page that starts past the window comes back empty. The total count stays exact either way.
- You can page through up to **100 000 results**; beyond that, narrow the search with filters.

**Why there is a window, and how Elasticsearch behaves by default**

- Elasticsearch pages with `from` + `size`. For every request, each shard has to collect and sort
  `from + size` hits and the coordinating node merges them all, so the cost (memory and time) grows with
  how deep the page is, not with how many rows come back. Page 500 at 1 000 rows sorts 500 000 hits to
  return 1 000.
- To protect the cluster, Elasticsearch has the index setting `index.max_result_window`. **The default is
  10 000**: a request where `from + size` is larger is rejected with a "Result window is too large"
  error.
- This project **raises it to 100 000** on both indexes (catalog and code lists) when they are created, and
  the code (`Paging.ToWindow`) keeps every request inside it, so a caller gets an empty page instead of an
  error. The window is a property of the index, so changing it needs a new index (or an index settings
  update), not just a code change.
- Elasticsearch's recommendation for going beyond the window is **not** to raise it further, but to read
  with a point in time plus `search_after` (a cursor: "give me the rows after this one"). That has the same
  cost at any depth. It cannot jump to page N, only continue from the previous page. This is what the
  export routes use, which is why they are not limited.
- **Exports are not affected** — "download all matching code list entries" still returns everything,
  however large, using a different mechanism built for exactly that.

---

## 4. Suggested test scenarios

Work through these against the same data on the old and the new system where you can.

| # | Test | Expected result |
|---|---|---|
| 1 | Search a word you know is in a resource title | That resource is found. Note its position — that is your evidence for 3.5 |
| 2 | Search the same word with different accents and case (`Zürich` / `zurich`) | Identical results |
| 3 | Search a partial word (`wetter` for `Wetterdaten`) | Found, but ranked below resources containing `Wetter` as a whole word |
| 4 | Search a long word that is a fragment of many titles | Noticeably fewer, more relevant results than the old system |
| 5 | Search the same term with the interface in German, then in French | Identical catalogue results |
| 6 | Search a word in a different grammatical form (`Daten` vs `Datens`) | Both find the same resources |
| 7 | Enter the full email address of a responsible person | Only their resources, no unrelated noise |
| 8 | Apply two filters from different groups | Both applied together; counts behave as before |
| 9 | Apply two options within one filter group | Either one matches; counts in *other* groups update |
| 10 | Search while logged out | Public resources only — see section 5 |
| 11 | In a code list, search a code, then a name, then a description word | Code matches rank highest, then name, then description |
| 12 | Search a code list with a deliberate typo | Still found |
| 13 | Export all entries of the largest code list | Complete export, nothing missing or truncated |
| 14 | Search using old-style syntax (`"phrase"`, `AND`, `field:value`) | Treated as plain text — see 3.3 |
| 15 | Edit the title of a resource, then search for the new title | Found within seconds |
| 16 | Delete a resource, then search for it | Gone within seconds |
| 17 | Change a code list entry, then search that code list for it | Updated within seconds, including its position in the hierarchy |

---

## 5. Who can see what

Unchanged, but the most important thing to verify, because a mistake here is a data leak rather than
an inconvenience.

| Your role | What search returns |
|---|---|
| Not logged in | Public resources only |
| Viewer, Local data steward, Submitter | Public resources, plus Internal resources of your own organisation |
| Swiss data steward | Everything |

**Test explicitly:**

1. Log out and search for something you know is Internal. It must not appear — not its title, not
   its description, not even as a number in a filter count.
2. Take a resource that is currently Public, set it to Internal, then repeat the same anonymous
   search. It must disappear within seconds.
3. Set it back to Public and confirm it returns.

If an Internal resource is ever visible to a caller who should not see it, stop and report it
immediately as a security issue.

---

## 6. How to report a difference

Include all of:

1. **What you typed**, exactly.
2. **Where** — catalogue search or code list search, and which code list.
3. **Your role**, and whether you were logged in.
4. **The interface language** you had selected.
5. **What you expected** and **what you got**. For ordering problems, name the resource you expected
   first and the one that actually came first.
6. **Whether the old system behaves differently**, if you are able to check.

Before reporting, check section 3 — it may be one of the known intended differences.

If the search settings were changed for the test (section 7), say which values were in use.

---

## 7. Changing the search settings to test scenarios

Every number in this section can be changed per environment, without a code change or a release.
That makes it possible to try a different ranking in DEV, compare the results, and only then decide
whether it should become the default.

Business decides which values to try. Someone with access to Azure App Configuration and the AKS
cluster (the development team) makes the change and restarts the search service.

### 7.1 How a change is made

**Where the settings live.** In Azure App Configuration (`bfs-appconfig-i14y-<environment>`), as one
key holding all the settings as JSON:

| Field | Value in DEV |
|---|---|
| Key | `np-iop-indexsearch-dev:SearchScoring` |
| Label | `np-iop-indexsearch-dev` |
| Content type | `application/json` |
| Value | the settings, in the format shown in 7.4 |

Other environments use the same pattern with their own name; take the prefix and label from the
existing `…:Elasticsearch:Uri` key of IndexSearch in that environment. The label and the content type
must be exactly as above. If either is wrong, the key is **silently ignored** and the defaults are
used, so a test would appear to show "no difference".

The value can hold all settings or only the ones being changed. Anything left out keeps its default.

**Making it take effect.** The settings are read when the search service starts, so after saving:

1. Restart the search service:
   ```bash
   az aks command invoke -g <resource group> -n <cluster> --command "kubectl -n indexsearch rollout restart deployment/indexsearch"
   ```
   This takes a few minutes. With the standard rolling restart, the old instance keeps answering
   until the new one is ready, so search stays available.
2. **Only if a registration status weight changed:** rebuild the search index as well, or wait for
   the nightly rebuild at 03:00 UTC. Status weights are stored with each resource when it is indexed,
   so until the rebuild only resources edited since the restart carry the new weight, and the status
   order looks inconsistent.

**If a value is invalid** (a misspelt field or status, a weight of 0 or below, a malformed
percentage), the new instance refuses to start and its log names the setting. The old instance keeps
answering (with the standard rolling restart), so nothing breaks. Fix the value and restart again.

**Going back to the defaults:** delete the key, or put the default values back, and restart. The
defaults are the values in the tables below.

### 7.2 Catalogue settings

| Setting | Default | What it does | Raise it to… | Lower it to… |
|---|---|---|---|---|
| `Catalog.FieldWeights.title` | 5 | How much a match in the title counts | make titles decide the ranking even more | let other fields catch up with titles |
| `Catalog.FieldWeights.name` | 5 | Same, for the name | | |
| `Catalog.FieldWeights.identifier` | 5 | Same, for identifiers | make pasted identifiers find their resource first | |
| `Catalog.FieldWeights.version` | 5 | Same, for the version | | |
| `Catalog.FieldWeights.keyword` | 1 | Same, for keywords | let well-tagged resources rise | |
| `Catalog.FieldWeights.description` | 0.3 | Same, for the description | let resources that explain the topic in their description rise | stop descriptions that merely mention a word from ranking high |
| `Catalog.FieldWeights.contactPointFn`, `contactPointHasAddress`, `contactPointNote`, `contactPointHasTelephone`, `dataOwner`, `responsiblePersonName`, `responsibleDeputyName` | 0.2 each | Same, for contact details, data owner and responsible people | find resources by the people or offices behind them | |
| `Catalog.PartialMatchBoost` | 0.75 | What a partial match (`wetter` in `Wetterdaten`) is worth compared to a whole word (step 2) | make fragments count almost like whole words | push fragment matches further down |
| `Catalog.PartialMatchMinimumShouldMatch` | `75%` | How much of the typed word a partial match must cover before it counts at all (3.2) | fewer, stricter partial matches | more, looser partial matches |
| `Catalog.RegistrationStatusWeights.<status>` | see below | The order of statuses among results that matched equally well (step 3) | move that status up | move that status down |

Default status weights: PreferredStandard 110, Standard 105, Qualified 102, Recorded 100,
Candidate 98, Incomplete 95, Superseded 90, Retired 85.

Two things to keep in mind:

- **Field weights work relative to each other.** Doubling every field changes almost nothing; what
  matters is, for example, that a title counts about 17 times as much as a description (5 against
  0.3).
- **Status weights only decide the order between statuses**, never whether a resource matched well.
  Only which number is higher matters, not by how much: setting Standard to 105 or to 1 000 gives the
  same result while it stays the highest but one. Two statuses with the same number rank as equals.

### 7.3 Code list settings

| Setting | Default | What it does |
|---|---|---|
| `CodeList.CodeBoost` | 20 | How much a match on the code counts (the code starts with the typed word) |
| `CodeList.NameBoost` | 16 | How much a match on the entry name counts |
| `CodeList.DescriptionBoost` | 12 | How much a match on the description counts |
| `CodeList.AnnotationBoost` | 8 | How much a match on an annotation counts |
| `CodeList.NgramMinimumShouldMatch` | `60%` | How much of the typed word a partial match on a name must cover |
| `CodeList.SingleEditMaxTermLength` | 4 | Words up to this many letters tolerate one typo, longer words two (3.4) |

As in the catalogue, the four boosts work relative to each other: by default a code match counts
20 against 16 for a name.

### 7.4 Ready-made scenarios

Each scenario lists the value to put in the key, what to search, and what to look for. Search the
same terms before and after the change, and write both orders down.

**A. Descriptions should count more.** For users who search by topic rather than by name.

```json
{ "Catalog": { "FieldWeights": { "description": 1 } } }
```

Look for: resources whose description covers the search word move up, closer to those that have it
in the title.

**B. Partial matches should count less.** For when fragments crowd out exact hits.

```json
{ "Catalog": { "PartialMatchBoost": 0.4 } }
```

Look for: searching `wetter`, resources containing the whole word `Wetter` stay on top and resources
with only `Wetterdaten` drop further down.

**C. Stricter partial matching.** For when long search words still return too many loose results.

```json
{ "Catalog": { "PartialMatchMinimumShouldMatch": "90%" } }
```

Look for: fewer results for long or unusual words. Too strict, and real matches start to disappear.

**D. Superseded resources should not rank below candidates.** Needs a rebuild after the restart.

```json
{ "Catalog": { "RegistrationStatusWeights": { "Superseded": 99 } } }
```

Look for: among results that matched equally well, superseded resources now appear between Recorded
(100) and Candidate (98).

**E. Code list names should beat codes.** For code lists that are searched by label more than by code.

```json
{ "CodeList": { "NameBoost": 25 } }
```

Look for: in a code list search, an entry whose name matches ranks above one whose code merely
starts with the search term.

The scenarios can be combined in one value, for example:

```json
{
  "Catalog": { "FieldWeights": { "description": 1 }, "PartialMatchBoost": 0.4 },
  "CodeList": { "NameBoost": 25 }
}
```

To try a different scenario, replace the value, restart, and compare again. When a scenario is
agreed, the development team makes those values the new defaults in the code, so every environment
gets them.

### 7.5 What a change applies to

- **Catalogue settings** (`Catalog…`) change only the catalogue search. **Code list settings**
  (`CodeList…`) change only code list searches.
- **There is one set of code list settings for all code lists.** All code lists are searched the
  same way (section 8), so a code list cannot be given its own boosts. Changing `CodeList.NameBoost`
  changes the ranking in every code list at once.
- **Each environment has its own values.** A change in DEV does not touch any other environment.
  Within one environment, the values apply to every user and every role.
- **Only the catalogue status weights need a rebuild** (7.1). Every other setting, including all
  code list settings, is applied when a search runs, so a restart is enough.

---

## 8. How code lists are stored and kept up to date

**One store for all code lists.** The search keeps two separate collections: one for catalogue
resources, and one for the entries of every code list together. Each entry is stored on its own,
labelled with the code list it belongs to. A code list search always looks only at the entries with
that label, so you only ever see entries of the code list you are searching, even though millions of
entries from all code lists sit side by side.

**Only the changed code list is updated.** When a code list is created, imported, edited, gets a new
version, or has entries deleted, the search is told *which* code list changed and updates only that
one, within seconds (test scenario 17). Nothing else is rewritten, however large the other code lists
are. For that one code list, the search:

1. reads all of its current entries from the database and stores them, replacing the entries it
   already had and adding new ones;
2. then removes the entries the database no longer lists.

New entries are stored *before* old ones are removed, so if something goes wrong halfway, the code
list stays searchable with its previous entries instead of disappearing. If a code list ends up with
no entries, or is deleted, all its entries are removed from the search.

Two updates of the same code list never run at the same time, and changes made while the whole
search is being rebuilt (nightly, or on request) are applied again once the rebuild finishes, so they are not lost.
