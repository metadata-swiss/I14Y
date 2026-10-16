# iop-core gaps

mds-ui reads its data from iop-core. Some features of the piveau version need things iop-core doesn't offer yet.
For now iop-core isn't changed: each gap is worked around or the feature is hidden, and the code is marked with
`iop-core gap [G…]`. Features that were removed from mds-ui until the backend can provide them (G11 comments,
G12 email subscriptions, G13 admin functions and login) are listed here too, with how they worked, so they can be
added back later. G13 also covers the features of admin-ui, which will be built again in mds-ui. G14 moves showcases
from the content repository into iop-core. G15 is the service behind the data preview of distributions. To find all
places in the code:

```bash
grep -rn "iop-core gap" app server
```

## Missing in iop-core

| ID | Missing in iop-core | Needed for | Until then | Code |
|---|---|---|---|---|
| G1 | Filter by DCAT catalog in `GET /api/Search` and `GET /api/Search/count` | Showing only the datasets of the opendata.swiss catalog; the piveau filter "catalog" | Only datasets with access rights `PUBLIC` are shown. The catalog filter is gone. | `app/composables/useDatasets.ts` |
| G2 | Search and counts by the categories of the DCAT catalog records (EU data themes, vocabulary `VOCAB_EU_DATA_THEME`) | The category filter, the categories on the dataset pages | The I14Y themes of the datasets are used instead. | `app/composables/useDatasets.ts`, `dcat-ap-ch-v2-dataset-adapter.ts` |
| G3 | Filter and counts by license in `GET /api/Search` | The piveau filter "license" | The filter is gone. | `app/composables/useDatasets.ts` |
| G4 | Filter and counts by keyword in `GET /api/Search` | The piveau filter "keywords", searching by a keyword tag | The filter is gone. | `app/composables/useDatasets.ts` |
| G5 | Sorting in `GET /api/Search` (by title, by modification date) | The sort select of the dataset search | The select is hidden; results are sorted by relevance. | `app/composables/useDatasets.ts`, `app/pages/datasets/index.vue` |
| G6 | DCAT export of a single dataset or distribution (JSON-LD, Turtle, RDF/XML) | Metadata download on the dataset page; later a `<link rel="alternate">` for search engines | The download isn't shown. | `dcat-ap-ch-v2-dataset-adapter.ts`, `dcat-ap-ch-v2-distribution-adapter.ts`, `app/pages/datasets/[datasetId]/index.vue` |
| G7 | Search over agents: text query and classification counts in `GET /api/Agents` | The organization search | All organizations are loaded (`pageSize` 1000) and filtered in mds-ui. Fine for a few hundred organizations. | `app/composables/useOrganizations.ts` |
| G8 | URI of an agent | The link to the organization's resource on the organization page | The link isn't shown. | `app/pages/organizations/[id].vue` |
| G9 | Keywords, licenses, `dct:issued` and `dct:modified` in the search results (`SearchResultModel`) | Keyword tags and the "modified on" date in the dataset list | Not shown in the list; the dataset page has them. | `dcat-ap-ch-v2-dataset-adapter.ts` (`fromSearchResult`) |
| G10 | Filter `modifiedSince` in `GET /api/Search` | The daily and weekly digest emails of the subscription service (G12) | Not needed while there are no email subscriptions. | — |
| G11 | A comment service run by I14Y, replacing Hyvor Talk. Hyvor Talk is a paid hosted service and not open source, so it would be a self-hosted open-source comment system (Comentario, Remark42 or Isso) or a comment module of our own in iop-core; the options are compared in [HyvorCommentsNote.md](HyvorCommentsNote.md#replacing-hyvor-talk). It needs comment threads per page, comment counts for many pages at once, moderation, and a notification to the dataset's contact point when a comment is posted. | Comments on the dataset, blog, handbook and showcase pages; the comment count in the dataset list; notifying publishers of new comments (S6) | Comments are switched off and the Hyvor Talk code is removed. The old comments are in the original project's Hyvor Talk account, which I14Y can't access, so a new service starts empty. What it did and how it was built: [HyvorCommentsNote.md](HyvorCommentsNote.md). | — |
| G12 | An email subscription service in iop-core, replacing the one in mds-ui. People subscribe to a dataset, a category or an organization with their email address and confirm it by email (mds-ui has no login any more). They get a daily or weekly digest of the datasets that changed (needs G10), and change or cancel their subscription through a signed link in each email. | Email subscriptions: subscribe (S2), subscription preferences (S3), digest emails (S4) | Email subscriptions are dropped from mds-ui (decision of 2026-10-08). How they worked: [below](#g12-how-the-email-subscriptions-worked). | — |
| G13 | Admin functions and login in mds-ui: everything admin-ui does today (admin-ui will be deleted), plus the GOGD admin menu. iop-core already has most endpoints. Missing are the registration of mds-ui at eIAM (users log in with eIAM, like in admin-ui), a role for the OGD office, and whatever the dashboard and metrics pages need. | Editing all I14Y resources, today done in admin-ui; the "GOGD Admin" menu: dashboard, quality and metrics, DCAT categories | Login and the admin menu are dropped from mds-ui V1, which is public and read-only, like public-ui (decision of 2026-10-08). admin-ui keeps running until mds-ui replaces it. Details: [below](#g13-admin-functions-and-login). | — |
| G14 | A `Showcase` entity with endpoints to read, create, update, review and publish showcases, and an anonymous endpoint for submissions from the public (decision of 2026-10-13). | Showcases edited in the mds-ui admin area with eIAM; public submissions; showcases per dataset and per organization | Until V4, showcases are Markdown files in the content repository, read by Nuxt Content, and edited on GitHub. Details: [below](#g14-showcases-in-iop-core). | — |
| G15 | A data preview endpoint for tabular distributions (CSV, TSV, XLSX, XLS, ODS) that answers in the format of the preview component mds-ui already uses (decision of 2026-10-15). It downloads the file of a public distribution of the catalog, reads the first rows and returns them with the column types. | The data preview on the distribution page | The preview component is kept, but hidden while no preview service is configured (`NUXT_PUBLIC_DATA_PREVIEW_URL` is empty). The original team's service can't be used. Details: [below](#g15-data-preview). | `app/components/dataset-detail/OdsPreview.vue` |

## G12: how the email subscriptions worked

This is the implementation in mds-ui before it was removed. The code is in commit `33c04f4`, the user guide with
screenshots in [subscriptions/index.md](subscriptions/index.md).

- **Storage.** No database of its own: each subscriber is a [Listmonk](https://listmonk.app) subscriber, and the
  subscription is kept in its attributes: `frequency` (`daily` or `weekly`), `datasets`, `categories`,
  `organisations` (lists of ids) and `language` (`de`, `fr`, `it`, `en`).
- **Subscribe** (`server/api/subscribe/`): a form on the dataset page posts to `/api/subscribe/dataset` or
  `/api/subscribe/category` (`/api/subscribe/organisation` existed, without a form). It needed a Keycloak login,
  which gave the email address.
- **Preferences** (`server/api/subscription/preferences*.ts`, page `/subscription/preferences`): every email links to
  the page with `?id={subscriber id}&token={HMAC-SHA256 of the id, hex}`. The key is
  `NUXT_LISTMONK_PREFERENCES_HMAC_KEY`.
- **Digest** (`server/api/subscription/dispatch.post.ts`, `server/lib/subscription/dispatch.ts`): a scheduled GitHub
  workflow (`subscriptions.yaml` in `opendata-swiss/metadata.swiss`) posted `digest=daily` or `digest=weekly`. It
  loaded from piveau all datasets modified in the last day or week. Then, for each subscriber with that frequency, it
  picked the datasets they subscribed to, directly or through one of the dataset's categories, and sent at most 100
  of them in one Listmonk transactional email. Organization subscriptions weren't used in the digest. There is one
  template per language (template ids de 6, fr 7, it 8, en 9).
- **Problems not to repeat** (found in the review of 2026-10-08):
  - the digest endpoint had no protection, so anyone could trigger it;
  - email addresses were put into Listmonk SQL queries without escaping;
  - after subscribing, the user was redirected to the `Referer` without checking it (open redirect);
  - the HMAC key defaulted to an empty string, and the token wasn't compared in constant time.
- **What can't be taken over.** The Listmonk instance with the subscribers, the HMAC key, the email templates and the
  scheduled workflow all belong to the original team, and I14Y has no access to them. So the new service starts
  without subscribers, and the preference links in emails already sent can't work again.
- **License of Listmonk.** Listmonk is AGPL-3.0, which is on the I14Y license deny list (`DENY_LICENSES` in
  `.github/workflows/quality-gates.yml`). The CI check only looks at code dependencies, and Listmonk would run as a
  separate service, but the new service shouldn't rely on Listmonk without an approved exception.

### Building it again without Listmonk

iop-core has no email feature today. The "notifier" services in its code only write the audit trail. A proposal:

- **Storage:** subscribers and their subscriptions in the iop-core database, in the Portal module.
- **Sending:** [MailKit](https://www.nuget.org/packages/MailKit) (MIT) through an SMTP relay, or
  [Azure Communication Services Email](https://learn.microsoft.com/en-us/azure/communication-services/concepts/email/email-overview),
  a managed Azure service. Check which one the BFS may use, and where the service stores its data.
- **Templates:** one per language, in the repository, changed through pull requests.
- **Confirmation:** a confirmation email before a subscription becomes active (double opt-in), and rate limiting on
  the subscribe endpoint.
- **Administration:** subscribers manage their subscription through the signed link in each email, so no admin
  screen is needed at first. If the OGD office needs one later, it goes into the admin area of mds-ui (V3, G13).
- **Digest:** a scheduled job that uses G10 (`modifiedSince`).
- The same email sending can be used for the showcase submission notification and for comment notifications (G11).

### Two ways to subscribe, built together in V4

Decision of 2026-10-08: subscriptions come back in V4, after the eIAM login of V3, with two ways to subscribe built
at the same time:

| Who | How they subscribe | How they manage it |
|---|---|---|
| The public, not logged in (most subscribers) | Email address, confirmed through a link sent to it | Links in every email |
| Staff logged in with eIAM | The email address of their account, no confirmation needed | "My subscriptions" in the mds-ui admin area; the links in the emails work too |

- **One subscriber table**, with an optional eIAM user. If a member of staff subscribed anonymously before with the
  same address, the two records are merged when they subscribe logged in, so they don't get two digests.
- **For logged-in users, the address comes only from `GET /api/Users/current`**, never from a form field; otherwise a
  logged-in user could subscribe any address and skip the confirmation. When the address changes in eIAM, it's
  updated at the next login.

The flow below is the one for the public.

### Flow for the public, without login

These subscribers never log in: they have no account, neither in eIAM nor anywhere else. eIAM is only for the staff
who use the admin area of mds-ui (V3, G13). The email address is proven by a link sent to it,
and every later change goes through a link in an email. mds-ui only calls anonymous Portal endpoints of iop-core and
holds no secret.

1. **Subscribe.** A form on the dataset, category or organization page: email address and frequency (daily or
   weekly); the language is the one of the page. mds-ui posts it to `POST /api/portal/subscriptions`.
   - iop-core stores the subscription as *pending* and sends a confirmation email.
   - The answer is always "We've sent you an email", whether the address is known or not, so nobody can find out who
     has subscribed.
   - A new subscription for an address that already exists also needs confirming. Otherwise anyone could subscribe
     someone else's address to many datasets.
2. **Confirm.** The email links to an mds-ui page such as `/subscription/confirm?token=…`. The page shows a
   "Confirm" button, which posts the token to `POST /api/portal/subscriptions/confirm`. A button, not the link
   itself, because mail security scanners open every link in an email and would confirm it by accident.
   Unconfirmed subscriptions are deleted after a few days.
3. **Digest.** A scheduled job in iop-core (for example a Kubernetes CronJob) runs daily and weekly. For each
   confirmed subscriber, it finds the subscribed datasets modified since the last run, directly or through their
   category or organization (needs G10), and sends one email in the subscriber's language.
4. **Manage and unsubscribe.** Every email has a link to `/subscription/manage?token=…`, where the subscriber
   changes the frequency, removes subscriptions or unsubscribes from everything. Emails also carry the
   `List-Unsubscribe` and `List-Unsubscribe-Post` headers, so mail programs show a one-click unsubscribe button
   (RFC 8058).
5. **Lost the link?** On the manage page, the subscriber enters the email address and gets a new manage link by
   email. No password is ever needed.

Details:

- **Tokens:** long random values, stored only as hashes, compared in constant time. The confirmation token expires
  after 48 hours; the manage token stays valid until it's replaced.
- **Abuse:** rate limiting per IP address and per email address, a hidden honeypot field, and a captcha that doesn't
  track users if spam shows up.
- **Personal data:** only the email address, the language and the subscriptions are stored. Unsubscribing deletes
  them. The data protection review has to cover this.
- **Sender:** a sender address on a domain I14Y controls, with SPF, DKIM and DMARC set up, otherwise the emails end
  up as spam.
- **Without email:** an Atom feed per dataset, category or organization can be offered next to it, for people who
  don't want to give an address.

A ready-made newsletter tool with a permissive license would be [SendPortal](https://github.com/mettle/sendportal)
(MIT, PHP). It hasn't been updated for about two years, though, and it adds a PHP stack, so building it in iop-core is
the better fit.

### Open for discussion

There were two decisions. They are independent of each other:

1. **How the public proves who they are** before subscribing: email links, accounts of our own, or AGOV.
   **Decided on 2026-10-08:** email links for the public, eIAM accounts for logged-in staff, both built in V4 (see
   "Two ways to subscribe" above). The comparison below is kept for reference.
2. **Where the subscriptions are stored and who sends the emails:** iop-core with an SMTP relay, or a Listmonk that
   I14Y runs itself. Still open.

Whatever is chosen for the first, emails still have to be sent. Accounts of our own even need more of them:
confirming the address and resetting passwords.

#### Decision 1: how the public proves who they are

| | A. No account, email links (the flow above) | B. Accounts of our own: the public registers as `public` users | C. AGOV, the official login of Swiss authorities for the public |
|---|---|---|---|
| What the user does | Enters an email address, clicks the confirmation link. Done. | Registers with email and password, confirms the address, logs in, subscribes. | Needs an AGOV account first (the AGOV app or a security key), logs in, subscribes. |
| Effort for the user before subscribing | Low | Medium | High |
| What else becomes possible | Only subscriptions | Public accounts: favourites, comments under a name, showcase submissions with an account | Same as B, with a verified identity |
| Personal data we store | Email address and subscriptions | Email address, password hash, profile, login history | A user id, email address and subscriptions; AGOV holds the identity |
| Security responsibility | Small: the tokens in the links | **Large**: password storage, brute force, credential stuffing, password reset, account takeover | Small: AGOV is responsible |
| Development effort | Small to medium: a few Portal endpoints and three mds-ui pages | **Large**: all of A, plus registration, address confirmation, login, logout, forgotten and changed passwords, account deletion, sessions, brute-force protection | Medium: like the eIAM login of V3, plus connecting AGOV through eIAM |
| mds-ui connects to an identity provider | No | No: the login form is in mds-ui and posts to iop-core | Yes, to eIAM and AGOV (like V3 does for staff) |

What option B needs in iop-core:

- A third way to authenticate. iop-core accepts only eIAM and Keycloak tokens today, so public users need a session
  cookie or tokens issued by iop-core itself.
- Strict separation of rights: a `public` user must never get write access to the catalog. A single bug there lets
  the public change datasets.
- Passwords hashed with Argon2 or bcrypt, lockout after failed attempts, a captcha, expiring reset links, perhaps a
  second factor.
- Account deletion, a data protection review, and a penetration test before going live.

A variant of B without its main risk is **login by email link** (passwordless): mds-ui has its own simple login page
where the user only enters an email address, and the link in the email logs them in. It's option A plus a session,
so it can still have `public` users, without storing any password.

**Proposal:**

- For subscriptions alone, choose **A**. Having to register first stops many people from subscribing, and keeping
  passwords of the public is a large responsibility for that little gain.
- If public accounts are needed later (favourites, comments under a name, showcase submissions), don't store
  passwords: use login by email link, or AGOV where a verified identity is needed. AGOV asks a lot of the user (an
  app or a security key), which is too much just to receive emails.

#### Decision 2: where subscriptions are stored and who sends the emails

| | Without Listmonk: iop-core and an SMTP relay ([above](#building-it-again-without-listmonk)) | Listmonk run by I14Y |
|---|---|---|
| License | No issue | AGPL-3.0, on the I14Y deny list: needs an approved exception first |
| Editing email templates | In the repository, through pull requests | In the Listmonk admin screen, by the OGD office |
| Managing subscribers | No admin screen at first; subscribers manage themselves through the links | Listmonk admin screen |
| Bounces, sending queue, rate limits | To build | Included |
| Services to run | None more | Listmonk, its database, backups, upgrades |
| Personal data | In the iop-core database | In the Listmonk database |

If Listmonk is chosen, it should be an engine hidden behind iop-core:

```
Public browser ──► mds-ui (AKS): subscribe form, confirm and manage pages
                      │ anonymous calls, no secret
                      ▼
                 iop-core Portal module (Container Apps): rate limits, tokens, business logic
                      │ internal network, Listmonk API token from Key Vault
                      ▼
                 Listmonk (AKS, internal only) ──► managed PostgreSQL (own database)
                      │
                      ▼
                 SMTP relay ──► subscriber's mailbox

Scheduled job (daily, weekly): iop-core search (modifiedSince) → match subscriptions → Listmonk sends
OGD office: Listmonk admin screen, only from the internal network or VPN, or with an eIAM login
```

- **Listmonk on AKS, internal only.** The backend team is already setting up Elasticsearch on AKS (branch
  `feature/#730-set-up-elasticsearch-in-aks`), so the internal connection from iop-core to AKS is needed anyway.
- **Only iop-core calls Listmonk.** mds-ui keeps no secret, and the browser never reaches Listmonk.
- **Database:** a managed Azure PostgreSQL in a Swiss region, for example a new database and user on the existing
  server. It holds email addresses, so it needs backups and the data protection review.
- **Admin screen** (`/admin`) only from the internal network or VPN. Listmonk 4 supports OIDC logins, so the OGD office
  could log in with eIAM; to be checked.
- **API token** of a dedicated Listmonk API user that may only manage subscribers and send transactional emails. In
  Key Vault, never in a log.
- **Scheduled job:** a Kubernetes CronJob or a Container Apps Job, replacing the GitHub workflow of the original
  team.
- **Tokens in the links** are created and stored by the Portal (random, stored as hashes), not `HMAC(subscriber id)`
  as before.
- iop-core keeps its own mapping from subscriptions to Listmonk subscriber ids and calls Listmonk by id, so no email
  address is ever put into a Listmonk SQL query.

Reusing the old mds-ui server code with a self-hosted Listmonk looks quicker but isn't: the email address came from a
Keycloak login that's gone, the changes were read from piveau, all the security problems above would have to be
fixed, and mds-ui would hold the Listmonk token.

**Proposal:** without Listmonk, unless the OGD office needs to edit templates and manage subscribers itself. In that
case, Listmonk behind iop-core as described, after the license exception is approved.

Sources: [AGOV](https://www.agov.admin.ch/en/information-authorities),
[CH-LOGIN becomes AGOV](https://www.anmeldestelle.admin.ch/en/ch-login-becomes-agov).

## G13: admin functions and login

### Decision (2026-10-08)

admin-ui, the I14Y input portal (`input.i14y.d.c.bfs.admin.ch`), will be deleted, and all its features will be
built again in mds-ui. So the admin pages go into mds-ui, including the GOGD admin menu that mds-ui had before, and
mds-ui gets a login again. In the plan (`docs/NextSteps.md`) this comes in two versions:

- **V3:** the eIAM login and the frame of the admin area (navigation, layout, permissions). V4 builds on it too.
- **V5:** the features of admin-ui and the GOGD admin menu, then admin-ui is deleted.

- mds-ui V1 is still public and read-only. admin-ui keeps running until the end of V5, so the Admin API must stay
  until then.
- The login button in the mds-ui header opens the input portal today, so it opens admin-ui. At the switch-over in V5,
  it opens the admin area of mds-ui instead.
- Decap CMS isn't part of this. It uses neither iop-core nor Keycloak: editors log in with GitHub. V1 removes it;
  the CMS comes back in V4 (`docs/CmsComparison.md`).

### What admin-ui does

admin-ui is an Angular app of about 90 components (`frontend/admin-ui`). Its routes are in
`src/app/app-routing.module.ts`.

| Area | Routes | What it does |
|---|---|---|
| Login and permissions | `/login`, `/signin-callback`, `/signout-callback`, `/unauthorized` | Login, logout, and what the user is allowed to do. See "How admin-ui logs in" below. |
| Home | `/home` | The start page after login. |
| Catalog overview | `/catalog/all`, `/catalog/datasets`, `/catalog/dataservices`, `/catalog/publicservices`, `/catalog/concepts`, `/catalog/mappingtables` | Search over all resources, with filters: type, publisher, registration status, publication level, themes, formats and more. |
| Datasets | `/catalog/datasets/…`: description, distributions, quality info | Create, edit and delete datasets, and manage their versions. Set the registration status and the publication level, or propose a change. Distributions and their access services. The structure (classes and properties, import and export). Quality information. The entries in DCAT catalogs. |
| Data services | `/catalog/dataservices/…`: description | Create, edit and delete; status and level; which datasets a service serves. |
| Public services | `/catalog/publicservices/…`: description | Create, edit and delete; status and level; channels, requirements and relations. |
| Concepts | `/catalog/concepts/…`: description | Create, edit and delete, and manage versions. Code lists: entries, import and export. Locking. Structure references. |
| Mapping tables | `/catalog/mappingtables/…`: description | Create, edit and delete; relations, import and export. |

admin-ui sends no emails and doesn't use Listmonk, so V3 needs no email service.

### How admin-ui logs in

- `oidc-client-ts` in the browser, authorization code flow with PKCE (`src/app/auth/auth.service.ts`).
- The identity provider is eIAM: `KEYCLOAK_AUTHORITY_URL` in `src/assets/config/appconfig.json` points to
  `https://identity-eiam-r.eiam.admin.ch/realms/edi_bfs-i14y`, client `BFS-i14y`. Scopes `openid offline_access roles`.
- After the login, eIAM gives the token. admin-ui keeps it, reads the roles from its `role` claim (`getRoles()`), and
  `src/app/auth/api_auth.interceptor.ts` sends it to the API. The user information comes from the API
  (`getUserInfo`).

### How iop-core handles users

iop-core connects to the identity providers and provides the user information. It doesn't do the login itself.

- It validates JWT bearer tokens from two kinds of issuers, eIAM and Keycloak, configured in the sections `Eiam` and
  `Keycloak` (`Bfs.Iop.Infrastructure.Security/ServiceCollectionExtensions.cs`).
- `GET /api/Users/current` returns the first name, last name, email, business role and organizations of the user.
  iop-core reads them from the claims of the token (`GetCurrentUserCommandHandler`).
- `GET /api/Users/current-agents` returns the organizations of the user, and `AllowActions` what the user may do.

### How mds-ui logs in: with eIAM, like admin-ui

Users log in with **eIAM**, the identity service of the federal administration, which gives the token after the
login. mds-ui does it the way admin-ui does. In admin-ui the setting is called `KEYCLOAK_AUTHORITY_URL`, but it points
to eIAM (`https://identity-eiam-r.eiam.admin.ch/realms/edi_bfs-i14y`, client `BFS-i14y`, in
`src/assets/config/appconfig.json`). eIAM runs on Keycloak software, which is why the paths contain `/realms/`.

mds-ui doesn't connect to the Keycloak that iop-core accepts next to eIAM (the `Keycloak` section; locally the realm
`i14y-local` in `docker-compose.yml`, next to `eiam-local` for eIAM). Only eIAM.

1. The login button starts an OIDC login in the browser: authorization code flow with PKCE, with
   [oidc-client-ts](https://github.com/authts/oidc-client-ts) like admin-ui. It doesn't depend on Angular, so it works
   in mds-ui too. The browser goes to the eIAM login page.
2. eIAM sends the browser back to an mds-ui callback page with a code, and oidc-client-ts gets the token. mds-ui is a
   public client: no secret.
3. mds-ui sends the token to iop-core in the `Authorization: Bearer` header (`api-client/iop-core-fetch.ts`).
   iop-core validates it as a token of the `Eiam` issuer.
4. mds-ui takes the user from `GET /api/Users/current` and the permissions from `AllowActions`.

Where mds-ui can do better than admin-ui:

- **Roles and user information from iop-core.** admin-ui decodes the roles from the token itself (`getRoles()`, roles
  starting with `BFS-i14y.`) and also asks eIAM for the user information (`loadUserInfo: true`). mds-ui takes both
  from iop-core, so its only contact with eIAM is the login and the token.
- **Token storage.** admin-ui also copies the token to `localStorage` (`access_token`), where any script injected into
  the page could read it. mds-ui keeps it in the memory or the session storage of oidc-client-ts.
- **Token expiry.** admin-ui shows a dialog when the token is about to expire (no silent renewal). mds-ui can do the
  same at first.

What's needed:

- The redirect URLs of mds-ui (callback and logout pages) registered at eIAM, for the client `BFS-i14y` or a new
  client. That goes through the eIAM registration process, which can take a while, so ask early.
- The eIAM address and client id as settings of mds-ui, for example `NUXT_PUBLIC_EIAM_AUTHORITY_URL` and
  `NUXT_PUBLIC_EIAM_CLIENT_ID`. They aren't secrets.
- Pages behind the login are rendered in the browser only (`ssr: false` in `routeRules`), because the token exists
  only in the browser. They don't need SEO.

This is a different kind of login than the one removed from mds-ui (below), where mds-ui's own server kept a session
with Zazuko's Keycloak.

### What iop-core already has

admin-ui calls the Admin API today (`bfs-iop-admin-web-api-client`), and the Admin API will be deleted. The Core API
already has read and write endpoints for every area admin-ui edits. Counted in its swagger:

| Area (swagger tag) | Read | Write |
|---|---|---|
| Datasets | 11 | 15 |
| Concepts | 20 | 14 |
| MappingTables | 8 | 13 |
| DataServices | 6 | 7 |
| PublicServices | 9 | 7 |
| DcatCatalogs | 8 | 6 |
| Agents | 5 | 3 |
| DatasetQualityInformation | 2 | 3 |
| Vocabularies | 3 | 3 |
| Media | 2 | 2 |
| FilterConfigurations | 1 | 2 |
| Persons | 3 | 1 |
| Search | 2 | 1 |
| AllowActions, Users, AuditTrail | 6 | 0 |

So most of this is frontend work: mds-ui uses the client it already generates from the Core
swagger (orval). Each admin-ui call still has to be checked against Core when its page is built again.

For the GOGD admin menu, iop-core has:

- `GET /api/Agents/statistics`: counts per publisher and resource type. A start for a dashboard.
- `/api/DatasetQualityInformation` (read and write, and `GET …/definition` for the questions): quality information
  per dataset. A start for "Quality & Metrics".
- `/api/Vocabularies/configurations` (read and write) and `GET /api/DcatCatalogs/{id}/themes`: related to the
  categories.

### What is missing

- The redirect URLs of mds-ui registered at eIAM (see "How mds-ui logs in" above). iop-core already validates eIAM
  tokens, so the login itself needs no change in iop-core.
- A role for the OGD office, checked by the GOGD admin endpoints in iop-core.
- What the dashboard and the metrics page should show was never defined. Define it first, then see which endpoints
  are missing, for example counts over time, or metadata quality scores per organization like piveau's metadata
  quality assessment (MQA).
- What managing categories means. The EU data themes are a fixed EU vocabulary, so deleting them makes little sense.
  It could mean choosing which themes the opendata.swiss catalog uses (see G2).
- Any admin-ui call that has no Core equivalent, found while checking each page.

### What mds-ui had before

This is the implementation in mds-ui before it was removed. The code is in commit `33c04f4`.

"GOGD" is most likely the Geschäftsstelle OGD, the Open Government Data office at the BFS that runs opendata.swiss.
The code never spells it out.

- **Login** (`server/api/auth/keycloak.get.ts`, with `nuxt-auth-utils`):
  - It used Zazuko's Keycloak (`keycloak.zazukoians.org`, realm `lindas-next-ref`, client `piveau-hub-ui`).
  - The session cookie kept only the user's name and email.
  - `app/middleware/require-auth.ts` sent visitors without a session to the login. `server/lib/login.ts` kept the
    page to come back to in the cookie `auth-return-to`.
  - The top header showed a logout button to logged-in users (`app/components/headers/OdsTopHeader.vue`).
  - The email subscriptions (G12) used the same login to get the subscriber's email address.
- **Who was an admin:** anyone who was logged in. `OdsHeader.vue` showed the `adminOnly` menu items to every
  logged-in user. There was no role check.
- **The "GOGD Admin" menu** (`app/composables/navigation-items.ts`, labels in `message.header.navigation.admin`):

  | Menu item (de / en) | Path | State |
  |---|---|---|
  | Dashboard | `/gogd/dashboard` | The page was never built. |
  | Qualität & Metriken / Quality & Metrics | `/gogd/metrics` | The page was never built. |
  | DCAT Kategorien / DCAT Categories | `/gogd/categories` | A read-only list of the EU data themes (vocabulary `VOCAB_EU_DATA_THEME`, read from iop-core). Each card had a delete icon that did nothing. |

## G14: showcases in iop-core

Decided on 2026-10-13: showcases move from the content repository into iop-core. Since 2026-10-16 the backend
(entity, endpoints, permissions, publication workflow) is built in V2, like the other resources; the submission
endpoint, the migration and the mds-ui pages follow in V4. Pages, the handbook and
the blog stay in the content repository. The CMS side of the decision is in
[CmsComparison.md](CmsComparison.md#decision-2026-10-13).

### Why

Showcases are structured data, not articles:

- they refer to datasets and organizations, and need reverse lookups such as "showcases using this dataset" and
  counts per organization;
- the list is filtered by type, theme and keyword;
- the public submits them. With Git, a GitHub App on the server had to open a pull request for each submission, and
  the submitter's name ended up in the public Git history.

### Data model

It follows the conventions of `DataAccess.Relational/Entities`. Basing it on `PublishableEntityBase` gives the
publisher, the publication level and its proposal, and the existing authorization checks
(`PublishableEntityAuthorizationService`) without new code.

| Entity | Fields |
|---|---|
| `Showcase : PublishableEntityBase` | `Slug` (unique, used in the URL), `Type` (vocabulary code), `Url`, `Pinned`, `Title`, `Summary` and `Description` as `MultiLanguage` (`Description` is plain Markdown), `Properties` (jsonb, for the answers of the submission form) |
| `ShowcaseMedia` | `ShowcaseId`, `Url` (from `/api/Media`), `Role` (cover or gallery), `AltText` (`MultiLanguage`), `Position` |
| `ShowcaseLink` | `ShowcaseId`, `TargetType` (dataset, data service, concept, public service, mapping table, agent, external), `TargetId` for iop-core resources, `Href` and `Label` for external links, `Role` (references, author, contributor), `Position` |
| `ShowcaseKeyword` | `ShowcaseId`, `Language`, `Keyword` |
| `ShowcaseTheme` | `ShowcaseId`, theme code from the vocabulary |
| `ShowcaseContact` | `ShowcaseId`, `Name`, `Email`, `Url`. Never returned by public endpoints; can be deleted on request |

`ShowcaseLink` points to its target by type and id rather than by foreign key, because the existing `Resource`
table already has a nullable foreign key per role and per resource type. Without a foreign key:

- the handler checks that the target exists when a showcase is saved;
- public endpoints return only links whose target is still public;
- the admin area lists showcases with broken links.

An index on (`TargetType`, `TargetId`) makes reverse lookups fast.

### Drafts, review and publishing

They use the existing publication level fields; nothing new is needed.

| State | Fields |
|---|---|
| Draft | `PublicationLevel = Internal` |
| Submitted for review | `PublicationLevelProposal = Public` |
| Published | `PublicationLevel = Public` |
| Withdrawn | Back to `Internal`, or `RegistrationStatus = Retired` |

Submissions from the public are stored as drafts with a review proposal. Their publisher is the agent of the
opendata.swiss portal, still to be chosen. The fields of the submission form and how they map to this model are in
[ShowcaseSubmission.md](ShowcaseSubmission.md#mapping-to-iop-core-g14).

### Endpoints

The routes follow `DatasetsController` and `ConceptsController`.

| Endpoint | Access | Purpose |
|---|---|---|
| `GET /api/Showcases?q=&type=&theme=&keyword=&linkedType=&linkedId=&agentId=&sort=&page=` | Anonymous (public showcases only) or logged in (by permission) | List, filters, reverse lookup |
| `GET /api/Showcases/{id}`, `GET /api/Showcases/by-slug/{slug}` | Same | Detail |
| `GET /api/Showcases/counts?groupBy=agent` | Anonymous | Counts on the organization pages |
| `POST /api/Showcases` | Editor | Create, as a draft |
| `PUT /api/Showcases/{id}` | Editor | Update, with `If-Match` so two editors don't overwrite each other (412 on conflict) |
| `DELETE /api/Showcases/{id}` | Editor, drafts only | Delete |
| `PUT /api/Showcases/{id}/publication-level-proposal` | Editor | Submit for review (existing handler) |
| `PUT /api/Showcases/{id}/publication-level` | Publisher | Publish or withdraw (existing handler) |
| `POST /api/ShowcaseSubmissions` | Anonymous, captcha, rate limit | Submission from the public, multipart with images |
| `POST /api/Media` (existing) | Logged in | Images uploaded by editors |

Every write sends an AuditTrail message, as the other resources do.

### Still to decide

- The two permissions (`AllowActions`): edit showcases, and publish them.
- The publisher agent of submissions from the public.
- How long the contact details of submitters are kept.
- The image library: ImageSharp 3 has a split license; SkiaSharp (MIT) or Magick.NET (Apache-2.0) avoid the question.

### Moving the existing showcases

A one-time script reads the Markdown files of the content repository and creates the showcases through the API:

- slugs are kept, so `/showcase/{slug}` links keep working;
- piveau dataset URIs are mapped to iop-core ids (postponed from V1 step 3 to this migration on 2026-10-16);
- images are uploaded to `/api/Media`.

Afterwards the `showcases/` folder and its images are deleted from the content repository, and the `showcases`
collection from `content.config.ts`.

## G15: data preview

Decided on 2026-10-15: mds-ui keeps its preview component, and iop-core provides the service behind it, in V2.

### How it works today

The distribution page shows a preview for tabular files: a table, numerical charts and categorical charts. The
component is `piveau-preview-plugin` (MIT, a Vue component). It doesn't read files itself: it calls a service that
downloads and parses the file, and only displays the result.

- The original team used piveau's preview service (`piveau-hub-data-preview`), run on their own server. It can't be
  used any more.
- The component is now hidden unless a service is configured in `NUXT_PUBLIC_DATA_PREVIEW_URL`, and its table and
  chart libraries load only on the distribution page, only when a preview is shown.
- piveau's service is published as a container image (`registry.gitlab.com/piveau/hub/piveau-hub-data-preview`), but
  its source code isn't public and it declares no license, so it isn't used. It's a Spring Boot application that
  needs its own PostgreSQL database.

### What iop-core has to provide

An anonymous endpoint that answers in the format the component expects, so mds-ui needs no change:

```
GET {base}?urlString=<file URL>&fileFormat=CSV|TSV|XLSX|XLS|ODS[&compressFormat=<media type>]
```

Response (JSON), taken from the same service run by data.europa.eu:

| Field | Content |
|---|---|
| `data` | The first rows, each an object keyed by column name |
| `labels` | `all_labels`, `numerical_labels`, `categorical_labels`, `time_labels`: the column names by type |
| `categorized` | For each categorical column: the count of each value, and the chart to use |
| `default_view_options` | The view to open first (table, numerical or categorical), the default charts and the axes of the numerical chart |

Errors use HTTP status codes; the component shows its own messages for 404, 401 and 403, 413 (file too large) and
5xx.

### Security: only files of the catalog, never internal addresses

A preview service downloads a file on behalf of whoever calls it. Without limits, it can be used to reach internal
systems (server-side request forgery).

- **Only URLs of the catalog.** iop-core previews a `urlString` only if it is the download or access URL of a public
  distribution. Any other URL gets 404. Unlike piveau's service, it never fetches an arbitrary address.
- **No internal addresses.** Publishers enter the URLs themselves, so a catalog URL can still point inward. Before
  connecting, and after every redirect, the resolved IP address is checked; private ranges, loopback and
  `169.254.0.0/16` are refused. In .NET this is `SocketsHttpHandler.ConnectCallback`, which also stops DNS rebinding.
  The egress rules of iop-core add a second barrier.
- **Limits.** Only http and https; a few redirects at most; a timeout; a maximum download size; reading stops after
  the first rows.
- **Load.** Results are cached per URL for some hours, and the endpoint is rate-limited.

### Building it

| Part | How | Estimate |
|---|---|---|
| Endpoint, catalog check, cache, rate limit | As the other anonymous endpoints of iop-core | 1–2 days |
| Safe download | `HttpClient` with the IP check, limits and redirect checks above | 1–2 days |
| CSV and TSV | CsvHelper; UTF-8 first, then Windows-1252 (`System.Text.Encoding.CodePages`); semicolon or comma detected | 1–2 days |
| XLSX and XLS | ExcelDataReader, first sheet, first rows | 1–2 days |
| Column types and the fields `labels`, `categorized`, `default_view_options` | Simple rules: numbers, dates, few distinct values | 1–2 days |
| ODS | No ready library; unzip and read `content.xml`. Can come later | 2 days, optional |
| Tests and security review | Encodings, broken and large files, redirects, internal addresses | 1–2 days |
| **Total** | Without ODS | **about 6–12 days** |

The libraries named are believed to be MIT or Apache-2.0 (CsvHelper offers MS-PL or Apache-2.0); they must pass the
license check before use. Once deployed, mds-ui only sets `NUXT_PUBLIC_DATA_PREVIEW_URL` to the endpoint.

## Not iop-core, still open

- **URLs.** Datasets are addressed by their DCAT identifier (`/datasets/{identifier}`), organizations by their
  identifier (`/organizations/{identifier}`), distributions by their iop-core id. Whether the old opendata.swiss
  URLs still work, or need redirects, depends on whether these identifiers are the ones piveau used.
- **Showcase counts per organization** come from Nuxt Content from V1 step 3, and from iop-core (G14) from V4.
  Until V1 step 3 they show 0.
