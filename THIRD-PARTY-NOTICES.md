# Third-Party Notices

OpenCashFlow depends on open-source frameworks and libraries distributed under their respective licenses. This file is a release checklist and attribution index for self-hosted/open-source distribution.

## Server-side packages

- .NET / ASP.NET Core: Microsoft open-source licenses.
- Entity Framework Core: MIT.
- Npgsql Entity Framework provider: PostgreSQL license.
- AutoMapper: MIT.
- Serilog and sinks: Apache-2.0 / MIT depending on package.
- MailKit and MimeKit: MIT.
- Stripe.net: not part of the community core runtime. Any future optional billing module must document its own dependency and license notices.
- Asp.Versioning: MIT.
- Sentry SDK, if enabled: MIT.

## Runtime Dependency Baseline

OpenCashFlow targets `.NET 10` and uses the .NET 10 Microsoft package line for ASP.NET Core, Entity Framework Core and Microsoft.Extensions packages.

Current release-sensitive packages include:

- `Npgsql.EntityFrameworkCore.PostgreSQL` `10.0.2`.
- `System.IdentityModel.Tokens.Jwt` `8.19.1`.
- `System.Linq.Dynamic.Core` `1.7.2`.
- `Microsoft.OpenApi` `2.10.0`.
- `Swashbuckle.AspNetCore` `10.2.3`.
- `MailKit` / `MimeKit` `4.17.0`.

Major upgrades not required by .NET 10 compatibility, such as `Polly` 8, `Sentry` 6 and `Asp.Versioning` 10, should be reviewed separately before release.

## Client-side libraries and assets

The web projects include vendored assets under `wwwroot/libs`. Before publishing a release archive, verify each asset directory against its upstream package metadata. Current expected major families include:

- Bootstrap ecosystem packages: MIT.
- jQuery ecosystem packages: MIT.
- Tabler UI assets: MIT, subject to upstream asset notices.
- Choices/select/dropzone/chart/vector-map/editor/player libraries: verify package-level license files before redistribution.

Vendored frontend `package.json` files are intentionally not kept in `wwwroot/libs` because OpenCashFlow does not run npm install or ship those upstream development dependency graphs. Keep runtime JS/CSS assets and upstream license/readme files, but do not reintroduce inactive npm manifests unless the WebApp gains an actual npm build pipeline.

## Redistributable asset rule

Do not commit or ship commercial themes, fonts, stock images, icons, or templates unless their license explicitly allows redistribution in an open-source repository. If an asset cannot be verified, replace it with an open-license equivalent or remove it from the release package.

## Release checklist

- Run `dotnet list package --include-transitive` and review license metadata for each package.
- Run `dotnet list OpenCashFlow.sln package --vulnerable --include-transitive` and resolve or document any advisory before release.
- Run an asset inventory over `src/**/wwwroot` and ensure each vendored directory has a known upstream license.
- Keep optional SaaS/billing dependencies documented as optional, not required for the community self-hosted core.
