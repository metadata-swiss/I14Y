---
name: writing
description: "Drafting and revising prose that readers will see: blog posts, articles, documentation, criticism, long-form, emails, marketing, SEO copy, UI text. Not for code comments, commit messages, or private notes."
license: MIT
metadata:
  author: Anbeeld
  version: "1.3.1"
---

# Writing ruleset

## Purpose

Write for the actual context.

The goal is prose that fits the medium, the task, and the reader. If it does that well, it will usually read as human-authored as a side effect. Do not optimize for "sounding human." Do not optimize for beating detectors. Both produce worse writing.

Some rules below are prose-quality defaults. Some target common chatbot defaults. They are not the same thing.

## Precedence

When rules conflict:

1. Truth, safety, accessibility, and platform/legal requirements
2. Explicit user instructions
3. Genre and medium norms
4. Core rules
5. Optional watchlists and heuristics

If the user asks for bullets, use bullets. If accessibility, platform rules, or the medium require structure, use structure. If the user asks for a neutral summary, do not force first person or extra stance into it.

## Scope

This document is for drafting and revising text. It is not a reliable method for deciding whether existing text was written by a human or an AI, and it is not an authorship-adjudication tool. In high-stakes settings, provenance beats surface style; see `references/provenance.md`.

## Core workflow

1. Identify the medium, audience, reader need, and job of the text.
2. If it is task-oriented, identify the answer or next action that belongs first.
3. If it is long-form, decide the through-line and one concrete example, moment, or case that can carry real weight in the piece.
4. Draft to fit that context, not an abstract idea of "good writing."
5. Run the required checks for the length and stakes of the piece.
6. Cut what sounds generic, ceremonial, over-engineered, suspiciously over-specific, or too cleanly modular.

## Medium routing

- Chat, comments, replies, DMs, forum posts: running prose by default. Use lists only when the information is naturally list-like or the user asked for one. Avoid decorative formatting and canned support tone. In plain-text contexts such as chat, comments, casual Markdown, and most text typed straight into editors, prefer straight ASCII quotes and apostrophes by default. Curly quotes, curly apostrophes, single-character ellipses, and similar typesetting artifacts often read like pasted or auto-formatted text rather than native internet prose. They are fine in typeset or publication-facing prose. If text arrived by copy-paste, normalize it before sending. In the same contexts, prefer commas, colons, conjunctions, subordinate clauses, or full stops over em dashes unless the dash clearly earns its keep. Do not replace every dash with a period; if the second thought is still part of the first turn, keep the sentence moving.
- Email between colleagues: usually prose first; lists are fine for discrete items, decisions, or action points.
- Documents, specs, reports, technical writing: structure is expected. Use headings, bullets, and sequence when they help scanning and precision.
- Web pages, help centers, UI text, and public docs: put the answer or next action early. Preserve scannability and accessibility: descriptive headings, lists for steps, descriptive link text, and plain alt text when images carry information. Do not flatten useful structure just to avoid looking templated.
- Long-form posts, articles, criticism, retrospectives: use structure on purpose. Pick an angle. Do not let dates, named milestones, or neat category buckets become the spine unless the user explicitly asked for that structure.

## Safety rails

These are not AI tells by themselves: em dashes, semicolons, `however`, competent punctuation, well-formed paragraphs, and the right word even if it appears on somebody's banned list.

Do not invent typos. Do not break grammar on purpose. Do not inject slang, profanity, fake uncertainty, or staged messiness to simulate humanity. No mandatory `actually` turn. No manufactured negativity. No programmatic sentence-length wobble. This is not a preference for short sentences; natural variety comes from the relationship between thoughts, not from alternating sentence lengths by formula.

Do not make text less usable or less accessible in the name of sounding less AI-written. Removing needed headings, lists, descriptive links, citations, caveats, or next steps is not a style improvement.

The recurring problem is regularity and mismatch, not any one feature. Use em dashes where they belong; do not reach for them as a default connective. If you keep using the same punctuation move in the same role, vary it rather than banning it. In casual internet prose, paragraph-after-paragraph em dashes are now a socially recognized AI cue, so prefer commas, colons, conjunctions, subordinate clauses, or full stops unless the dash clearly earns its keep. A full stop is not the automatic replacement; sometimes the fix is to make the relationship between the clauses clearer. For temporary compound modifiers, hyphenate before the noun and usually open after it; do not let the model turn every compound into a hyphenated unit.

## Core rules

### 1. Anchor to the actual context before drafting

Decide what the text is, who it is for, what register it uses, what answer or next action the reader needs, and, in replies, what thread, person, or community it is responding to. A reply that could be pasted into any thread on the same topic will often read generic even if the prose is clean. Keep register stable across the piece.

### 2. Fit the format to the medium

Format is part of register. Over-structuring casual writing makes it feel templated. Under-structuring technical writing makes it harder to use. Match the format to the medium instead of obeying a global ban on bullets, headers, or emphasis.

### 3. Prefer concrete specificity over polished generality

Each substantial paragraph should carry at least one concrete anchor.

What counts:

- a proper noun the reader could look up
- a specific number that is not only a date or version
- a direct quote
- a named decision, moment, or thread
- a checkable detail
- in criticism, reporting, and reviews: a user-facing, reader-facing, or otherwise observed detail of what changed in practice

What does not count:

- `many`, `various`, `several`, `a lot of`
- `in ways that mattered`, `meaningful changes`, `broad implications`
- `the standard X arc`, `the usual pattern`, `as is often the case`
- vague intensifiers in place of claims: `essentially`, `fundamentally`, `ultimately`
- milestone names, dates, titles, organization names, or feature labels standing alone with no material consequence attached

If the most concrete thing in a paragraph is a name and a date, the paragraph is still probably too generic.

### 4. Specificity must be earned

When writing about real entities, milestones, people, dates, quotes, events, public metrics, planned releases, or numbers, prefer fewer verified facts to many guessed ones.

Do not use specificity theater: invented milestone names, suspiciously exact claims, synthetic quotes, or decorative factuality added only to avoid sounding generic.

Be especially careful with hidden-mechanism claims: internal logic, unseen motives, back-end behavior, or claims about what a system is "really" doing under the hood. If the reader could not observe it and you cannot verify it, do not narrate it as fact.

Do not launder analysis through vague authority. Avoid `experts say`, `observers note`, `research suggests`, or `critics argue` unless you can name the source, describe what it actually supports, and keep your claim within that support.

Treat exact quotes, close paraphrases, public metrics, future claims, and causal claims as high-fragility facts. If you write that a person said something, a metric moved, a release is planned, or one change caused another, you need source support for that exact claim. If the support is weaker, narrow the claim: `coincided with`, `appeared alongside`, `was followed by`, or cut the relationship entirely.

If you cannot verify a claim, attribute it, soften it, or cut it.

### 5. Use plain words. Allow ordinary repetition. Prefer verbs.

Do not chase synonyms for basic words like `problem`, `change`, `system`, `work`, or `people`. Repeat the ordinary word when it is the right word. Prefer `we changed it` to `the implementation of the change`, `latency dropped` to `a reduction in latency was observed`, and `applying the rule` to `the application of the rule`. Prefer actions happening to people over abstractions being observed by systems.

### 6. Cohere through reference and sentence shape

Use pronouns and continued reference when the reader can easily track them. Do not restate the full frame in every paragraph. Treat signpost openers like `Furthermore`, `Moreover`, `Additionally`, `Importantly`, and `Notably` as things to justify, not default sentence starters.

Let closely related thoughts share a sentence when the relationship is tight. Use coordination for equal-weight thoughts (`and`, `but`, `so`), subordination for unequal ones (`because`, `although`, `when`, `if`, `which`, `that`), and colons or semicolons when the second clause explains, sharpens, or turns the first. A period should mark a real pause, shift, or emphasis, not merely the place where an adjacent thought happened to arrive.

Do not turn every idea into its own sentence for crispness. Short sentences are useful when the break creates emphasis, gives the reader time, or marks a real turn; they become false crispness when neighboring sentences are only separated because the prose is afraid to keep moving. `The term works. It names the pattern.` is usually weaker than `The term works: it names the pattern.` The second version carries the relationship instead of making the reader reconstruct it.

### 7. Do not perform

Avoid keynote cadence, mission-statement phrasing, applause-line endings, and ceremonial wrap-ups. Also avoid service-desk tone: no `Great question`, `Absolutely`, or similar canned praise unless the situation clearly calls for it; no `I hope this helps`, `Feel free to reach out`, or similar canned closers unless the situation clearly calls for it. Start where the answer starts. Stop where the answer stops.

### 8. Calibrate confidence, stance, and voice to genre

Be confident where evidence is strong. Be explicit where it is weak or interpretive. If the genre normally carries a visible writer - review, opinion, comment reply, personal post - let the writer appear. If the genre normally aims at neutrality - summary, documentation, news-style reporting - do not inject first person or attitude just to make the piece feel human. If the subject naturally invites a view, do not sand everything down to evenly polite neutrality. If the subject does not require a view, do not manufacture one. For public, technical, product, or instructional writing, keep language globally legible and inclusive; avoid culturally specific jokes, ableist figures of speech, and slang unless the audience and medium genuinely call for them.

### 9. Show concrete things before generalizing

Do not open with abstract diagnosis when the reader has nothing concrete to attach it to. This is not a ban on leading with the conclusion in web, docs, email, news, or task-oriented writing; if you lead with the conclusion, make it concrete enough to be useful. Usually the order should be:

1. what happened
2. where the pattern appeared
3. what constraint mattered
4. what failed or changed
5. what that seems to mean

### 10. Watch regularity

LLM writing often becomes suspicious when its most visible feature is its own regularity.

Watch for repeated use of the same moves:

- parallel enumeration and reflexive three-part cadence inside sentences
- multiple sentences doing hidden list work even without bullets
- concession-plus-positive rhythm (`not X, but Y`; `may sound X, but Y`)
- paragraph-closing type definitions (`the kind of X where Y`)
- identical paragraph arcs
- one neat claim sentence at the top of every paragraph followed by orderly elaboration
- the same punctuation move in every paragraph
- the same controlling metaphor or contrast returning until it feels too tidy
- repeated thesis-like openings
- stacked mini-sentences for impact, especially when each sentence carries one adjacent thought that could have shared a sentence

Three-item parallel lists still count. Changing `X, Y, Z, and W` to `X, Y, and Z` does not fix the underlying shape if the sentence is still doing list work. The fix is not random variation; it is to break the repeated pattern where it starts to dominate.

One common over-correction is false crispness: splitting every clause into its own sentence to break regularity. For when to combine and when to keep the split, see rule 6.

### 11. Let the thought develop

Longer pieces should not feel pre-solved. If the prose moves in a perfectly efficient straight line from claim to conclusion, it can feel rushed. Let the thought develop through a concrete example, a noticed detail, a sentence that gathers related material, or a brief doubling-back when the material naturally allows it. A concrete example usually does this better than an artificial aside.

Development can happen inside a sentence, not only across paragraphs. A cumulative sentence can start with the main claim and then add the reason, qualification, or consequence that belongs with it. Do not split that movement into separate sentences unless the break itself does useful work.

### 12. Choose structure consciously for longer pieces

Default genre shapes are not wrong. They are only a problem when used by reflex.

For task pages, procedures, reference docs, and news briefs, the predictable structure is often the clearest one. Do not avoid it for novelty or anti-template aesthetics.

For retrospectives, criticism, feature writing, and other developmental or historical pieces, the obvious defaults are often weak:

- starting state -> changes -> verdict
- one topic bucket per paragraph
- one paragraph per named milestone

Avoid those unless the user explicitly asked for them.

Choose a through-line instead: one complaint that stopped mattering, one system that changed the rest, one shift in what people actually had to do, one mismatch between promise and reality, one constraint that suddenly started biting.

Useful alternatives:

- thematic instead of chronological
- reverse-chronological
- perspective-led
- counterfactual
- opinion-first
- single-example-led

### 13. Do not turn a piece into catalog prose or system-tour prose

If a paragraph is mainly names, milestones, categories, feature nouns, or system labels, it is probably catalog prose.

If each paragraph can be summarized with a single label such as `background`, `mechanism`, `impact`, `response`, `ending`, the piece is probably system-tour prose.

Do not give one paragraph to each milestone or one paragraph to each topic bucket unless that mapping is the actual point. Pick one change and trace its consequence. Cross-wire the piece so paragraphs depend on each other instead of sitting like labeled boxes.

### 14. Revise by reading and cutting

Re-read as a first-time reader. Cut anything that is auditioning. Cut sentences whose only job is to announce the next sentence. Collapse paragraphs that restate each other. Replace the most generic clause in the piece with something specific or delete it. Most edits should make the text shorter, but do not confuse concision with chopping: combining two tightly related sentences can be the cleaner edit when it restores the relationship between the thoughts.

## Required checks

Always read before finishing a piece. Run during revision (workflow step 5). Do not skip for short pieces; do not output the audit unless asked. See `references/required-checks.md`.

## Optional long-form diagnostics

Read only if the required checks passed but the piece still feels off — usually longer work where regularity or modular structure persists after the standard tripwires. See `references/long-form-diagnostics.md`.

## Examples of useful corrections

Read when revising a paragraph that feels generic, puffy, vague, choppy, or over-regular and you are not sure what the fix looks like. Do not read during drafting. See `references/examples.md`.

## Optional audit reference

Scan when your draft falls into repeated formula, fallback jargon, or copy-paste formatting artifacts. Do not treat the lists as bans; do not consult for every piece — only when the draft sounds like default LLM output. See `references/formula-watchlist.md`.

## Provenance in high-stakes contexts

Read only when someone's authorship claim or detection result is at stake. Do not consult for normal drafting or revision. See `references/provenance.md`.
