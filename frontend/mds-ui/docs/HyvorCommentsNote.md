# Comments (switched off)

mds-ui had comments on its pages, provided by [Hyvor Talk](https://talk.hyvor.com). Hyvor Talk is a paid hosted
service and isn't open source, and mds-ui used the account of the original project. The comments are switched off
and the Hyvor Talk code is removed until I14Y decides on a comment service of its own (gap G11 in
[iop-core-gaps.md](iop-core-gaps.md)).

This page describes what the feature did and how it was built, so that it can be added back.

To see the removed code, find the commit that removed it and show the files of the commit before:

```bash
git log --oneline -- frontend/mds-ui/server/lib/webhooks/hyvor.ts
git show <commit>~1:frontend/mds-ui/server/lib/webhooks/hyvor.ts
```

## What it did

| Feature | Where |
|---|---|
| A comment thread at the end of the page, in the language of the page | Dataset page, blog posts, handbook pages, showcase pages |
| The number of comments of each dataset | Dataset search results, list and card view |
| Ratings of showcases (part of the Hyvor Talk thread), written back to piveau as `schema:ratingValue` | Showcase pages. The rating was not shown anywhere in mds-ui. |
| An email to the publisher of a dataset when a new comment was published | Server webhook `POST /api/webhooks/hyvor` |

## Page ids

Each thread was identified by a page id. The comments already in the Hyvor Talk account are stored under these ids,
so a new service that imports them needs the same ids, or a mapping.

| Page | Page id | Set in |
|---|---|---|
| Dataset | `dataset-{dataset id}`. The dataset id is now the DCAT identifier; under piveau it was the piveau id. | Dataset page, dataset list items |
| Blog post | `blog-{content id}` | `app/pages/blog/[year]-[month]/[slug].vue` |
| Handbook page | `handbook-{content id}` | `app/components/handbook/OdsHandbookPage.vue` (`app/pages/handbook/[...slug].vue` passed the same id) |
| Showcase | The content stem without the language, for example `showcases/{name}` | `app/pages/showcase/[id].vue` |

## Pages

The package `@hyvor/hyvor-talk-vue` wraps the Hyvor Talk web components for Vue. Nuxt had to transpile it and its
dependency (`build.transpile: ['@hyvor/hyvor-talk-vue', '@hyvor/hyvor-talk-base']`).

**Comment thread.** `OdsPage.vue` had an optional prop `commentsId`. Pages that wanted comments set it, and
`OdsPage` showed the thread after the page content:

```vue
<script setup lang="ts">
import { Comments } from '@hyvor/hyvor-talk-vue'

const { locale } = useI18n()
const { comments: { websiteId } } = useRuntimeConfig().public
</script>

<template>
  <!-- after the content of the page -->
  <div v-if="commentsId" class="container">
    <Comments :website-id="websiteId" :page-id="commentsId" :page-language="locale" />
  </div>
</template>
```

The dataset page doesn't use `OdsPage`. It had the same `<Comments>` at the end of its main section, with the page id
`` `dataset-${dataset.id}` ``.

**Comment counts.** Each dataset list item (`OdsDatasetListItem.vue`, `OdsDatasetCardListItem.vue`) showed the count
in its meta line:

```vue
<span class="meta-info__item">
  <CommentCount :page-id="`dataset-${props.dataset.id}`" :language="locale" />
</span>
```

`OdsDatasetList.vue` then loaded the counts of all datasets on the page in one request. After mounting and after each
update, it waited up to 1 second for the script of Hyvor Talk and called it:

```ts
import { waitUntil } from 'async-wait-until'

async function loadCommentCounts() {
  await waitUntil(() => window.hyvorTalkCommentCounts, 1000)
  window.hyvorTalkCommentCounts!.load({ 'website-id': 15455 })
}

onMounted(loadCommentCounts)
onUpdated(loadCommentCounts)
```

The website id was hard-coded here. When the script wasn't there in time, the browser console showed
"Timed out after waiting for 1000 ms".

## Server: webhook `POST /api/webhooks/hyvor`

Hyvor Talk called this endpoint for new comments and ratings
(`server/api/webhooks/hyvor.post.ts`, logic in `server/lib/webhooks/hyvor.ts`).

1. **Signature.** The HMAC-SHA256 of the raw body with `hyvor.webhookSecret`, as hex, had to equal the `X-Signature`
   header. Otherwise: 400.
2. **Body.** A missing body: 400.
3. **Switch.** With `hyvor.webhooksEnabled` false, the endpoint returned 204 and did nothing.
4. **Events.** `comment.create` and `comment.update` went to the comment handler, `rating.created` and
   `rating.updated` to the rating handler. Other events: 400.

**Comment handler: notify the publisher.** It sent an email only when all of these were true:

- `hyvor.publisherNotificationTemplateId` is set;
- the comment is a top-level comment, not a reply;
- the comment is published, either right away (empty `history`) or by a moderator (the first `history` entry has
  `type: 'moderation'` and `new_status: 'published'`).

The dataset id was the page id without `dataset-`. The handler loaded the dataset from piveau, took its first contact
point and sent a Listmonk transactional email to the contact point's email. Without an email, it returned an error.

```ts
await listmonk.transactional.send({
  template_id: publisherNotificationTemplateId,
  subscriber_email: publisher.email,
  subscriber_mode: 'external',
  data: {
    page: { url: comment.page.url, title: comment.page.title },
    publisher: { name: publisher.name },
    author: { email: comment.user.email },
    comment: { body: comment.body_html },
  },
})
```

The Listmonk template uses these `data` fields. In iop-core, the contact points of a dataset are
`DcatDatasetModel.contactPoints` (`VCardModel`: name `fn`, email `hasEmail`).

**Rating handler: write the rating to piveau.** The page id had to match `showcase/{id}`. The handler loaded the
showcase from the piveau hub-repo and replaced its `schema:ratingValue` with the average rating of the page. Showcase
pages used `showcases/{name}` as page id, which doesn't match this pattern (the tests used `showcase/…`), so the
ratings were probably never written.

## Configuration

| Runtime config | Environment variable | Default | Meaning |
|---|---|---|---|
| `public.comments.websiteId` | `NUXT_PUBLIC_COMMENTS_WEBSITE_ID` | `15455` | Hyvor Talk website, the account of the original project. `k8s/deployment.yaml` read it from the config map `public-endpoints`, key `comments-website-id`. |
| `hyvor.webhooksEnabled` | `NUXT_HYVOR_WEBHOOKS_ENABLED` | `false` | Whether the webhook does anything |
| `hyvor.webhookSecret` | `NUXT_HYVOR_WEBHOOK_SECRET` | empty | Secret for the `X-Signature` check |
| `hyvor.publisherNotificationTemplateId` | `NUXT_HYVOR_PUBLISHER_NOTIFICATION_TEMPLATE_ID` | `5` (`9` in `k8s/deployment.yaml`) | Listmonk template of the publisher email |

## Tests

These tests were removed with the code:

- `server/tests/webhooks/hyvor.test.ts` (mocha). Comment handler:
  - an email is sent for a new top-level comment;
  - no email for a reply, for a pending comment, or without a template id;
  - an email is sent when a pending comment is approved;
  - real delivery through Listmonk to Mailpit.

  Rating handler: errors without a page or with a wrong page id, and the showcase is updated.
- `server/tests/webhooks/hyvor.test.n3` (api-tuner). A wrong or missing signature returns 400; rating created, rating
  updated, comment created.
- `server/tests/webhooks/payloads/`: example payloads of Hyvor Talk. `signature.js <file>` calculated the
  `X-Signature` of a payload file.

The test dataset `hyvor-test` in `server/tests/hub-repo/` is kept, because the subscription tests use it too.

## Adding comments back

1. Choose the comment service (G11).
2. Put the thread back where `<Comments>` was: in `OdsPage.vue` behind a prop set by the blog, handbook and
   showcase pages, and on the dataset page. Use the page ids above if old comments are imported.
3. Show the comment counts in the dataset list, loaded for all datasets of a page in one request.
4. Notify publishers as the comment handler did, with the contact points from iop-core.
5. Check the signature of the service's webhooks, and keep a switch to turn them off.
6. Put the secrets in Key Vault (phase 8). Comments contain personal data of their authors, so the data protection
   review has to cover the new service.
