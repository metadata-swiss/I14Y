# Third-Party Licenses

The repository-owned source code is published under the MIT License.

Third-party components remain subject to their own license terms.

## Scope

- Frontend lockfiles:
- frontend/admin-ui/package-lock.json
- frontend/public-ui/package-lock.json
- Backend inventory command: dotnet list backend/i14y.slnx package --include-transitive --format json
- NuGet evidence path types: registration API, nuspec expression, nupkg license-file content classification, URL content classification

## Summary (Direct + Transitive)

- Frontend package rows: 1993
  - Direct: 103
  - Transitive: 1890
- Backend package rows: 202
  - Direct: 48
  - Transitive: 154

Frontend license families:

- MIT (1616)
- ISC (136)
- Apache-2.0 (127)
- BSD-2-Clause (50)
- BSD-3-Clause (28)
- BlueOak-1.0.0 (22)
- 0BSD (4)
- CC-BY-3.0 (2)
- CC-BY-4.0 (2)
- CC0-1.0 (2)
- EPL-2.0 (2)
- Python-2.0 (2)

Backend license families:

- MIT (165)
- Apache-2.0 (31)
- BSD-3-Clause (3)
- PostgreSQL (2)
- MS-PL OR Apache-2.0 (1)

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
- NuGet resolution counts: registration-expression=0, nuspec-expression=187, nupkg-content-classification=10, url-content-classification=5, unresolved=0

## Direct Dependencies

### Frontend

| Component | Project Name | Package | Version | Homepage | SPDX Identifier | License Link | Scope |
| --- | --- | --- | --- | --- | --- | --- | --- |
| admin-ui | @angular-devkit/build-angular | @angular-devkit/build-angular | 21.2.20 | https://www.npmjs.com/package/@angular-devkit/build-angular | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @angular-devkit/core | @angular-devkit/core | 21.2.20 | https://www.npmjs.com/package/@angular-devkit/core | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @angular-devkit/schematics | @angular-devkit/schematics | 21.2.20 | https://www.npmjs.com/package/@angular-devkit/schematics | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @angular-eslint/builder | @angular-eslint/builder | 21.3.1 | https://www.npmjs.com/package/@angular-eslint/builder | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @angular-eslint/eslint-plugin | @angular-eslint/eslint-plugin | 21.3.1 | https://www.npmjs.com/package/@angular-eslint/eslint-plugin | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @angular-eslint/eslint-plugin-template | @angular-eslint/eslint-plugin-template | 21.3.1 | https://www.npmjs.com/package/@angular-eslint/eslint-plugin-template | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @angular-eslint/schematics | @angular-eslint/schematics | 21.3.1 | https://www.npmjs.com/package/@angular-eslint/schematics | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @angular-eslint/template-parser | @angular-eslint/template-parser | 21.3.1 | https://www.npmjs.com/package/@angular-eslint/template-parser | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @angular/build | @angular/build | 21.2.20 | https://www.npmjs.com/package/@angular/build | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @angular/cdk | @angular/cdk | 21.2.14 | https://www.npmjs.com/package/@angular/cdk | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @angular/cli | @angular/cli | 21.2.20 | https://www.npmjs.com/package/@angular/cli | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @angular/common | @angular/common | 21.2.19 | https://www.npmjs.com/package/@angular/common | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @angular/compiler | @angular/compiler | 21.2.19 | https://www.npmjs.com/package/@angular/compiler | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @angular/compiler-cli | @angular/compiler-cli | 21.2.19 | https://www.npmjs.com/package/@angular/compiler-cli | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @angular/core | @angular/core | 21.2.19 | https://www.npmjs.com/package/@angular/core | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @angular/forms | @angular/forms | 21.2.19 | https://www.npmjs.com/package/@angular/forms | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @angular/localize | @angular/localize | 21.2.19 | https://www.npmjs.com/package/@angular/localize | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @angular/material | @angular/material | 21.2.14 | https://www.npmjs.com/package/@angular/material | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @angular/material-moment-adapter | @angular/material-moment-adapter | 21.2.14 | https://www.npmjs.com/package/@angular/material-moment-adapter | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @angular/platform-browser | @angular/platform-browser | 21.2.19 | https://www.npmjs.com/package/@angular/platform-browser | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | @angular/router | @angular/router | 21.2.19 | https://www.npmjs.com/package/@angular/router | MIT | https://spdx.org/licenses/MIT | runtime |
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
| admin-ui | @schematics/angular | @schematics/angular | 21.2.20 | https://www.npmjs.com/package/@schematics/angular | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @types/jasmine | @types/jasmine | 6.0.0 | https://www.npmjs.com/package/@types/jasmine | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @types/node | @types/node | 25.9.5 | https://www.npmjs.com/package/@types/node | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @typescript-eslint/eslint-plugin | @typescript-eslint/eslint-plugin | 8.67.0 | https://www.npmjs.com/package/@typescript-eslint/eslint-plugin | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @typescript-eslint/parser | @typescript-eslint/parser | 8.67.0 | https://www.npmjs.com/package/@typescript-eslint/parser | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | @typescript-eslint/utils | @typescript-eslint/utils | 8.67.0 | https://www.npmjs.com/package/@typescript-eslint/utils | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | date-fns | date-fns | 4.4.0 | https://www.npmjs.com/package/date-fns | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | eslint | eslint | 10.8.1 | https://www.npmjs.com/package/eslint | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | eslint-config-prettier | eslint-config-prettier | 10.1.8 | https://www.npmjs.com/package/eslint-config-prettier | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | eslint-plugin-prettier | eslint-plugin-prettier | 5.5.6 | https://www.npmjs.com/package/eslint-plugin-prettier | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | fuse.js | fuse.js | 7.5.0 | https://www.npmjs.com/package/fuse.js | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | runtime |
| admin-ui | jasmine-core | jasmine-core | 6.1.0 | https://www.npmjs.com/package/jasmine-core | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | js-cookie | js-cookie | 3.0.8 | https://www.npmjs.com/package/js-cookie | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | jwt-decode | jwt-decode | 4.0.0 | https://www.npmjs.com/package/jwt-decode | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | ngx-logger | ngx-logger | 5.0.12 | https://www.npmjs.com/package/ngx-logger | MIT | https://spdx.org/licenses/MIT | runtime |
| admin-ui | oidc-client-ts | oidc-client-ts | 3.5.0 | https://www.npmjs.com/package/oidc-client-ts | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | runtime |
| admin-ui | prettier | prettier | 3.9.6 | https://www.npmjs.com/package/prettier | MIT | https://spdx.org/licenses/MIT | dev |
| admin-ui | rxjs | rxjs | 7.8.2 | https://www.npmjs.com/package/rxjs | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | dev |
| admin-ui | tslib | tslib | 2.8.1 | https://www.npmjs.com/package/tslib | 0BSD | https://spdx.org/licenses/0BSD | runtime |
| admin-ui | typescript | typescript | 5.9.3 | https://www.npmjs.com/package/typescript | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | dev |
| admin-ui | zone.js | zone.js | 0.16.2 | https://www.npmjs.com/package/zone.js | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @angular-devkit/build-angular | @angular-devkit/build-angular | 21.2.20 | https://www.npmjs.com/package/@angular-devkit/build-angular | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @angular-devkit/core | @angular-devkit/core | 21.2.20 | https://www.npmjs.com/package/@angular-devkit/core | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @angular-devkit/schematics | @angular-devkit/schematics | 21.2.20 | https://www.npmjs.com/package/@angular-devkit/schematics | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @angular-eslint/builder | @angular-eslint/builder | 21.3.1 | https://www.npmjs.com/package/@angular-eslint/builder | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @angular-eslint/eslint-plugin | @angular-eslint/eslint-plugin | 21.3.1 | https://www.npmjs.com/package/@angular-eslint/eslint-plugin | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @angular-eslint/eslint-plugin-template | @angular-eslint/eslint-plugin-template | 21.3.1 | https://www.npmjs.com/package/@angular-eslint/eslint-plugin-template | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @angular-eslint/schematics | @angular-eslint/schematics | 21.3.1 | https://www.npmjs.com/package/@angular-eslint/schematics | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @angular-eslint/template-parser | @angular-eslint/template-parser | 21.3.1 | https://www.npmjs.com/package/@angular-eslint/template-parser | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @angular/cdk | @angular/cdk | 21.2.14 | https://www.npmjs.com/package/@angular/cdk | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @angular/cli | @angular/cli | 21.2.20 | https://www.npmjs.com/package/@angular/cli | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @angular/common | @angular/common | 21.2.19 | https://www.npmjs.com/package/@angular/common | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @angular/compiler | @angular/compiler | 21.2.19 | https://www.npmjs.com/package/@angular/compiler | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @angular/compiler-cli | @angular/compiler-cli | 21.2.19 | https://www.npmjs.com/package/@angular/compiler-cli | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @angular/core | @angular/core | 21.2.19 | https://www.npmjs.com/package/@angular/core | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @angular/forms | @angular/forms | 21.2.19 | https://www.npmjs.com/package/@angular/forms | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @angular/localize | @angular/localize | 21.2.19 | https://www.npmjs.com/package/@angular/localize | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @angular/material | @angular/material | 21.2.14 | https://www.npmjs.com/package/@angular/material | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @angular/platform-browser | @angular/platform-browser | 21.2.19 | https://www.npmjs.com/package/@angular/platform-browser | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @angular/router | @angular/router | 21.2.19 | https://www.npmjs.com/package/@angular/router | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @foblex/2d | @foblex/2d | 1.2.2 | https://www.npmjs.com/package/@foblex/2d | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @foblex/drag-toolkit | @foblex/drag-toolkit | 1.1.1 | https://www.npmjs.com/package/@foblex/drag-toolkit | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @foblex/flow | @foblex/flow | 18.5.0 | https://www.npmjs.com/package/@foblex/flow | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @foblex/flow-elk-layout | @foblex/flow-elk-layout | 18.6.1 | https://www.npmjs.com/package/@foblex/flow-elk-layout | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @foblex/mediator | @foblex/mediator | 1.1.3 | https://www.npmjs.com/package/@foblex/mediator | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @foblex/platform | @foblex/platform | 1.0.4 | https://www.npmjs.com/package/@foblex/platform | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @foblex/utils | @foblex/utils | 1.1.1 | https://www.npmjs.com/package/@foblex/utils | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @ngx-translate/core | @ngx-translate/core | 17.0.0 | https://www.npmjs.com/package/@ngx-translate/core | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @oblique/oblique | @oblique/oblique | 15.1.3 | https://www.npmjs.com/package/@oblique/oblique | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | @popperjs/core | @popperjs/core | 2.11.8 | https://www.npmjs.com/package/@popperjs/core | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @schematics/angular | @schematics/angular | 21.2.20 | https://www.npmjs.com/package/@schematics/angular | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @types/node | @types/node | 25.5.2 | https://www.npmjs.com/package/@types/node | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @typescript-eslint/eslint-plugin | @typescript-eslint/eslint-plugin | 8.57.2 | https://www.npmjs.com/package/@typescript-eslint/eslint-plugin | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @typescript-eslint/parser | @typescript-eslint/parser | 8.57.2 | https://www.npmjs.com/package/@typescript-eslint/parser | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | @typescript-eslint/utils | @typescript-eslint/utils | 8.67.0 | https://www.npmjs.com/package/@typescript-eslint/utils | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | ajv | ajv | 8.18.0 | https://www.npmjs.com/package/ajv | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | ajv-formats | ajv-formats | 3.0.1 | https://www.npmjs.com/package/ajv-formats | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | angular-split | angular-split | 20.0.0 | https://www.npmjs.com/package/angular-split | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | runtime |
| public-ui | commander | commander | 14.0.3 | https://www.npmjs.com/package/commander | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | date-fns | date-fns | 4.4.0 | https://www.npmjs.com/package/date-fns | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | eslint | eslint | 10.1.0 | https://www.npmjs.com/package/eslint | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | eslint-config-prettier | eslint-config-prettier | 10.1.8 | https://www.npmjs.com/package/eslint-config-prettier | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | eslint-plugin-prettier | eslint-plugin-prettier | 5.5.6 | https://www.npmjs.com/package/eslint-plugin-prettier | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | fuse.js | fuse.js | 7.5.0 | https://www.npmjs.com/package/fuse.js | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | runtime |
| public-ui | js-cookie | js-cookie | 3.0.8 | https://www.npmjs.com/package/js-cookie | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | jwt-decode | jwt-decode | 4.0.0 | https://www.npmjs.com/package/jwt-decode | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | ngx-logger | ngx-logger | 5.0.12 | https://www.npmjs.com/package/ngx-logger | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | ngx-matomo-client | ngx-matomo-client | 9.0.1 | https://www.npmjs.com/package/ngx-matomo-client | MIT | https://spdx.org/licenses/MIT | runtime |
| public-ui | prettier | prettier | 3.8.5 | https://www.npmjs.com/package/prettier | MIT | https://spdx.org/licenses/MIT | dev |
| public-ui | rxjs | rxjs | 7.8.2 | https://www.npmjs.com/package/rxjs | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | dev |
| public-ui | typescript | typescript | 5.9.3 | https://www.npmjs.com/package/typescript | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | dev |
| public-ui | zone.js | zone.js | 0.16.2 | https://www.npmjs.com/package/zone.js | MIT | https://spdx.org/licenses/MIT | runtime |

### Backend

| Component | Project Name | Package | Version | Homepage | SPDX Identifier | License Link | Scope | Referenced By Projects |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| dotnet | AspNetCore.HealthChecks.UI.Client | AspNetCore.HealthChecks.UI.Client | 9.0.0 | https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | runtime | 4 |
| dotnet | AwesomeAssertions | AwesomeAssertions | 9.4.0 | https://github.com/AwesomeAssertions/AwesomeAssertions | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | runtime | 5 |
| dotnet | AWSSDK.S3 | AWSSDK.S3 | 3.7.402.8 | https://github.com/aws/aws-sdk-net/ | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | runtime | 1 |
| dotnet | Azure.Core | Azure.Core | 1.57.0 | https://github.com/Azure/azure-sdk-for-net/blob/Azure.Core_1.57.0/sdk/core/Azure.Core/README.md | MIT | https://spdx.org/licenses/MIT | runtime | 1 |
| dotnet | Azure.Extensions.AspNetCore.Configuration.Secrets | Azure.Extensions.AspNetCore.Configuration.Secrets | 1.5.1 | https://github.com/Azure/azure-sdk-for-net/blob/Azure.Extensions.AspNetCore.Configuration.Secrets_1.5.1/sdk/extensions/Azure.Extensions.AspNetCore.Configuration.Secrets/README.md | MIT | https://spdx.org/licenses/MIT | runtime | 4 |
| dotnet | Azure.Identity | Azure.Identity | 1.21.0 | https://github.com/Azure/azure-sdk-for-net/blob/Azure.Identity_1.21.0/sdk/identity/Azure.Identity/README.md | MIT | https://spdx.org/licenses/MIT | runtime | 4 |
| dotnet | Azure.Storage.Blobs | Azure.Storage.Blobs | 12.25.0 | https://github.com/Azure/azure-sdk-for-net/blob/Azure.Storage.Blobs_12.25.0/sdk/storage/Azure.Storage.Blobs/README.md | MIT | https://spdx.org/licenses/MIT | runtime | 2 |
| dotnet | coverlet.collector | coverlet.collector | 8.0.0 | https://github.com/coverlet-coverage/coverlet | MIT | https://spdx.org/licenses/MIT | runtime | 4 |
| dotnet | coverlet.msbuild | coverlet.msbuild | 8.0.0 | https://github.com/coverlet-coverage/coverlet | MIT | https://spdx.org/licenses/MIT | runtime | 5 |
| dotnet | CsvHelper | CsvHelper | 33.0.1 | https://joshclose.github.io/CsvHelper/ | MS-PL OR Apache-2.0 | https://licenses.nuget.org/MS-PL%20OR%20Apache-2.0 | runtime | 1 |
| dotnet | dotNetRdf.Client | dotNetRdf.Client | 3.5.1 | https://www.dotnetrdf.org/ | MIT | https://spdx.org/licenses/MIT | runtime | 1 |
| dotnet | FluentValidation.DependencyInjectionExtensions | FluentValidation.DependencyInjectionExtensions | 11.9.2 | https://fluentvalidation.net/ | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | runtime | 2 |
| dotnet | Hellang.Middleware.ProblemDetails | Hellang.Middleware.ProblemDetails | 6.5.1 | https://github.com/khellang/Middleware | MIT | https://spdx.org/licenses/MIT | runtime | 3 |
| dotnet | Lamar.Microsoft.DependencyInjection | Lamar.Microsoft.DependencyInjection | 16.0.0 | https://jasperfx.github.io/lamar | MIT | https://spdx.org/licenses/MIT | runtime | 2 |
| dotnet | Lucene.Net.Analysis.Common | Lucene.Net.Analysis.Common | 4.8.0-beta00017 | https://lucenenet.apache.org/ | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | runtime | 1 |
| dotnet | Lucene.Net.Facet | Lucene.Net.Facet | 4.8.0-beta00017 | https://lucenenet.apache.org/ | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | runtime | 1 |
| dotnet | Lucene.Net.Join | Lucene.Net.Join | 4.8.0-beta00017 | https://lucenenet.apache.org/ | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | runtime | 1 |
| dotnet | Lucene.Net.QueryParser | Lucene.Net.QueryParser | 4.8.0-beta00017 | https://lucenenet.apache.org/ | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | runtime | 1 |
| dotnet | Mapster.DependencyInjection | Mapster.DependencyInjection | 10.0.10 | https://github.com/MapsterMapper/Mapster | MIT | https://spdx.org/licenses/MIT | runtime | 1 |
| dotnet | Mapster | Mapster | 10.0.10 | https://github.com/MapsterMapper/Mapster | MIT | https://spdx.org/licenses/MIT | runtime | 1 |
| dotnet | MediatR | MediatR | 12.5.0 | https://www.nuget.org/packages/mediatr/12.5.0 | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | runtime | 2 |
| dotnet | Microsoft.AspNetCore.Authentication.JwtBearer | Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.4 | https://asp.net/ | MIT | https://spdx.org/licenses/MIT | runtime | 1 |
| dotnet | Microsoft.AspNetCore.Http.Features | Microsoft.AspNetCore.Http.Features | 2.1.1 | https://asp.net/ | Apache-2.0 | https://raw.githubusercontent.com/aspnet/Home/2.0.0/LICENSE.txt | runtime | 1 |
| dotnet | Microsoft.AspNetCore.Mvc.Testing | Microsoft.AspNetCore.Mvc.Testing | 10.0.4 | https://asp.net/ | MIT | https://spdx.org/licenses/MIT | runtime | 1 |
| dotnet | Microsoft.Azure.AppConfiguration.AspNetCore | Microsoft.Azure.AppConfiguration.AspNetCore | 8.5.0 | https://github.com/Azure/AppConfiguration | MIT | https://licenses.nuget.org/MIT | runtime | 4 |
| dotnet | Microsoft.EntityFrameworkCore.Design | Microsoft.EntityFrameworkCore.Design | 10.0.4 | https://docs.microsoft.com/ef/core/ | MIT | https://spdx.org/licenses/MIT | runtime | 1 |
| dotnet | Microsoft.EntityFrameworkCore.InMemory | Microsoft.EntityFrameworkCore.InMemory | 10.0.4 | https://docs.microsoft.com/ef/core/ | MIT | https://spdx.org/licenses/MIT | runtime | 1 |
| dotnet | Microsoft.EntityFrameworkCore.Tools | Microsoft.EntityFrameworkCore.Tools | 10.0.4 | https://docs.microsoft.com/ef/core/ | MIT | https://spdx.org/licenses/MIT | runtime | 1 |
| dotnet | Microsoft.Extensions.Configuration.Abstractions | Microsoft.Extensions.Configuration.Abstractions | 10.0.4 | https://dot.net/ | MIT | https://spdx.org/licenses/MIT | runtime | 1 |
| dotnet | Microsoft.Extensions.Configuration.Binder | Microsoft.Extensions.Configuration.Binder | 10.0.4 | https://dot.net/ | MIT | https://spdx.org/licenses/MIT | runtime | 1 |
| dotnet | Microsoft.Extensions.DependencyInjection.Abstractions | Microsoft.Extensions.DependencyInjection.Abstractions | 10.0.4 | https://dot.net/ | MIT | https://spdx.org/licenses/MIT | runtime | 1 |
| dotnet | Microsoft.Extensions.Hosting.Abstractions | Microsoft.Extensions.Hosting.Abstractions | 10.0.4 | https://dot.net/ | MIT | https://spdx.org/licenses/MIT | runtime | 1 |
| dotnet | Microsoft.Net.Http.Headers | Microsoft.Net.Http.Headers | 10.0.4 | https://asp.net/ | MIT | https://spdx.org/licenses/MIT | runtime | 1 |
| dotnet | Microsoft.NET.Test.Sdk | Microsoft.NET.Test.Sdk | 18.0.1 | https://github.com/microsoft/vstest | MIT | https://spdx.org/licenses/MIT | runtime | 9 |
| dotnet | Newtonsoft.Json | Newtonsoft.Json | 13.0.3 | https://www.newtonsoft.com/json | MIT | https://spdx.org/licenses/MIT | runtime | 1 |
| dotnet | Npgsql.EntityFrameworkCore.PostgreSQL | Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.2 | https://github.com/npgsql/efcore.pg | PostgreSQL | https://spdx.org/licenses/PostgreSQL | runtime | 1 |
| dotnet | NSubstitute | NSubstitute | 5.3.0 | https://nsubstitute.github.io/ | BSD-3-Clause | https://spdx.org/licenses/BSD-3-Clause | runtime | 6 |
| dotnet | NSwag.CodeGeneration.CSharp | NSwag.CodeGeneration.CSharp | 14.1.0 | http://nswag.org/ | MIT | https://spdx.org/licenses/MIT | runtime | 1 |
| dotnet | NSwag.CodeGeneration.TypeScript | NSwag.CodeGeneration.TypeScript | 14.1.0 | http://nswag.org/ | MIT | https://spdx.org/licenses/MIT | runtime | 1 |
| dotnet | NUnit.Analyzers | NUnit.Analyzers | 4.11.2 | https://github.com/nunit/nunit.analyzers | MIT | https://spdx.org/licenses/MIT | runtime | 1 |
| dotnet | NUnit | NUnit | 4.6.1 | https://nunit.org/ | MIT | https://spdx.org/licenses/MIT | runtime | 9 |
| dotnet | NUnit3TestAdapter | NUnit3TestAdapter | 6.1.0 | https://docs.nunit.org/articles/vs-test-adapter/Index.html | MIT | https://spdx.org/licenses/MIT | runtime | 9 |
| dotnet | Serilog.AspNetCore | Serilog.AspNetCore | 9.0.0 | https://github.com/serilog/serilog-aspnetcore | Apache-2.0 | https://spdx.org/licenses/Apache-2.0 | runtime | 1 |
| dotnet | Slugify.Core | Slugify.Core | 5.1.1 | https://github.com/ctolkien/Slugify | MIT | https://spdx.org/licenses/MIT | runtime | 1 |
| dotnet | Swashbuckle.AspNetCore.Filters | Swashbuckle.AspNetCore.Filters | 8.0.3 | https://github.com/mattfrear/Swashbuckle.AspNetCore.Filters | MIT | https://spdx.org/licenses/MIT | runtime | 1 |
| dotnet | Swashbuckle.AspNetCore.SwaggerGen | Swashbuckle.AspNetCore.SwaggerGen | 6.7.3 | https://github.com/domaindrivendev/Swashbuckle.AspNetCore | MIT | https://spdx.org/licenses/MIT | runtime | 2 |
| dotnet | Swashbuckle.AspNetCore | Swashbuckle.AspNetCore | 6.7.3 | https://github.com/domaindrivendev/Swashbuckle.AspNetCore | MIT | https://spdx.org/licenses/MIT | runtime | 4 |
| dotnet | System.Linq.Async | System.Linq.Async | 7.0.0 | https://github.com/dotnet/reactive | MIT | https://spdx.org/licenses/MIT | runtime | 1 |

