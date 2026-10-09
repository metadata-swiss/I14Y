# How search works, and how the new engine differs from the old one

**Audience:** business users testing the new search. No technical background assumed.

**What changed:** search used to run *inside* the I14Y backend using a library called Lucene. It now
runs in a *separate service* called IndexSearch, built on Elasticsearch. What you search for and what
you see should feel the same. Where it does not, this document tells you whether that is expected.

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
Each match gets a relevance score. The score is then adjusted by the registration status of the
resource, so better-established resources rise:

| Registration status | Effect on score |
|---|---|
| Preferred standard | +10% |
| Standard | +5% |
| Qualified | +2% |
| Recorded | no change |
| Candidate | −2% |
| Incomplete | −5% |
| Superseded | −10% |
| Retired | −15% |

These numbers are **unchanged** from the old search.

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
- **Registration status weighting** — the table above.
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

### 3.5 The ordering of results will not match exactly

Two reasons:

1. The underlying relevance formula is a newer, better-regarded one; the old engine used a formula
   from 2013. Even for identical matches, the numbers differ.
2. **The old engine gave extra weight to matches in the title (double), in keywords (× 1.75) and in
   the description (× 1.5). The new engine currently weights all fields equally.**

Point 2 is the one you are most likely to notice. A resource whose *description* mentions your word
may now rank above one whose *title* is your word.

**Please test this specifically and report examples.** Whether to restore title weighting is an open
decision, and concrete examples of bad ordering are exactly what is needed to make it. Report them
as: *"searched X, expected Y first, got Z first"*.

### 3.6 Page size and very deep paging

- A single page is capped at **200 results**. Asking for more returns 200.
- You can page through up to **100 000 results**; beyond that, narrow the search with filters.
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
