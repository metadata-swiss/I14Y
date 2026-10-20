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

No environment variables are required. By default, mds-ui uses the iop-core of the DEV environment. To use another
one, create a `.env` file with:

```bash
NUXT_PUBLIC_IOP_CORE_URL=https://core.i14y.d.c.bfs.admin.ch
```

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

- `NUXT_PUBLIC_IOP_CORE_URL`: iop-core. The server calls it directly; the browser calls it through the server at
  `/input-backend/`, see [api-client/README.md](api-client/README.md).
- `NUXT_PUBLIC_LOGIN_URL`: the I14Y input portal, opened by the login button.
- `NUXT_PUBLIC_IRI_URL`: the base URL of the IRIs of I14Y resources, for example the IRI of an organization,
  `{base}/agent/{identifier}`. Production is `https://register.ld.admin.ch/i14y`.
- `NUXT_PUBLIC_DATA_PREVIEW_URL`: the data preview service of tabular distributions. Empty: no preview.
- `NUXT_PUBLIC_MATOMO_URL`, `NUXT_PUBLIC_MATOMO_SITE_ID`: Matomo. Empty: no tracking.

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
