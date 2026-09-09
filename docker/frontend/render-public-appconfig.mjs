import {writeFileSync} from "node:fs";

const parseBool = (value, fallback) => {
  if (value === undefined) {
    return fallback;
  }

  return value.toLowerCase() === "true";
};

const config = {
  ENV_NAME: process.env.ENV_NAME ?? "LOCAL-DOCKER",
  ADMIN_APP_ROUTE: process.env.ADMIN_APP_ROUTE ?? "http://localhost:4200",
  ANALYTICS_SITE_ID: process.env.ANALYTICS_SITE_ID ?? null,
  IOP_ADMIN_API_BASE_URL:
    process.env.IOP_ADMIN_API_BASE_URL ?? "http://localhost:5010",
  PUBLIC_API_BASE_URL:
    process.env.PUBLIC_API_BASE_URL ?? "http://localhost:5050/api/public/v1",
  DASHBOARD_URL:
    process.env.DASHBOARD_URL ?? "",
  SHOW_INFO_VIDEO: parseBool(process.env.SHOW_INFO_VIDEO, true),
  LINK_HANDBOOK:
    process.env.LINK_HANDBOOK ?? "",
  I14Y_IRI_URL: process.env.I14Y_IRI_URL ?? "http://localhost:5288",
};

writeFileSync(
  "src/assets/config/appconfig.json",
  `${JSON.stringify(config, null, 2)}\n`
);
