import { writeFileSync } from "node:fs";

const config = {
  ENV_NAME: process.env.ENV_NAME ?? "LOCAL-DOCKER",
  RELEASE_VERSION: process.env.RELEASE_VERSION ?? "1.0 LOCAL DOCKER",
  KEYCLOAK_CLIENT_ID: process.env.KEYCLOAK_CLIENT_ID ?? "BFS-i14y",
  KEYCLOAK_AUTHORITY_URL: process.env.KEYCLOAK_AUTHORITY_URL ?? "http://localhost:8080/realms/i14y-local",
  API_BASE_URL: process.env.API_BASE_URL ?? "http://localhost:8001",
  PARTNER_API_BASE_URL: process.env.PARTNER_API_BASE_URL ?? "http://localhost:8002/api",
  I14Y_PUBLIC_ROUTE: process.env.I14Y_PUBLIC_ROUTE ?? "http://localhost:5022",
  I14Y_IRI_URL: process.env.I14Y_IRI_URL ?? "http://localhost:8003"
};

writeFileSync("src/assets/config/appconfig.json", `${JSON.stringify(config, null, 2)}\n`);
