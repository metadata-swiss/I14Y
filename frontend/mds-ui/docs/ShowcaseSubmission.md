# Showcase submission form

The form where people outside the administration propose a showcase: an app, a visualization or a study that uses
open data. It's written down here because the form is removed from mds-ui in V1 and built again in V4.

- **Removed in V1, step 1.** Every part of it depends on services we no longer have: the dropdowns search the
  original team's piveau, submissions were saved as pull requests by the original team's GitHub App, and the
  notification went through their Listmonk.
- **Built again in V4**, step 4, against iop-core: the submission becomes a draft `Showcase` in the review queue of
  the admin area ([G14](iop-core-gaps.md#g14-showcases-in-iop-core)). The CMS side is in
  [CmsComparison.md](CmsComparison.md#decision-2026-10-13).
- **The source** is in commit `33c04f4`:

  ```bash
  git show 33c04f4:frontend/mds-ui/app/pages/showcases/submit.vue
  git show 33c04f4:frontend/mds-ui/server/api/showcases.post.ts
  ```

- **The texts are kept** in the four language files, so V4 can use them as they are. See [Texts](#texts).

## How people get to it

- The showcase list (`/{locale}/showcases`) has a button "Submit a showcase" ("Showcase einreichen") in the hero area.
  The main menu has no direct link.
- The form is at `/{locale}/showcases/submit` and is rendered in the browser only.

## Page structure

The page has a title, an introduction, three groups of fields and a submit button. Fields marked "yes" in the
"Required" column have an asterisk in the form.

**Introduction** (`paragraph.top`): thanks for sharing an open data project; fields with an asterisk are required;
the OGD office reviews the request and gets back to the submitter.

### 1. Showcase information

| Field | Input | Required | Notes |
|---|---|---|---|
| Showcase title | Text | yes | At least 5 characters. Turned into the slug of the URL |
| Showcase URL | Text | yes | Link to the app, visualization or study |
| Content type | Dropdown | yes | Options from the `showcase-types` vocabulary, for example application, visualization |
| Images | File upload, several files, images only | yes | Resized to 900 px wide and converted to JPEG. The first image is the cover |
| Dataset(s) used | Search while typing, several | **not marked, but required by the validation** | Searched the datasets of piveau, 10 results, 300 ms delay |
| Theme(s) | Multi-select | yes | Options from the `data-theme` vocabulary (EU data themes) |
| Keywords | Text | no | Separated by commas |
| Created by | Text | yes | Who made the showcase |

### 2. Contact details

| Field | Input | Required | Notes |
|---|---|---|---|
| Name First Name | Text | yes | |
| Email address | Email | yes | Label says "will not be published". Not saved in the showcase, only used for the notification |
| GitHub account | Text | no | Label says "will be published" |

### 3. Tell us a little more

An introduction (`paragraph.more`) explains that the answers help the OGD office write the showcase text and promote
it on the portal and on social media, and that every question is optional. It mentions a "red box shown here", but
the page shows no such box.

Each question is a text area with hints below it:

| Question | Hints |
|---|---|
| Who are you? | Name of your organization or team. What is your mission or main objective? |
| What is the goal of your showcase in one sentence? | What question does it answer, what need does it address? Suggested start: "Thanks to this showcase, the user can…" |
| Why did you use this specific data? | Why is the data useful? Which technologies or tools do you use? Any limitations or challenges with the data? |
| Who are the end users? | How is the showcase useful to them? Which fields benefit from reusing this data? |
| What challenges did end users face BEFORE using your showcase? | Major challenges; how were they overcome? |
| What challenges did YOU face in creating this showcase? | Did you prepare or clean the data? Was it easy to find? How did you identify sources? Difficulties accessing it? |
| What should the user do to get the most out of this showcase? | Link and access instructions; conditions such as registration; tips |
| Would you like to add anything else? | Hashtags or people to mention, with links; visuals or other material |

### Submit button and results

| Case | What the user saw |
|---|---|
| Sending | The button is disabled until the server answers |
| Success | The form is cleared. A message "Your submission has been received and is pending review" is shown through the global message cookie |
| Validation error (400) | A red banner at the top: "Showcase submission failed", with one line per wrong field. The page scrolls to the top |
| Same title submitted before (409) | Banner: "A submission with the exact same title already exists" |
| Other errors | Banner with the HTTP status, or with the JavaScript error and stack trace |

## Mapping to iop-core (G14)

| Form field | Before (Markdown in the content repository) | V4 (`Showcase` in iop-core) |
|---|---|---|
| Title | `title`, only in the file of the submission language | `Title` (`MultiLanguage`), the submission language; editors add the others |
| URL | `url`, copied to all four files | `Url` |
| Content type | `type`, a vocabulary URI | `Type`, vocabulary code |
| Images | `images`, files in `assets/` | `ShowcaseMedia`, files through the submission endpoint into Azure Blob storage |
| Datasets | `datasets`, piveau URIs with labels | `ShowcaseLink` with `TargetType = Dataset` and the iop-core id |
| Themes | `themes`, URIs | `ShowcaseTheme` |
| Keywords | `keywords` | `ShowcaseKeyword`, in the submission language |
| Created by | `createdBy` | `ShowcaseLink` with role author and an external label, or `Properties`; to be decided |
| Name, GitHub account | `relationships`, a point of contact, written into the public repository | `ShowcaseContact`, never returned by public endpoints |
| Email | Not stored; used as the sender of the notification | `ShowcaseContact.Email`, internal only |
| Tell us a little more | `more.*`, written into the public repository | `Properties` (jsonb), internal only, shown to editors in the review queue |

## Problems in the old form, to fix in V4

| Problem | Fix |
|---|---|
| No captcha and no rate limit | ALTCHA captcha and rate limiting in iop-core |
| Datasets are required by the validation but not marked as required | Mark the field; check on the client before sending |
| Themes are marked as required but optional in the validation | Decide one way and make both agree |
| Only the submission language gets a title; the other three files had none | Store one language; editors complete the others before publishing |
| Uploaded images were referenced by a wrong path and didn't show | Images go to iop-core; check the preview in the review queue |
| The submitter's name, GitHub account and answers ended up in the public Git history | Contact details and answers stay internal in iop-core and can be deleted |
| The GitHub account is published, but nothing else about the submitter is | Decide whether anything about the submitter is shown publicly, and say so on the form |
| No privacy notice | Short notice under the contact fields: what is stored, why, for how long |
| The hint "Did you prepare or clean the data?" appears twice under the challenges question | Show it once |
| The introduction mentions a "red box shown here" that isn't on the page | Remove the sentence, or show the example |
| "Name First Name" as one label | "Name" and "First name", or "Full name" |
| Errors could show a JavaScript stack trace to the public | Show a plain message; log the details |
| After success the form is only cleared | A confirmation page or message that says what happens next |
| No email confirmation to the submitter, and no notification that reached anyone | Email to the editors and a confirmation to the submitter, through the V4 mail module |

## Requirements for V4

- The form is part of the new UI design of V2, so it's built once, in the new design.
- Content type, themes and datasets are searched in iop-core through the orval client.
- The form posts to `POST /api/ShowcaseSubmissions` in iop-core (G14), multipart with the images and the captcha
  token. mds-ui needs no server route for it.
- Limits on the images: number of files, size per file, image types only. The server resizes them.
- Every field and hint in four languages, from the kept texts.
- Accessible: labels on every field, errors linked to their fields, usable with the keyboard.

## Texts

Kept in `i18n/locales/{de,fr,it,en}.json` when the form is removed:

| Key | Content |
|---|---|
| `message.showcase.submission_form.*` | Title, introductions, group headings, labels, hints, success and failure messages, submit button |
| `message.header.navigation.showcase_submit` | The "Submit a showcase" button on the showcase list |
| `message.server.api.showcases.post.*` | "Pending review" and the two error messages |

## Screenshots

To be added before the form is removed in V1 step 1: run mds-ui locally, open `/de/showcases/submit`, and save
screenshots of the whole page, an open dataset search, and the error banner, under `docs/img/showcase-submission/`.
