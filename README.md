# KeyBasedAuthenticationTemplate_API

A production-style **.NET 10 Web API template** that uses **API keys as its only authentication method**:

- **API keys** → long-lived, header-based credentials (`X-Api-Key`), shown once, hashed at rest
- **Roles table** → each key holds any number of roles (`ApiKeyRoles` join table); authorization is plain role-based policies
- **Owner username** → each key carries a free-text `OwnerUsername`; products are scoped to it

There are no user accounts, passwords or JWTs. Keys holding the `Admin` role manage all other keys and roles. The template also demonstrates a two-database setup (SQL Server for keys/roles, PostgreSQL for business data), EF Core and Dapper read paths side by side, and a layered architecture meant to be easy to extend for a real project.

This README is a guide: it walks through what the project does, how to get it running locally, and how to use every endpoint.

---

## Table of contents

- [What this demonstrates](#what-this-demonstrates)
- [Tech stack](#tech-stack)
- [Architecture](#architecture)
- [Project structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting started](#getting-started)
- [Seeded demo data](#seeded-demo-data)
- [Resetting the dev databases](#resetting-the-dev-databases)
- [Authentication guide](#authentication-guide)
  - [1. Calling the API with a key](#1-calling-the-api-with-a-key)
  - [2. Roles](#2-roles)
  - [3. Managing API keys](#3-managing-api-keys)
  - [4. Product ownership](#4-product-ownership)
- [API reference](#api-reference)
- [Automation (report generation)](#automation-report-generation)
- [Database & migrations](#database--migrations)
- [Configuration reference](#configuration-reference)
- [Error response format](#error-response-format)
- [Extending this template](#extending-this-template)

---

## What this demonstrates

| Concern | Approach used here |
|---|---|
| API key storage | Only a SHA-256 hash of the key's secret portion is ever stored; the full key is returned **exactly once**, at creation time |
| Authentication | A single custom `ApiKey` scheme (`ApiKeyAuthenticationHandler`) — `[Authorize]` everywhere means "a valid, unrevoked, unexpired key" |
| Roles | A real `Roles` table, many-to-many with `ApiKeys` through `ApiKeyRoles` — Admin keys can define new roles at runtime |
| Authorization | One role claim per role on the key, so `[Authorize(Roles = "Admin")]` and role-based policies (`ReadAccess`, `WriteAccess`, `ReportAccess`) work directly |
| Ownership | `ApiKey.OwnerUsername` is stamped on the products a key creates; non-Admin keys only see/change their owner's products |
| Lockout protection | The last active `Admin` key cannot be revoked or lose its `Admin` role (`409`) |
| Two databases | SQL Server for `Roles`/`ApiKeys`/`ApiKeyRoles`, PostgreSQL for `Products` — no shared foreign keys, ownership enforced in application code |
| Layering | Controllers → Services (business logic) → Repositories (EF Core / Dapper access), wired through DI |
| Error handling | Typed exceptions mapped to HTTP status codes + `ProblemDetails` bodies by a global exception handler |
| Secrets | `.env` file (gitignored) + `DotNetEnv`, mirrored into a gitignored `launchSettings.json` for F5 debugging |

## Tech stack

- **.NET 10** / ASP.NET Core Web API
- **Entity Framework Core 10** — `Microsoft.EntityFrameworkCore.SqlServer` + `Npgsql.EntityFrameworkCore.PostgreSQL`
- **EFCore.NamingConventions** — enforces snake_case naming on the PostgreSQL side
- **Dapper** — raw-SQL read path for the report endpoints (alongside EF Core)
- **Serilog** — structured console logging (API) and structured JSON file logging (Automation)
- **ClosedXML** — Excel (.xlsx) generation in the Automation project
- **Scalar** — interactive OpenAPI documentation UI (not Swagger UI)
- **DotNetEnv** — loads `.env` into configuration at startup

## Architecture

```
HTTP request
     │
     ▼
Controllers/*            ← thin; parse request, read caller identity from claims, delegate
     │
     ▼
Services/*                ← business logic & authorization decisions (ownership, lockout guard)
     │
     ▼
Repository/DB/*           ← EF Core data access only, no business rules
Repository/DapperDB/*     ← Dapper / ADO.NET report queries
     │
     ▼
AuthDbContext (SQL Server)              AppDbContext (PostgreSQL)
   Roles, ApiKeys, ApiKeyRoles, stg.*      Products
```

Authentication is resolved **before** any of this, by the `ApiKey` scheme configured in `Extensions/AuthenticationServiceExtensions.cs`:

```
Incoming request
     │
     ├─ no "X-Api-Key" header ──► not authenticated → 401 on any [Authorize] endpoint
     │
     └─ "X-Api-Key: kbat_<keyId>.<secret>"
            ApiKeyAuthenticationHandler
              looks up ApiKeys by keyId (with Roles), verifies the secret hash,
              rejects revoked / expired keys, updates LastUsedAtUtc
              → principal claims: key id, key name, owner_username, one role claim per role
     │
     ▼
[Authorize(Roles = "Admin")] / [Authorize(Policy = Policies.ReadAccess | WriteAccess | ReportAccess)]
```

Policies (`Authorization/Policies.cs`) are plain role requirements — `Admin` passes all of them:

| Policy | Roles that pass |
|---|---|
| `ReadAccess` | `Reader`, `Writer`, `Reporter`, `Admin` |
| `WriteAccess` | `Writer`, `Admin` |
| `ReportAccess` | `Reporter`, `Admin` |

Because `Product.OwnerUsername` refers to an `ApiKey.OwnerUsername` in a *different physical database*, there is no database-level foreign key between them — `ProductService` enforces ownership (with an Admin/Reporter bypass) in code instead.

## Project structure

```
KeyBasedAuthenticationTemplate_API/            (solution root)
├── env/
│   ├── .env                                   ← your real secrets (gitignored)
│   └── .example.env                           ← committed template, copy this to .env
├── reset-dev-db.ps1                           ← drops both dev databases so they re-seed on next run
├── KeyBasedAuthenticationTemplate_Contracts/  ← DTOs/constants shared by API and Automation
├── KeyBasedAuthenticationTemplate_Automation/ ← report job: API → Excel → mock email (see "Automation")
└── KeyBasedAuthenticationTemplate_API/        (project root)
    ├── Authentication/                        ← custom API key auth scheme
    ├── Authorization/Policies.cs              ← ReadAccess / WriteAccess / ReportAccess policy names
    ├── Common/
    │   ├── Exceptions/                        ← ConflictException, NotFoundException, etc.
    │   ├── SystemRoleNames.cs                 ← reserved Admin/Reader/Writer/Reporter role names
    │   └── ClaimsPrincipalExtensions.cs        ← GetApiKeyId() / GetOwnerUsername() / IsAdmin()
    ├── Configuration/                         ← ApiKeySettings, DatabaseSettings (bound from config)
    ├── Controllers/                           ← thin HTTP endpoints
    ├── Data/
    │   ├── AuthDbContext.cs                   ← SQL Server: Roles, ApiKeys, ApiKeyRoles
    │   ├── AppDbContext.cs                    ← PostgreSQL: Products
    │   ├── Migrations/AuthDb/ , Migrations/AppDb/
    │   └── Seed/DbInitializer.cs              ← Development-only demo data (roles, dev keys, products, stg tables)
    ├── DTOs/                                  ← request/response models, by feature
    ├── Entities/                              ← Role, ApiKey, Product
    ├── ErrorHandling/GlobalExceptionHandler.cs
    ├── Extensions/                            ← DI/auth wiring, keeps Program.cs short
    ├── Helpers/                               ← ApiKeyGenerator
    ├── OpenApi/SecuritySchemeTransformer.cs   ← registers the ApiKey scheme for Scalar
    ├── Repository/
    │   ├── Interfaces/
    │   ├── DB/                                ← EF Core-backed repository implementations
    │   └── DapperDB/                          ← Dapper-backed (raw SQL) report repositories
    ├── Sql/                                   ← raw queries as .sql files (Postgres/, SqlServer/), embedded in the dll
    ├── Services/
    │   ├── Interfaces/
    │   └── *.cs                               ← business logic
    ├── Properties/
    │   ├── launchSettings.json                ← your real secrets for F5 (gitignored)
    │   └── launchSettings.json.example        ← committed template, copy this
    └── Program.cs
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A local **SQL Server** instance (used for `Roles`/`ApiKeys`/`ApiKeyRoles` and the `stg` staging tables)
- A local **PostgreSQL** instance (used for `Products`)
- The EF Core CLI tool:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

## Getting started

1. **Clone and restore**
   ```bash
   git clone <this-repo>
   cd KeyBasedAuthenticationTemplate_API/KeyBasedAuthenticationTemplate_API
   dotnet restore
   ```

2. **Set up secrets.** Two files are gitignored and need to be created from their committed templates:

   ```bash
   # from the solution root
   cp env/.example.env env/.env
   cp KeyBasedAuthenticationTemplate_API/Properties/launchSettings.json.example \
      KeyBasedAuthenticationTemplate_API/Properties/launchSettings.json
   ```

   Edit **both** files and fill in:
   - `ConnectionStrings__AuthDb` — your SQL Server connection string
   - `ConnectionStrings__AppDb` — your PostgreSQL connection string

   > `.env` is read when you run `dotnet run`/`dotnet watch`. `launchSettings.json` is read when you launch via Visual Studio (F5). They are **not** wired together — if you change a secret, update both.

3. **Run the app** (Development)
   ```bash
   dotnet run
   ```
   On startup in Development, `DbInitializer` applies the migrations for both databases, creates the `stg.ProductRiskSummary` staging table, and seeds the system roles, four dev API keys and the sample products (see below).

4. **Open the API docs** — Scalar's UI is served at:
   ```
   http://localhost:5090/scalar
   ```
   (or `https://localhost:7116/scalar` on the https profile). Use **Authorize → ApiKey** and paste one of the dev keys.

## Seeded demo data

Created automatically by `Data/Seed/DbInitializer.cs` when you run in Development. Seeding is idempotent — it checks for existing rows before inserting, so re-running the app won't duplicate data.

**Roles** (`IsSystemRole = true`):

| Role | Grants |
|---|---|
| `Admin` | Manage API keys and roles; read/write every owner's products; passes every policy |
| `Reader` | Read the products of the key's own owner |
| `Writer` | Create/update/delete the products of the key's own owner; with `Reporter`, send notifications and load staging |
| `Reporter` | Report endpoints; read every owner's products; with `Writer`, send notifications and load staging |

**Dev API keys** (fixed values, Development only — never use them anywhere else):

| Key name | Owner | Roles | Full key (`X-Api-Key`) |
|---|---|---|---|
| Dev admin key | `admin` | Admin | `kbat_devadmin.dev-admin-secret-change-me` |
| Dev user key | `user` | Reader, Writer | `kbat_devuser.dev-user-secret-change-me` |
| Dev user read-only key | `user` | Reader | `kbat_devreader.dev-reader-secret-change-me` |
| Dev automation key | `automation` | Reporter, Writer | `kbat_devautomation.dev-automation-secret-change-me` |

(`kbat` is the default `ApiKey__Prefix`; if you change the prefix, the seeded keys use yours.)

**Products** (PostgreSQL): five owned by `user`, two owned by `admin` — so owner scoping is visible straight away.

## Resetting the dev databases

To throw away all data and reload the dummy data from scratch (also required the first time you switch to this branch from a version with users, since the migrations were regenerated):

```powershell
./reset-dev-db.ps1          # asks for confirmation; -Force skips it
dotnet run --project KeyBasedAuthenticationTemplate_API
```

The script runs `dotnet ef database drop --force` for both contexts. The next Development start recreates the databases from the migrations and re-seeds everything above, including the `stg` staging table.

## Authentication guide

### 1. Calling the API with a key

Every protected endpoint takes the key in the `X-Api-Key` header:

```bash
curl http://localhost:5090/api/products -H "X-Api-Key: kbat_devuser.dev-user-secret-change-me"
```

No header, an unknown key, a wrong secret, a revoked key or an expired key → `401`. A valid key without the required role → `403`.

### 2. Roles

Roles are rows in a table, not a fixed enum. All role endpoints require the `Admin` role.

**Create a role:**

```bash
curl -X POST http://localhost:5090/api/roles \
  -H "X-Api-Key: kbat_devadmin.dev-admin-secret-change-me" \
  -H "Content-Type: application/json" \
  -d '{"name":"Support", "description":"Support tooling"}'
```

**List roles:**

```bash
curl http://localhost:5090/api/roles -H "X-Api-Key: kbat_devadmin.dev-admin-secret-change-me"
```

A custom role shows up as a role claim on the keys that hold it, but grants nothing until code references it (e.g. `[Authorize(Roles = "Support")]` or a new policy).

### 3. Managing API keys

All key management requires the `Admin` role.

**Create a key:**

```bash
curl -X POST http://localhost:5090/api/apikeys \
  -H "X-Api-Key: kbat_devadmin.dev-admin-secret-change-me" \
  -H "Content-Type: application/json" \
  -d '{"name":"CI pipeline", "ownerUsername":"ci", "roles":["Reader","Writer"], "expiresInDays": 90}'
```

```json
{
  "id": "4e27c7e0-74c0-4856-8136-3e902229a2ee",
  "name": "CI pipeline",
  "ownerUsername": "ci",
  "fullKey": "kbat_c1c99e627fcf2b48.aRYNQKZtCPMcvB1XN3oK14sZxSn3d3bZvFCtA-BsLCg",
  "roles": ["Reader", "Writer"],
  "createdAtUtc": "2026-10-03T15:51:15Z",
  "expiresAtUtc": "2027-01-01T15:51:15Z"
}
```

> ⚠️ **`fullKey` is only ever returned here.** The database only stores a SHA-256 hash of its secret portion — there is no "show key again" endpoint. Save it immediately.

`roles` must contain at least one existing role name (case-insensitive); unknown names → `400`. Omit `expiresInDays` (or send `null`) for a key that never expires.

**List all keys** (no secrets or hashes included):

```bash
curl http://localhost:5090/api/apikeys -H "X-Api-Key: kbat_devadmin.dev-admin-secret-change-me"
```

**Replace a key's roles** (full replace — the key ends up with exactly this set; takes effect on its next request):

```bash
curl -X PUT http://localhost:5090/api/apikeys/<id>/roles \
  -H "X-Api-Key: kbat_devadmin.dev-admin-secret-change-me" \
  -H "Content-Type: application/json" \
  -d '{"roles": ["Reader"]}'
```

**Revoke a key:**

```bash
curl -X DELETE http://localhost:5090/api/apikeys/<id> -H "X-Api-Key: kbat_devadmin.dev-admin-secret-change-me"
```

Revoking the **last** active `Admin` key, or removing `Admin` from it, is refused with `409` — otherwise nobody could manage keys any more.

### 4. Product ownership

- A key's `ownerUsername` is stamped on every product it creates.
- `Reader`/`Writer` keys only see, update and delete products with the same owner (`403` on another owner's product).
- `Reporter` keys read every owner's products (`GET /api/products` doubles as the `products-ef` report), but writes stay limited to their own owner.
- `Admin` keys read and write every owner's products.

## API reference

### Roles — `/api/roles` (requires `Admin` role)

| Method | Path | Body | Description |
|---|---|---|---|
| POST | `/api/roles` | `{ name, description? }` | Create a new role |
| GET | `/api/roles` | — | List all roles |

### API keys — `/api/apikeys` (requires `Admin` role)

| Method | Path | Body | Description |
|---|---|---|---|
| POST | `/api/apikeys` | `{ name, ownerUsername, roles[], expiresInDays? }` | Create a new API key; returns the full key **once** |
| GET | `/api/apikeys` | — | List all keys with owner and roles (no secrets) |
| PUT | `/api/apikeys/{id}/roles` | `{ roles[] }` | Replace a key's full role set |
| DELETE | `/api/apikeys/{id}` | — | Revoke a key |

### Products — `/api/products` (requires `ReadAccess`)

A demo CRUD resource, scoped to the calling key's owner (see [Product ownership](#4-product-ownership)).

| Method | Path | Body | Policy | Description |
|---|---|---|---|---|
| GET | `/api/products` | — | `ReadAccess` | List the owner's products (all products for `Admin`/`Reporter`) |
| GET | `/api/products/{id}` | — | `ReadAccess` | Get one product (403 if another owner's and not `Admin`/`Reporter`) |
| POST | `/api/products` | `{ name, description?, price, stock }` | `WriteAccess` | Create a product owned by the key's owner |
| PUT | `/api/products/{id}` | `{ name, description?, price, stock }` | `WriteAccess` | Update a product of the key's owner (any, as `Admin`) |
| DELETE | `/api/products/{id}` | — | `WriteAccess` | Delete a product of the key's owner (any, as `Admin`) |

### Reports — `/api/reports` (requires `ReportAccess`)

Read-only report data for the Automation project. `ef` endpoints read through EF Core, `dapper` endpoints return the same data through raw Dapper SQL. (Products via EF is the existing `GET /api/products`.)

| Method | Path | Tech / database | Description |
|---|---|---|---|
| GET | `/api/reports/ef/apikeys` | EF Core / SQL Server | All API keys with owner and roles (same as `/api/apikeys`, but for `Reporter` keys) |
| GET | `/api/reports/dapper/products` | Dapper / PostgreSQL | All products (same data as `GET /api/products` for a `Reporter` key) |
| GET | `/api/reports/dapper/apikeys` | Dapper / SQL Server | All API keys with owner and roles |

### Notifications — `/api/notifications` (requires `ReportAccess` **and** `WriteAccess`)

| Method | Path | Body | Description |
|---|---|---|---|
| POST | `/api/notifications/email` | `{ to[], subject, body, isHtml?, attachments?[{ fileName, contentType, contentBase64 }] }` | **Mock** email: validates and logs the message (recipients, subject, attachment names/sizes — never contents), sends nothing, returns `202` with a `messageId`. Swap `MockEmailService` for a real `IEmailService` to deliver mail. |

### Staging — `/api/staging` (requires `ReportAccess` **and** `WriteAccess`)

| Method | Path | Description |
|---|---|---|
| POST | `/api/staging/product-risk-summary` | Groups `products` (PostgreSQL, Dapper) by cost band × stock band into risk levels, then replaces the contents of `stg.ProductRiskSummary` (SQL Server) in one transaction: `TRUNCATE` + a streamed `SqlBulkCopy` (the PostgreSQL reader feeds the bulk copy directly, nothing is buffered in memory). Returns `{ table, rowsLoaded, snapshotAtUtc }`. |

"High" cost/stock means price ≥ 250 / stock ≥ 250 (constants in `ProductDapperRepository`). The staging table is **not** managed by EF migrations. In Development `DbInitializer` creates it from `Sql/SqlServer/CreateStagingTables.sql` (idempotent); elsewhere, run that script once against the `AuthDb` database.

## Automation (report generation)

`KeyBasedAuthenticationTemplate_Automation` is a one-shot console job that exercises the API with an API key:

```
Automation ──(X-Api-Key)──► API reports ──► Excel (.xlsx) ──► POST /api/notifications/email (mock)
```

Depending on the mode (below) it first refreshes the SQL Server staging table, then generates four reports — `products-ef`, `apikeys-ef`, `products-dapper`, `apikeys-dapper` — saves each to `Automation:Reports:OutputDirectory`, and sends one digest email with all files attached. A failing report is logged and skipped, and any failure sends an error-notification email to `Automation:ErrorNotification:Recipients`. Exit code: `0` all good, `1` at least one report/email failed, `2` bad configuration or crash.

**Setup**

1. The key needs the **`Reporter` and `Writer`** roles (`Writer` because sending the mock email and loading staging are `POST`s). In Development, use the seeded `kbat_devautomation...` key — `env/.example.env` already contains it. Elsewhere, create one with an Admin key: `POST /api/apikeys` with `"roles": ["Reporter","Writer"]`.
2. Put the key in `env/.env` (gitignored) as `Automation__Api__Key=...`. Everything else is configured in `KeyBasedAuthenticationTemplate_Automation/appsettings.json` — check at least `Automation:Api:BaseUrl` and the recipients. The Automation project finds `env/.env` by walking up from the working directory; real environment variables take precedence.
3. Run it (API must be running). The optional argument picks the mode:
   ```bash
   dotnet run --project KeyBasedAuthenticationTemplate_Automation                      # same as "reports"
   dotnet run --project KeyBasedAuthenticationTemplate_Automation -- reports            # reports only
   dotnet run --project KeyBasedAuthenticationTemplate_Automation -- stage-and-reports  # refresh staging, then reports
   ```
   In `stage-and-reports` mode a staging failure is logged, triggers the error email, and stops the run before any report is generated (exit code `1`). An unknown argument prints usage and exits `2`.

   Schedule it with Windows Task Scheduler / cron / CI — there is no in-process scheduler.

**Configuration** lives in `KeyBasedAuthenticationTemplate_Automation/appsettings.json` (copied next to the executable and read from there, whatever the working directory). Only secrets stay in `env/.env`. Any value can be overridden by an environment variable or `env/.env` line using `__` for nesting and indexes for arrays, e.g. `Automation__Reports__OutputDirectory=D:/reports` or `Automation__Reports__Recipients__0=me@example.com`.

| Setting (`Automation:` section) | Default | Purpose |
|---|---|---|
| `Api:BaseUrl` | `http://localhost:5090` | API to call |
| `Api:Key` | *(empty)* | **Secret** — an API key with `Reporter` + `Writer`; set `Automation__Api__Key` in `env/.env`, never in appsettings.json |
| `Api:TimeoutSeconds` | `60` | Per-call HTTP timeout (the staging call runs a query plus a bulk load, so it can take longer than a report call) |
| `Api:ErrorDetailMaxLength` | `300` | API error bodies are cut to this length in messages/logs |
| `Api:Endpoints:Email` / `ProductRiskStaging` | `api/notifications/email` / `api/staging/product-risk-summary` | Non-report endpoint paths |
| `Reports:OutputDirectory` | `reports-output` | Where `.xlsx` files are saved (relative = working directory; use an absolute path for scheduled runs) |
| `Reports:FileTimestampFormat` | `yyyyMMdd_HHmmss` | Timestamp in `{report}_{timestamp}.xlsx` (UTC) |
| `Reports:Recipients` | `["reports@example.com"]` | Digest email recipients |
| `Reports:EmailSubject` | `Automated reports - {0:yyyy-MM-dd}` | `{0}` = run start time |
| `Reports:Definitions:<name>` | one per report | `Enabled`, `Endpoint` and `SheetName` for `products-ef`, `apikeys-ef`, `products-dapper`, `apikeys-dapper` (set `Enabled` to `false` to skip a report) |
| `ErrorNotification:Recipients` / `Subject` | `["ops-alerts@example.com"]` / `[ALERT] Report automation failed - {0} error(s)` | Failure email; `{0}` = number of errors |

Startup validates all of this at once and exits with code `2` listing every problem (missing key, bad URL, empty recipients, ...). Column layouts and Excel styling are code (`ReportColumns`, `ClosedXmlReportWriter`), not configuration.

**Logging** is the standard Serilog `Serilog` section of the same file: sinks, log file path (`logs/automation-.log`), rolling interval, retention (30 files), minimum levels and the JSON format. Override the file path with e.g. `Serilog__WriteTo__1__Args__path=D:/logs/automation-.log`. Logs are structured JSON, one file per day: every API call (method, path, status, elapsed), each generated report, emails, and failures with exceptions. The API key is never logged.

API-side timeouts (`Database:CommandTimeoutSeconds`, `Database:BulkCopyTimeoutSeconds`, both `60`) are in the API's `appsettings.json`. `BulkCopyTimeoutSeconds` covers the whole streamed load, so it includes time spent waiting on a slow source query.

Shared request/response models used by both the API and Automation (`ProductResponse`, `ApiKeySummaryResponse`, the email DTOs, the `X-Api-Key` header name) live in `KeyBasedAuthenticationTemplate_Contracts`.

### Raw SQL files and parameters

Dapper/ADO.NET queries live in `Sql/<Postgres|SqlServer>/*.sql`, compiled in as embedded resources and loaded by `Data/SqlQueries.Load("Postgres.ProductsAll")`. Each report query carries a `WHERE 1 = 1 OR (...)` demo clause: always true, so it never changes results, but every parameter is bound with an explicit type — Dapper `DynamicParameters`/`DbString` in `ProductDapperRepository.GetAllAsync` and `ApiKeyDapperRepository`, and typed `NpgsqlParameter`s in plain ADO.NET in `ProductDapperRepository.StreamRiskSummaryAsync`. Remove the clause when you add real filters. Values are never concatenated into the SQL text.

## Database & migrations

This project uses **two independent `DbContext`s against two different database engines** — always specify `--context` with EF Core CLI commands:

```bash
# Create a new migration
dotnet ef migrations add <Name> --context AuthDbContext --output-dir Data/Migrations/AuthDb
dotnet ef migrations add <Name> --context AppDbContext  --output-dir Data/Migrations/AppDb

# Apply migrations (Development also applies them on startup)
dotnet ef database update --context AuthDbContext
dotnet ef database update --context AppDbContext
```

**Naming conventions differ by database**, intentionally:

- **SQL Server** (`AuthDbContext`) — PascalCase tables/columns (`Roles`, `ApiKeys`, `ApiKeyRoles`, `OwnerUsername`, ...), matching C# property names directly.
- **PostgreSQL** (`AppDbContext`) — snake_case tables/columns (`products`, `owner_username`, `created_at_utc`, ...), enforced automatically for every property via `EFCore.NamingConventions`' `.UseSnakeCaseNamingConvention()`. Don't add manual `[Column]`/`[Table]` attributes to work around casing — the convention already handles every entity in that context.

## Configuration reference

All of these are read as environment variables (double-underscore maps to nested config, e.g. `ApiKey__Prefix` → `ApiKey:Prefix`), set in both `env/.env` and `Properties/launchSettings.json`:

| Key | Example | Description |
|---|---|---|
| `ConnectionStrings__AuthDb` | `Server=localhost,1433;Database=AuthDb;User Id=sa;Password=...;TrustServerCertificate=True;` | SQL Server connection string |
| `ConnectionStrings__AppDb` | `Host=localhost;Port=5432;Database=AppDb;Username=postgres;Password=...` | PostgreSQL connection string |
| `ApiKey__Prefix` | `kbat` | Prefix used in API keys (`{prefix}_{keyId}.{secret}`) |

## Error response format

Business-rule failures return [`ProblemDetails`](https://datatracker.ietf.org/doc/html/rfc7807) JSON with the appropriate status code:

```json
// 400 Bad Request - unknown role name
{ "title": "Unknown role(s): Auditor.", "status": 400, "instance": "/api/apikeys" }

// 409 Conflict - lockout guard
{ "title": "Cannot revoke the last active Admin key.", "status": 409, "instance": "/api/apikeys/4e27c7e0-74c0-4856-8136-3e902229a2ee" }
```

`[ApiController]`'s built-in model validation handles malformed request bodies (missing fields, empty `roles`, etc.) and returns a standard `400` validation-errors payload automatically.

## Extending this template

- **Add a new field to `Products`**: edit `Entities/Product.cs`, then `dotnet ef migrations add <Name> --context AppDbContext -o Data/Migrations/AppDb` — snake_case naming is applied automatically.
- **Add a new SQL Server-backed entity**: add it to `Entities/`, register a `DbSet` + Fluent config in `AuthDbContext.OnModelCreating`, then generate a migration with `--context AuthDbContext`.
- **Add a new role**: `POST /api/roles` with an Admin key, assign it with `PUT /api/apikeys/{id}/roles`, then reference it from code (`[Authorize(Roles = "...")]` or a new policy in `AuthenticationServiceExtensions`).
- **Add a new write-protected endpoint**: add `[Authorize(Policy = Policies.WriteAccess)]` on the mutating action, the same way `ProductsController` does it.
- **Add a new protected resource**: follow the `Products` pattern — `Entities/` → `Repository/Interfaces` + `Repository/DB` → `Services/Interfaces` + `Services` → `Controllers`, registering the new repository/service in `Extensions/ApplicationServiceExtensions.cs`.
