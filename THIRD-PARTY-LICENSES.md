# Third-Party Licenses

The repository-owned source code is published under the MIT License.

Third-party components remain subject to their own license terms.

## Scope

- Frontend lockfiles: frontend/admin-ui/package-lock.json, frontend/public-ui/package-lock.json
- Backend inventory command: dotnet list backend/i14y.slnx package --include-transitive --format json
- NuGet evidence path types: registration API, nuspec expression, nupkg license-file content classification, URL content classification

## Summary (Direct + Transitive)

- Frontend package rows: 2109
  - Direct: 103
  - Transitive: 2006
- Backend package rows: 205
  - Direct: 50
  - Transitive: 155

Frontend license families:

- 0BSD (4)
- Apache-2.0 (133)
- BlueOak-1.0.0 (34)
- BSD-2-Clause (50)
- BSD-3-Clause (32)
- CC0-1.0 (2)
- CC-BY-3.0 (2)
- CC-BY-4.0 (2)
- EPL-2.0 (2)
- ISC (158)
- MIT (1688)
- Python-2.0 (2)

Backend license families:

- Apache-2.0 (26)
- BSD-3-Clause (3)
- MIT (173)
- MS-PL OR Apache-2.0 (1)
- PostgreSQL (2)

## Forbidden Licenses Policy

Blocked by policy (runtime and development scopes):

- GPL-2.0
- GPL-3.0
- LGPL-2.1
- LGPL-3.0
- AGPL-3.0
- SSPL-1.0
- MPL-2.0

## Blocked Findings

- none

## Key Notes

- Internal override: none applied.
- Frontend unresolved licenses: 0
- Backend unresolved licenses: 0
- NuGet resolution counts: registration-expression=0, nuspec-expression=190, nupkg-content-classification=10, url-content-classification=5, unresolved=0

## Package Tables

- Direct dependencies table: THIRD-PARTY-LICENSES.md (section "Direct Dependencies")
- Transitive dependencies table: THIRD-PARTY-TRANSITIVE-LICENSES.md

## Publication Note

Publishing this repository under MIT applies only to repository-owned code.

Redistribution that includes third-party dependencies remains subject to the obligations of their respective licenses.

As of this inventory snapshot, blocked-license policy checks are green.

## Direct Dependencies

### Frontend

| App | Project Name | Package | Version | Homepage | SPDX Identifier | License Link | Scope |
| --- | --- | --- | --- | --- | --- | --- | --- |
| admin-ui | @angular/build | @angular/build | 21.2.18 | https://www.npmjs.com/package/@angular/build | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @angular/cdk | @angular/cdk | 21.2.14 | https://www.npmjs.com/package/@angular/cdk | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @angular/cli | @angular/cli | 21.2.18 | https://www.npmjs.com/package/@angular/cli | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @angular/common | @angular/common | 21.2.17 | https://www.npmjs.com/package/@angular/common | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @angular/compiler | @angular/compiler | 21.2.17 | https://www.npmjs.com/package/@angular/compiler | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @angular/compiler-cli | @angular/compiler-cli | 21.2.17 | https://www.npmjs.com/package/@angular/compiler-cli | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @angular/core | @angular/core | 21.2.17 | https://www.npmjs.com/package/@angular/core | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @angular/forms | @angular/forms | 21.2.17 | https://www.npmjs.com/package/@angular/forms | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @angular/localize | @angular/localize | 21.2.17 | https://www.npmjs.com/package/@angular/localize | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @angular/material | @angular/material | 21.2.14 | https://www.npmjs.com/package/@angular/material | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @angular/material-moment-adapter | @angular/material-moment-adapter | 21.2.14 | https://www.npmjs.com/package/@angular/material-moment-adapter | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @angular/platform-browser | @angular/platform-browser | 21.2.17 | https://www.npmjs.com/package/@angular/platform-browser | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @angular/router | @angular/router | 21.2.17 | https://www.npmjs.com/package/@angular/router | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @angular-devkit/build-angular | @angular-devkit/build-angular | 21.2.18 | https://www.npmjs.com/package/@angular-devkit/build-angular | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @angular-devkit/core | @angular-devkit/core | 21.2.18 | https://www.npmjs.com/package/@angular-devkit/core | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @angular-devkit/schematics | @angular-devkit/schematics | 21.2.18 | https://www.npmjs.com/package/@angular-devkit/schematics | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @angular-eslint/builder | @angular-eslint/builder | 21.3.1 | https://www.npmjs.com/package/@angular-eslint/builder | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @angular-eslint/eslint-plugin | @angular-eslint/eslint-plugin | 21.3.1 | https://www.npmjs.com/package/@angular-eslint/eslint-plugin | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @angular-eslint/eslint-plugin-template | @angular-eslint/eslint-plugin-template | 21.3.1 | https://www.npmjs.com/package/@angular-eslint/eslint-plugin-template | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @angular-eslint/schematics | @angular-eslint/schematics | 21.3.1 | https://www.npmjs.com/package/@angular-eslint/schematics | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @angular-eslint/template-parser | @angular-eslint/template-parser | 21.3.1 | https://www.npmjs.com/package/@angular-eslint/template-parser | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @foblex/2d | @foblex/2d | 1.2.2 | https://www.npmjs.com/package/@foblex/2d | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @foblex/drag-toolkit | @foblex/drag-toolkit | 1.1.1 | https://www.npmjs.com/package/@foblex/drag-toolkit | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @foblex/flow | @foblex/flow | 18.5.0 | https://www.npmjs.com/package/@foblex/flow | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @foblex/flow-elk-layout | @foblex/flow-elk-layout | 18.6.1 | https://www.npmjs.com/package/@foblex/flow-elk-layout | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @foblex/mediator | @foblex/mediator | 1.1.3 | https://www.npmjs.com/package/@foblex/mediator | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @foblex/platform | @foblex/platform | 1.0.4 | https://www.npmjs.com/package/@foblex/platform | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @foblex/utils | @foblex/utils | 1.1.1 | https://www.npmjs.com/package/@foblex/utils | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @ngx-translate/core | @ngx-translate/core | 17.0.0 | https://www.npmjs.com/package/@ngx-translate/core | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @oblique/oblique | @oblique/oblique | 15.1.3 | https://www.npmjs.com/package/@oblique/oblique | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @popperjs/core | @popperjs/core | 2.11.8 | https://www.npmjs.com/package/@popperjs/core | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @schematics/angular | @schematics/angular | 21.2.18 | https://www.npmjs.com/package/@schematics/angular | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @types/jasmine | @types/jasmine | 6.0.0 | https://www.npmjs.com/package/@types/jasmine | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @types/node | @types/node | 25.9.3 | https://www.npmjs.com/package/@types/node | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @typescript-eslint/eslint-plugin | @typescript-eslint/eslint-plugin | 8.61.1 | https://www.npmjs.com/package/@typescript-eslint/eslint-plugin | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @typescript-eslint/parser | @typescript-eslint/parser | 8.61.1 | https://www.npmjs.com/package/@typescript-eslint/parser | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @typescript-eslint/utils | @typescript-eslint/utils | 8.61.1 | https://www.npmjs.com/package/@typescript-eslint/utils | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | date-fns | date-fns | 4.4.0 | https://www.npmjs.com/package/date-fns | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | eslint | eslint | 10.5.0 | https://www.npmjs.com/package/eslint | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | eslint-config-prettier | eslint-config-prettier | 10.1.8 | https://www.npmjs.com/package/eslint-config-prettier | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | eslint-plugin-prettier | eslint-plugin-prettier | 5.5.6 | https://www.npmjs.com/package/eslint-plugin-prettier | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | fuse.js | fuse.js | 7.4.2 | https://www.npmjs.com/package/fuse.js | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | runtime |
| admin-ui | jasmine-core | jasmine-core | 6.1.0 | https://www.npmjs.com/package/jasmine-core | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | js-cookie | js-cookie | 3.0.8 | https://www.npmjs.com/package/js-cookie | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | jwt-decode | jwt-decode | 4.0.0 | https://www.npmjs.com/package/jwt-decode | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | ngx-logger | ngx-logger | 5.0.12 | https://www.npmjs.com/package/ngx-logger | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | oidc-client-ts | oidc-client-ts | 3.5.0 | https://www.npmjs.com/package/oidc-client-ts | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | runtime |
| admin-ui | prettier | prettier | 3.8.4 | https://www.npmjs.com/package/prettier | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | rxjs | rxjs | 7.8.2 | https://www.npmjs.com/package/rxjs | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | dev |
| admin-ui | tslib | tslib | 2.8.1 | https://www.npmjs.com/package/tslib | 0BSD | https://spdx.org/licenses/0BSD | runtime |
| admin-ui | typescript | typescript | 5.9.3 | https://www.npmjs.com/package/typescript | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | dev |
| admin-ui | zone.js | zone.js | 0.16.2 | https://www.npmjs.com/package/zone.js | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @angular/cdk | @angular/cdk | 21.2.14 | https://www.npmjs.com/package/@angular/cdk | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @angular/cli | @angular/cli | 21.2.18 | https://www.npmjs.com/package/@angular/cli | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @angular/common | @angular/common | 21.2.17 | https://www.npmjs.com/package/@angular/common | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @angular/compiler | @angular/compiler | 21.2.17 | https://www.npmjs.com/package/@angular/compiler | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @angular/compiler-cli | @angular/compiler-cli | 21.2.17 | https://www.npmjs.com/package/@angular/compiler-cli | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @angular/core | @angular/core | 21.2.17 | https://www.npmjs.com/package/@angular/core | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @angular/forms | @angular/forms | 21.2.17 | https://www.npmjs.com/package/@angular/forms | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @angular/localize | @angular/localize | 21.2.17 | https://www.npmjs.com/package/@angular/localize | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @angular/material | @angular/material | 21.2.14 | https://www.npmjs.com/package/@angular/material | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @angular/platform-browser | @angular/platform-browser | 21.2.17 | https://www.npmjs.com/package/@angular/platform-browser | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @angular/router | @angular/router | 21.2.17 | https://www.npmjs.com/package/@angular/router | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @angular-devkit/build-angular | @angular-devkit/build-angular | 21.2.18 | https://www.npmjs.com/package/@angular-devkit/build-angular | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @angular-devkit/core | @angular-devkit/core | 21.2.18 | https://www.npmjs.com/package/@angular-devkit/core | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @angular-devkit/schematics | @angular-devkit/schematics | 21.2.18 | https://www.npmjs.com/package/@angular-devkit/schematics | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @angular-eslint/builder | @angular-eslint/builder | 21.3.1 | https://www.npmjs.com/package/@angular-eslint/builder | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @angular-eslint/eslint-plugin | @angular-eslint/eslint-plugin | 21.3.1 | https://www.npmjs.com/package/@angular-eslint/eslint-plugin | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @angular-eslint/eslint-plugin-template | @angular-eslint/eslint-plugin-template | 21.3.1 | https://www.npmjs.com/package/@angular-eslint/eslint-plugin-template | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @angular-eslint/schematics | @angular-eslint/schematics | 21.3.1 | https://www.npmjs.com/package/@angular-eslint/schematics | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @angular-eslint/template-parser | @angular-eslint/template-parser | 21.3.1 | https://www.npmjs.com/package/@angular-eslint/template-parser | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @foblex/2d | @foblex/2d | 1.2.2 | https://www.npmjs.com/package/@foblex/2d | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @foblex/drag-toolkit | @foblex/drag-toolkit | 1.1.1 | https://www.npmjs.com/package/@foblex/drag-toolkit | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @foblex/flow | @foblex/flow | 18.5.0 | https://www.npmjs.com/package/@foblex/flow | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @foblex/flow-elk-layout | @foblex/flow-elk-layout | 18.6.0 | https://www.npmjs.com/package/@foblex/flow-elk-layout | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @foblex/mediator | @foblex/mediator | 1.1.3 | https://www.npmjs.com/package/@foblex/mediator | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @foblex/platform | @foblex/platform | 1.0.4 | https://www.npmjs.com/package/@foblex/platform | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @foblex/utils | @foblex/utils | 1.1.1 | https://www.npmjs.com/package/@foblex/utils | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @ngx-translate/core | @ngx-translate/core | 17.0.0 | https://www.npmjs.com/package/@ngx-translate/core | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @oblique/oblique | @oblique/oblique | 15.1.3 | https://www.npmjs.com/package/@oblique/oblique | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @popperjs/core | @popperjs/core | 2.11.8 | https://www.npmjs.com/package/@popperjs/core | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @schematics/angular | @schematics/angular | 21.2.18 | https://www.npmjs.com/package/@schematics/angular | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @types/node | @types/node | 25.5.2 | https://www.npmjs.com/package/@types/node | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @typescript-eslint/eslint-plugin | @typescript-eslint/eslint-plugin | 8.57.2 | https://www.npmjs.com/package/@typescript-eslint/eslint-plugin | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @typescript-eslint/parser | @typescript-eslint/parser | 8.57.2 | https://www.npmjs.com/package/@typescript-eslint/parser | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @typescript-eslint/utils | @typescript-eslint/utils | 8.59.2 | https://www.npmjs.com/package/@typescript-eslint/utils | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | ajv | ajv | 8.18.0 | https://www.npmjs.com/package/ajv | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | ajv-formats | ajv-formats | 3.0.1 | https://www.npmjs.com/package/ajv-formats | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | angular-split | angular-split | 20.0.0 | https://www.npmjs.com/package/angular-split | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | runtime |
| public-ui | commander | commander | 14.0.3 | https://www.npmjs.com/package/commander | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | date-fns | date-fns | 4.4.0 | https://www.npmjs.com/package/date-fns | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | eslint | eslint | 10.1.0 | https://www.npmjs.com/package/eslint | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | eslint-config-prettier | eslint-config-prettier | 10.1.8 | https://www.npmjs.com/package/eslint-config-prettier | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | eslint-plugin-prettier | eslint-plugin-prettier | 5.5.5 | https://www.npmjs.com/package/eslint-plugin-prettier | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | fuse.js | fuse.js | 7.3.0 | https://www.npmjs.com/package/fuse.js | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | runtime |
| public-ui | js-cookie | js-cookie | 3.0.8 | https://www.npmjs.com/package/js-cookie | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | jwt-decode | jwt-decode | 4.0.0 | https://www.npmjs.com/package/jwt-decode | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | ngx-logger | ngx-logger | 5.0.12 | https://www.npmjs.com/package/ngx-logger | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | ngx-matomo-client | ngx-matomo-client | 9.0.1 | https://www.npmjs.com/package/ngx-matomo-client | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | prettier | prettier | 3.8.3 | https://www.npmjs.com/package/prettier | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | rxjs | rxjs | 7.8.2 | https://www.npmjs.com/package/rxjs | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | dev |
| public-ui | typescript | typescript | 5.9.3 | https://www.npmjs.com/package/typescript | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | dev |
| public-ui | zone.js | zone.js | 0.16.1 | https://www.npmjs.com/package/zone.js | MIT | https://spdx.org/licenses/MIT | runtime |

### Backend

| Component | Project Name | Package | Version | Homepage | SPDX Identifier | License Link | Referenced By Projects |
| --- | --- | --- | --- | --- | --- | --- | --- |
| dotnet | AspNetCore.HealthChecks.UI.Client | AspNetCore.HealthChecks.UI.Client | 9.0.0 | https://www.nuget.org/packages/aspnetcore.healthchecks.ui.client/9.0.0 | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | 4 |
| dotnet | AwesomeAssertions | AwesomeAssertions | 9.4.0 | https://www.nuget.org/packages/awesomeassertions/9.4.0 | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | 5 |
| dotnet | AWSSDK.S3 | AWSSDK.S3 | 3.7.402.8 | https://www.nuget.org/packages/awssdk.s3/3.7.402.8 | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | 1 |
| dotnet | Azure.Core | Azure.Core | 1.57.0 | https://www.nuget.org/packages/azure.core/1.57.0 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | Azure.Extensions.AspNetCore.Configuration.Secrets | Azure.Extensions.AspNetCore.Configuration.Secrets | 1.5.1 | https://www.nuget.org/packages/azure.extensions.aspnetcore.configuration.secrets/1.5.1 | MIT | https://spdx.org/licenses/MIT | 4 |
| dotnet | Azure.Identity | Azure.Identity | 1.21.0 | https://www.nuget.org/packages/azure.identity/1.21.0 | MIT | https://spdx.org/licenses/MIT | 4 |
| dotnet | Azure.Storage.Blobs | Azure.Storage.Blobs | 12.25.0 | https://www.nuget.org/packages/azure.storage.blobs/12.25.0 | MIT | https://spdx.org/licenses/MIT | 2 |
| dotnet | coverlet.collector | coverlet.collector | 8.0.0 | https://www.nuget.org/packages/coverlet.collector/8.0.0 | MIT | https://spdx.org/licenses/MIT | 4 |
| dotnet | coverlet.msbuild | coverlet.msbuild | 8.0.0 | https://www.nuget.org/packages/coverlet.msbuild/8.0.0 | MIT | https://spdx.org/licenses/MIT | 5 |
| dotnet | CsvHelper | CsvHelper | 33.0.1 | https://www.nuget.org/packages/csvhelper/33.0.1 | MS-PL OR Apache-2.0 | https://www.nuget.org/packages/csvhelper/33.0.1 | 1 |
| dotnet | dotNetRdf.Client | dotNetRdf.Client | 3.5.1 | https://www.nuget.org/packages/dotnetrdf.client/3.5.1 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | Elastic.Clients.Elasticsearch | Elastic.Clients.Elasticsearch | 9.4.2 | https://www.nuget.org/packages/elastic.clients.elasticsearch/9.4.2 | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | 1 |
| dotnet | FluentValidation.DependencyInjectionExtensions | FluentValidation.DependencyInjectionExtensions | 11.9.2 | https://www.nuget.org/packages/fluentvalidation.dependencyinjectionextensions/11.9.2 | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | 2 |
| dotnet | Hellang.Middleware.ProblemDetails | Hellang.Middleware.ProblemDetails | 6.5.1 | https://www.nuget.org/packages/hellang.middleware.problemdetails/6.5.1 | MIT | https://spdx.org/licenses/MIT | 3 |
| dotnet | Lamar.Microsoft.DependencyInjection | Lamar.Microsoft.DependencyInjection | 16.0.0 | https://www.nuget.org/packages/lamar.microsoft.dependencyinjection/16.0.0 | MIT | https://spdx.org/licenses/MIT | 2 |
| dotnet | Lucene.Net.Analysis.Common | Lucene.Net.Analysis.Common | 4.8.0-beta00017 | https://www.nuget.org/packages/lucene.net.analysis.common/4.8.0-beta00017 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | Lucene.Net.Facet | Lucene.Net.Facet | 4.8.0-beta00017 | https://www.nuget.org/packages/lucene.net.facet/4.8.0-beta00017 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | Lucene.Net.Join | Lucene.Net.Join | 4.8.0-beta00017 | https://www.nuget.org/packages/lucene.net.join/4.8.0-beta00017 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | Lucene.Net.QueryParser | Lucene.Net.QueryParser | 4.8.0-beta00017 | https://www.nuget.org/packages/lucene.net.queryparser/4.8.0-beta00017 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | Mapster | Mapster | 10.0.10 | https://www.nuget.org/packages/mapster/10.0.10 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | Mapster.DependencyInjection | Mapster.DependencyInjection | 10.0.10 | https://www.nuget.org/packages/mapster.dependencyinjection/10.0.10 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | MediatR | MediatR | 12.5.0 | https://www.nuget.org/packages/mediatr/12.5.0 | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | 2 |
| dotnet | Microsoft.AspNetCore.Authentication.JwtBearer | Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.4 | https://www.nuget.org/packages/microsoft.aspnetcore.authentication.jwtbearer/10.0.4 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | Microsoft.AspNetCore.Http.Features | Microsoft.AspNetCore.Http.Features | 2.1.1 | https://www.nuget.org/packages/microsoft.aspnetcore.http.features/2.1.1 | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | 1 |
| dotnet | Microsoft.AspNetCore.Mvc.Testing | Microsoft.AspNetCore.Mvc.Testing | 10.0.4 | https://www.nuget.org/packages/microsoft.aspnetcore.mvc.testing/10.0.4 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | Microsoft.Azure.AppConfiguration.AspNetCore | Microsoft.Azure.AppConfiguration.AspNetCore | 8.5.0 | https://www.nuget.org/packages/microsoft.azure.appconfiguration.aspnetcore/8.5.0 | MIT | https://spdx.org/licenses/MIT | 4 |
| dotnet | Microsoft.EntityFrameworkCore.Design | Microsoft.EntityFrameworkCore.Design | 10.0.4 | https://www.nuget.org/packages/microsoft.entityframeworkcore.design/10.0.4 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | Microsoft.EntityFrameworkCore.InMemory | Microsoft.EntityFrameworkCore.InMemory | 10.0.4 | https://www.nuget.org/packages/microsoft.entityframeworkcore.inmemory/10.0.4 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | Microsoft.EntityFrameworkCore.Tools | Microsoft.EntityFrameworkCore.Tools | 10.0.4 | https://www.nuget.org/packages/microsoft.entityframeworkcore.tools/10.0.4 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | Microsoft.Extensions.Configuration.Abstractions | Microsoft.Extensions.Configuration.Abstractions | 10.0.4 | https://www.nuget.org/packages/microsoft.extensions.configuration.abstractions/10.0.4 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | Microsoft.Extensions.Configuration.Binder | Microsoft.Extensions.Configuration.Binder | 10.0.4 | https://www.nuget.org/packages/microsoft.extensions.configuration.binder/10.0.4 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | Microsoft.Extensions.DependencyInjection.Abstractions | Microsoft.Extensions.DependencyInjection.Abstractions | 10.0.4 | https://www.nuget.org/packages/microsoft.extensions.dependencyinjection.abstractions/10.0.4 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | Microsoft.Extensions.Hosting.Abstractions | Microsoft.Extensions.Hosting.Abstractions | 10.0.4 | https://www.nuget.org/packages/microsoft.extensions.hosting.abstractions/10.0.4 | MIT | https://spdx.org/licenses/MIT | 2 |
| dotnet | Microsoft.Extensions.Options | Microsoft.Extensions.Options | 10.0.4 | https://www.nuget.org/packages/microsoft.extensions.options/10.0.4 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | Microsoft.Net.Http.Headers | Microsoft.Net.Http.Headers | 10.0.4 | https://www.nuget.org/packages/microsoft.net.http.headers/10.0.4 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | Microsoft.NET.Test.Sdk | Microsoft.NET.Test.Sdk | 18.0.1 | https://www.nuget.org/packages/microsoft.net.test.sdk/18.0.1 | MIT | https://spdx.org/licenses/MIT | 9 |
| dotnet | Newtonsoft.Json | Newtonsoft.Json | 13.0.3 | https://www.nuget.org/packages/newtonsoft.json/13.0.3 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | Npgsql.EntityFrameworkCore.PostgreSQL | Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.2 | https://www.nuget.org/packages/npgsql.entityframeworkcore.postgresql/10.0.2 | PostgreSQL | https://spdx.org/licenses/PostgreSQL | 1 |
| dotnet | NSubstitute | NSubstitute | 5.3.0 | https://www.nuget.org/packages/nsubstitute/5.3.0 | BSD-3-Clause | https://spdx.org/licenses/BSD-3-Clause | 6 |
| dotnet | NSwag.CodeGeneration.CSharp | NSwag.CodeGeneration.CSharp | 14.1.0 | https://www.nuget.org/packages/nswag.codegeneration.csharp/14.1.0 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | NSwag.CodeGeneration.TypeScript | NSwag.CodeGeneration.TypeScript | 14.1.0 | https://www.nuget.org/packages/nswag.codegeneration.typescript/14.1.0 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | NUnit | NUnit | 4.6.1 | https://www.nuget.org/packages/nunit/4.6.1 | MIT | https://spdx.org/licenses/MIT | 9 |
| dotnet | NUnit.Analyzers | NUnit.Analyzers | 4.11.2 | https://www.nuget.org/packages/nunit.analyzers/4.11.2 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | NUnit3TestAdapter | NUnit3TestAdapter | 6.1.0 | https://www.nuget.org/packages/nunit3testadapter/6.1.0 | MIT | https://spdx.org/licenses/MIT | 9 |
| dotnet | Serilog.AspNetCore | Serilog.AspNetCore | 9.0.0 | https://www.nuget.org/packages/serilog.aspnetcore/9.0.0 | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | 1 |
| dotnet | Slugify.Core | Slugify.Core | 5.1.1 | https://www.nuget.org/packages/slugify.core/5.1.1 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | Swashbuckle.AspNetCore | Swashbuckle.AspNetCore | 6.7.3 | https://www.nuget.org/packages/swashbuckle.aspnetcore/6.7.3 | MIT | https://spdx.org/licenses/MIT | 4 |
| dotnet | Swashbuckle.AspNetCore.Filters | Swashbuckle.AspNetCore.Filters | 8.0.3 | https://www.nuget.org/packages/swashbuckle.aspnetcore.filters/8.0.3 | MIT | https://spdx.org/licenses/MIT | 1 |
| dotnet | Swashbuckle.AspNetCore.SwaggerGen | Swashbuckle.AspNetCore.SwaggerGen | 6.7.3 | https://www.nuget.org/packages/swashbuckle.aspnetcore.swaggergen/6.7.3 | MIT | https://spdx.org/licenses/MIT | 2 |
| dotnet | System.Linq.Async | System.Linq.Async | 7.0.0 | https://www.nuget.org/packages/system.linq.async/7.0.0 | MIT | https://spdx.org/licenses/MIT | 1 |
