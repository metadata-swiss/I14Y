# Nuxt Minimal Starter

Look at the [Nuxt documentation](https://nuxt.com/docs/getting-started/introduction) to learn more.

## Setup

Make sure to install dependencies:

```bash
# npm
npm install

# pnpm
pnpm install

# yarn
yarn install

# bun
bun install
```

No environment variables are required. Optionally, create a `.env` file with:

```bash
NUXT_PUBLIC_PIVEAU_HUB_SEARCH_URL=http://localhost:8084
NUXT_PUBLIC_PIVEAU_HUB_REPO_URL=http://localhost:8081
```

The `*PIVEAU_HUB*` can be overridden to point to another Piveau instance. 
By default, `https://piveau-hub-repo.test.ods.zazukoians.org` is used.

## Development Server

Before running the application for development, clone on of the CMS content repositories into `content`.

1. https://github.com/opendata-swiss/opendata-swiss-cms-content
2. https://github.com/opendata-swiss/opendata-swiss-cms-content-int
2. https://github.com/opendata-swiss/opendata-swiss-cms-content-test

For example, to use TEST content:

```bash
git clone https://github.com/opendata-swiss/opendata-swiss-cms-content-test.git content
```

Start the development server on `http://localhost:3000`:

```bash
# npm
npm run dev

# pnpm
pnpm dev

# yarn
yarn dev

# bun
bun run dev
```

## Production

Set the environment variables as needed:

- `NUXT_PUBLIC_IOP_CORE_URL`
- `NUXT_PUBLIC_LOGIN_URL`
- `NUXT_PUBLIC_PIVEAU_HUB_SEARCH_URL`
- `NUXT_PUBLIC_PIVEAU_HUB_REPO_URL`
- `NUXT_MATOMO_URL`
- `NUXT_MATOMO_SITE_ID`

mds-ui needs no secrets.

Build the application for production:

```bash
# npm
npm run build

# pnpm
pnpm build

# yarn
yarn build

# bun
bun run build
```

Locally preview production build:

```bash
# npm
npm run preview

# pnpm
pnpm preview

# yarn
yarn preview

# bun
bun run preview
```

Check out the [deployment documentation](https://nuxt.com/docs/getting-started/deployment) for more information.
