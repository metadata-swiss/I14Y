# mds-ui work plan

Short English version of [NextSteps.md](NextSteps.md), which has the details (in Chinese). Status: 2026-10-08.

- Five versions, one theme each. Each feature is built once, with no interim solutions.
- Nothing run by the original team (Zazuko) is used, and we have none of their accounts.
- Content stays out of the I14Y main repository. The license deny list in `quality-gates.yml` applies.

Decision numbers refer to the decision table in NextSteps.md.

## Done

- Runs locally (`e221ba0`).
- iop-core client generated with orval (`e938f84`).
- Pages read datasets, organizations and categories from iop-core; Nuxt 4 folder structure (`b13cde2`).
- Comments switched off, Hyvor Talk removed (`b2f2b8f`, `33c04f4`).

## V1: public display on AKS

1. Remove everything that isn't public display: login, GOGD admin pages, email subscriptions, showcase submission,
   the piveau and Listmonk server code, and their tests and configuration.
2. Fix the remaining security, performance and SEO issues: plain Markdown for external text, data preview removed,
   content queries in `useAsyncData`, 404 for missing pages, one `<main>`, correct `aria-label`s.
3. Showcases: list from Nuxt Content, names from iop-core; migrate piveau URIs to iop-core identifiers in the
   content repository.
4. URLs: identifier or GUID; redirects from the old piveau URLs.
5. Remove Decap; write a guide for editors who change content directly on GitHub.
6. Remove piveau completely, and clean up dead code, unused dependencies and hard-coded texts.
7. Unit tests with vitest and a CI workflow for mds-ui: lint, typecheck, test, build.
8. Deploy to AKS: rewrite `k8s/` with an overlay per environment, push the image to ACR, add a deployment workflow
   that also runs when content is merged, and allow mds-ui in iop-core's CORS settings.
9. Test and go live. Done when:
   - all pages work in four languages, with no server or console errors;
   - missing pages return 404;
   - CI passes;
   - the code has no `piveau`, `zazuko`, `keycloak`, `listmonk` or `hyvor`;
   - mds-ui runs without any secret.

Decisions needed first: URLs (2), data preview (3), showing all public datasets (4), AKS details (7), access to the
content repository (9), editing on GitHub until V4 (12).

## V2: iop-core endpoints and new UI

1. Add the missing iop-core endpoints G1–G10 ([iop-core-gaps.md](iop-core-gaps.md)), ideally as part of the new
   search service the backend team is building.
2. Rebuild the public pages in the new UI design; bring back filters, sorting and metadata download.
3. SEO and GEO: descriptions, Open Graph, `hreflang`, `canonical`, sitemap, schema.org `Dataset`, caching.

Decision needed first: the new UI design (11).

## V3: login and admin area

The base for V4 and V5. admin-ui keeps running unchanged.

1. eIAM login like admin-ui (`oidc-client-ts`, code flow with PKCE); user and permissions from iop-core. Register
   the mds-ui redirect URLs at eIAM early.
2. The frame of the admin area: navigation, layout, a "no permission" page, features shown by permission. Pages
   rendered in the browser only.

Details: G13 in [iop-core-gaps.md](iop-core-gaps.md).

## V4: interactive features and CMS

1. iop-core Portal module with email sending (SMTP relay or Azure Communication Services; no Listmonk).
2. Email subscriptions: the public through email links, staff through their eIAM account, one subscriber table;
   digests use G10.
3. Newsletter, written and sent from the admin area.
4. CMS and showcase submissions; options compared in [CmsComparison.md](CmsComparison.md).
5. Comments; options compared in [HyvorCommentsNote.md](HyvorCommentsNote.md#replacing-hyvor-talk).

Decisions needed first: CMS (1), comments (8), email sending and storage (10).

## V5: admin-ui features in mds-ui

admin-ui and the Admin API stay until this version is done.

1. Catalog overview with search and filters.
2. Rebuild the resource types one by one: data services, mapping tables, concepts, public services, datasets.
3. GOGD admin menu: dashboard, quality and metrics, categories.
4. Switch over: point the input portal and the header login button to mds-ui, delete admin-ui; the backend team
   removes the Admin API afterwards.

Details: G13 in [iop-core-gaps.md](iop-core-gaps.md).

## Before merging into main

- Keep or revert the `StoreSwaggerJson` change in the backend client generator (6).
