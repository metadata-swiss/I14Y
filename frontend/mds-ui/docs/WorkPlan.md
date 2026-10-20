# mds-ui work plan

Short English version of [NextSteps.md](NextSteps.md), which has the details (in Chinese). Status: 2026-10-20.

- Six versions, one theme each. Each feature is built once, with no interim solutions.
- V1 to V5 are the commitment. V6 is a bonus, done only with the time left once V1 to V5 are finished.
- Nothing run by the original team (Zazuko) is used, and we have none of their accounts.
- Content stays out of the I14Y main repository. From V4, showcases are in iop-core; pages, the handbook and the blog
  stay in the content repository. The license deny list in `quality-gates.yml` applies.

Decision numbers refer to the decision table in NextSteps.md.

## Done

- Runs locally (`e221ba0`).
- iop-core client generated with orval (`e938f84`).
- Pages read datasets, organizations and categories from iop-core; Nuxt 4 folder structure (`b13cde2`).
- Comments switched off, Hyvor Talk removed (`b2f2b8f`, `33c04f4`).
- Everything that isn't public display removed: login, GOGD admin pages, email subscriptions, showcase submission,
  the piveau and Listmonk server code, their tests and configuration (`8c1e23d`, 2026-10-14).
- piveau removed from the code and from Decap; showcases read from Nuxt Content; unused components and dependencies
  removed (2026-10-16).
- Running on AKS in dev: `https://mds.i14y.d.c.bfs.admin.ch` (2026-10-20).

## V1: public display on AKS

1. Remove everything that isn't public display. Done (`8c1e23d`). Left: the documentation still names the original
   team's piveau address and PR previews. The screenshots of the showcase submission form for
   [ShowcaseSubmission.md](ShowcaseSubmission.md) weren't taken; the form can be run again from the commit before
   `8c1e23d`. Its translations are kept for V4.
2. Security, performance and SEO issues. Done: the data preview kept, its service configurable and its libraries
   loaded only where needed; content queries in `useAsyncData`; 404 for missing pages; one `<main>`; no wrong
   `aria-label`s. Left: plain Markdown for external text.
3. Showcases: list from Nuxt Content, names from iop-core, all through one composable, so V4 only changes where it
   reads from. Done. The counts on the organization pages and the migration of piveau URIs wait for V4.
4. URLs use the DCAT identifier, like public-ui; no redirects from the old piveau URLs (decision 2, 2026-10-16). Left:
   what the removed pages show (5).
5. Decap: its piveau parts are removed; Decap itself stays until the showcase backend of V2 exists, then it is
   deleted. The editor guide in `docs/cms.md` is not rewritten for now.
6. Remove piveau completely and clean up. Mostly done; left: a search for leftover references, hard-coded texts,
   placeholder content.
7. Unit tests with vitest and a CI workflow for mds-ui: lint, typecheck, test, build.
8. Deploy to AKS. Dev runs in `aks-i14y-search-dev`, namespace `bfs-np-mds-ui-dev`, image `0.1.1` from ACR
   `i14ycontainers`; `k8s/` is a base and a dev overlay.
   - Access: the Application Gateway holds the domain and the wildcard TLS certificate and forwards to an internal
     load balancer (`10.71.0.10:80`) in front of the pods. The cluster needs no ingress controller.
   - The browser reaches iop-core through the mds-ui server at `/input-backend/`, so iop-core needs no CORS rule.
     Only the GET endpoints listed in `api-client/input-backend.paths.json` are forwarded, without credentials or
     cookies; they are documented at `/input-backend/docs`.
   - Left: a deployment workflow (GitHub Actions with OIDC) that also runs when content is merged and once a day;
     overlays and gateway settings for int and prod; a health probe on `/api/healthz` in the gateway.
9. Test and go live. Done when:
   - all pages work in four languages, with no server or console errors;
   - missing pages return 404;
   - CI passes;
   - nothing depends on piveau or other backend services and accounts of the original team; the code has no
     `zazuko`, `keycloak`, `listmonk` or `hyvor`, and `piveau` appears only as the name of the preview component
     `piveau-preview-plugin`;
   - mds-ui runs without any secret.

Decisions needed first: showing all public datasets (4), the removed pages (5), int and prod details (7), access to
the content repository (9), editing on GitHub until V4 (12). URLs (2) are decided; the dev details of (7) are known.

## V2: iop-core endpoints and new UI

1. Add the missing iop-core endpoints G1–G10 ([iop-core-gaps.md](iop-core-gaps.md)), ideally as part of the new
   search service the backend team is building. Add the data preview endpoint
   ([G15](iop-core-gaps.md#g15-data-preview)); mds-ui then only sets `NUXT_PUBLIC_DATA_PREVIEW_URL`. Build the
   backend of showcases ([G14](iop-core-gaps.md#g14-showcases-in-iop-core)): entity, endpoints, permissions and
   publication workflow, like the other resources (moved from V4 on 2026-10-16).
2. Rebuild the public pages in the new UI design; bring back filters, sorting and metadata download. The design also
   covers the showcase submission form, built in V4 ([ShowcaseSubmission.md](ShowcaseSubmission.md)).
3. SEO: descriptions, Open Graph, `hreflang`, `canonical`, sitemap, schema.org `Dataset`, caching. The structured
   data is also the base for GEO in V6.

Decision needed first: the new UI design (11).

## V3: login, admin area and public-ui features

The base for V4 and V5. admin-ui keeps running unchanged.

1. eIAM login like admin-ui (`oidc-client-ts`, code flow with PKCE); user and permissions from iop-core. Register
   the mds-ui redirect URLs at eIAM early. `/input-backend/` doesn't forward the token: either it gets a second list
   of endpoints that are called with a token, or iop-core gets a CORS rule for mds-ui; to be decided then.
2. The frame of the admin area: navigation, layout, a "no permission" page, features shown by permission. Pages
   rendered in the browser only.
3. The features of public-ui (i14y.admin.ch) in mds-ui, added on 2026-10-16: catalog search over all resource
   types, concepts and code lists, data services, public services, mapping tables, dataset structure and quality,
   metasearch. The list with sizes is in NextSteps.md (V3 step 2); which features move, and whether mds-ui
   replaces public-ui, is still open (16). iop-core has no endpoints yet for metasearch and RDF links.

Details: G13 in [iop-core-gaps.md](iop-core-gaps.md).

## V4: interactive features and CMS

1. iop-core Portal module with email sending (SMTP relay or Azure Communication Services; no Listmonk). Its write
   endpoints are called through `/input-backend/` with `POST` and `PUT` allowed, or with a CORS rule on iop-core;
   decided together with the token in V3.
2. Email subscriptions: the public through email links, staff through their eIAM account, one subscriber table;
   digests use G10.
3. Newsletter, written and sent from the admin area.
4. Content, as decided on 2026-10-13 ([CmsComparison.md](CmsComparison.md#decision-2026-10-13)). Every editor logs in
   with eIAM.
   - First, two prototypes: Nuxt UI inside mds-ui without affecting the public pages; Nuxt Studio with eIAM and our
     content repository.
   - Showcases move into iop-core
     ([G14](iop-core-gaps.md#g14-showcases-in-iop-core)), whose backend exists since V2: editor and review queue in
     the admin area, public submissions with a captcha, public pages read from iop-core, existing showcases moved over. The
     submission form follows [ShowcaseSubmission.md](ShowcaseSubmission.md) and fixes the problems listed there.
   - Pages, the handbook and the blog stay in the content repository, edited with Nuxt Studio.
5. Comments; options compared in [HyvorCommentsNote.md](HyvorCommentsNote.md#replacing-hyvor-talk).

Decisions needed first: comments (8), email sending and storage (10). The content decision (1) is taken; its smaller
open points are listed in NextSteps.md.

## V5: admin-ui features in mds-ui

admin-ui and the Admin API stay until this version is done.

1. Catalog overview with search and filters.
2. Rebuild the resource types one by one: data services, mapping tables, concepts, public services, datasets.
3. GOGD admin menu: dashboard, quality and metrics, categories.
4. Switch over: point the input portal and the header login button to mds-ui, delete admin-ui; the backend team
   removes the Admin API afterwards.

Details: G13 in [iop-core-gaps.md](iop-core-gaps.md).

## V6 (bonus): GEO, AI assistants and recommendations

Only if time is left after V1 to V5; not part of the committed estimate. The steps are done in order, and the work
stops wherever the time runs out; each finished step can go live on its own.

**GEO** doesn't depend on V3 to V5 and can come earlier if needed. It can't guarantee that AI assistants cite us; it
makes our data the easiest to find, understand and trust.

1. GEO, part one: an ordinary extension of SEO, on the website only.
   - More structured data: catalog, organizations, breadcrumbs, blog articles, showcases with the datasets they use,
     FAQ.
   - Citable content: a short factual summary at the top of each dataset page, the handbook as questions and answers,
     the last update date on every page.
   - Check that Google's structured data test and Google Dataset Search recognize our datasets.
2. GEO, part two: push our data and showcases to the places AI assistants rely on.
   - Notify search engines of new and changed pages with IndexNow; submit sitemaps in Google Search Console and Bing
     Webmaster Tools.
   - Make sure data.europa.eu harvests the opendata.swiss catalog through DCAT-AP.
   - Create or complete Wikidata items for major dataset series, publishers and showcases, linking back to us.
   - `llms.txt` in four languages; the public read-only API with its OpenAPI description; a DCAT export of the whole
     catalog including showcases. The MCP server is not part of this plan: another team already has one; if it is
     public, `llms.txt` points to it.
   - A policy for AI crawlers in `robots.txt`.
   - Monitoring: visits from AI assistants in Matomo, and quarterly checks of how assistants answer typical questions.

**AI assistants** call a model only through iop-core; no model key is in mds-ui or the browser. A model never publishes
anything by itself.

3. AI gateway and metadata assistant.
   - AI gateway, a new iop-core module: one place to call the chosen model service, versioned prompt templates,
     structured JSON output checked by the backend, no personal data in logs, rate and monthly cost limits, a test set
     per feature.
   - Metadata assistant in the dataset editor of V5: suggests keywords in four languages, themes from the vocabularies
     only, a short summary and translations; lists what the metadata quality metrics find missing. Each suggestion is
     accepted, edited or rejected by a person; the acceptance rate is recorded.
4. Natural language search: a sentence such as "rents in Bern since 2010" is turned into keywords, themes,
   organizations, a time range and formats; Elasticsearch runs the search, with an optional vector query fused in
   iop-core (free features only); the recognized conditions are shown as filters the visitor can change. Short keyword
   queries skip the model; on errors the normal search is used. The model never writes answers.

**Recommendations** need the subscriptions, newsletter and showcases of V4.

5. Recommendations and newsletter recommendations by closeness in the relationship graph of datasets, concepts and
   code lists, organizations, themes and showcases. Shared code lists weigh most; shared keywords weigh least. Every
   recommendation says why it was made.
   - Backend: a nightly job builds the graph and stores the top related items per dataset; endpoints for related
     items and for recommendations from a set of datasets, themes or organizations.
   - Website: related datasets on dataset and showcase pages; "based on what you viewed recently", with the history
     kept only in the visitor's browser.
   - Emails: a "you may also be interested in" section in digests and newsletters, based only on what the subscriber
     subscribed to.
   - Evaluation with click-through rates; a data protection review.
6. Personalized ranking: reorder, never hide. Final score = relevance × (1 + α × interest), with the interest taken
   from step 5. The website renders neutral results on the server (SEO and caching are unaffected) and reorders the
   top results in the browser from the local viewing history, with a switch to turn it off. Emails list items in the
   order of the subscriber's interests.

**Possible additional features**, not scheduled, each to be assessed when needed: a translation assistant in the
CMS; matching the columns of a data structure to concepts and code lists; drafting showcase texts from the answers
of submitters; a public question-answering assistant (possibly unnecessary given GEO and the existing MCP server); a
public metadata quality grade; weekly broken-link reports to publishers; saved searches with alerts; data requests
from the public; feedback on data errors; data preview and simple charts; usage statistics for publishers; embeddable
widgets; Romansh as a fifth language.

Decisions needed first: AI crawler policy and Wikidata (13), before step 2; model service, data protection, labelling
of AI content and cost limits (15), before step 3; scope and data protection of recommendations and personalized
ranking (14), before step 5.

## Before merging into main

- Keep or revert the `StoreSwaggerJson` change in the backend client generator (6).
