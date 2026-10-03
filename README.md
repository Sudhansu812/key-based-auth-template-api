# KeyBasedAuthenticationTemplate_API

A production-style **.NET 10 Web API template** demonstrating two authentication methods side by side:

- **Username/password login** → short-lived JWT access tokens
- **User-generated API keys** → long-lived, header-based credentials (shown once, hashed at rest)

Both work interchangeably on the *same* protected endpoints. The template also demonstrates a two-database setup (SQL Server for identity, PostgreSQL for business data), role-based authorization, and a layered architecture meant to be easy to extend for a real project.

This README is a guide: it walks through what the project does, how to get it running locally, and how to use every endpoint.

---

## Table of contents

- [What this demonstrates](#what-this-demonstrates)
- [Tech stack](#tech-stack)
- [Architecture](#architecture)
- [Project structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting started](#getting-started)
- [Seeded demo accounts](#seeded-demo-accounts)
- [Authentication guide](#authentication-guide)
  - [1. Username/password → JWT](#1-usernamepassword--jwt)
  - [2. API keys](#2-api-keys)
  - [3. Calling a protected endpoint with either credential](#3-calling-a-protected-endpoint-with-either-credential)
- [API reference](#api-reference)
- [Database & migrations](#database--migrations)
- [Configuration reference](#configuration-reference)
- [Error response format](#error-response-format)
- [Extending this template](#extending-this-template)

---

## What this demonstrates

| Concern | Approach used here |
|---|---|
| Password storage | `Microsoft.AspNetCore.Identity.PasswordHasher<User>` (PBKDF2) — no external hashing library |
| API key storage | Only a SHA-256 hash of the key's secret portion is ever stored; the full key is returned **exactly once**, at creation time |
| Dual authentication | A policy scheme picks JWT Bearer or API key authentication per-request based on which header is present — `[Authorize]` works the same either way |
| Authorization | Simple `User`/`Admin` roles via `[Authorize(Roles = "Admin")]` |
| Two databases | SQL Server for `Users`/`ApiKeys`, PostgreSQL for `Products` — no shared foreign keys, ownership enforced in application code |
| Layering | Controllers → Services (business logic) → Repositories (EF Core access), wired through DI |
| Error handling | Typed exceptions mapped to HTTP status codes + `ProblemDetails` bodies by a global exception handler |
| Secrets | `.env` file (gitignored) + `DotNetEnv`, mirrored into a gitignored `launchSettings.json` for F5 debugging |

## Tech stack

- **.NET 10** / ASP.NET Core Web API
- **Entity Framework Core 10** — `Microsoft.EntityFrameworkCore.SqlServer` + `Npgsql.EntityFrameworkCore.PostgreSQL`
- **EFCore.NamingConventions** — enforces snake_case naming on the PostgreSQL side
- **JWT Bearer authentication** — `Microsoft.AspNetCore.Authentication.JwtBearer`
- **Serilog** — structured console logging
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
Services/*                ← business logic & authorization decisions (ownership, role checks)
     │
     ▼
Repository/DB/*           ← EF Core data access only, no business rules
     │
     ▼
AuthDbContext (SQL Server)   AppDbContext (PostgreSQL)
   Users, ApiKeys               Products
```

Authentication is resolved **before** any of this, by a policy scheme configured in `Extensions/AuthenticationServiceExtensions.cs`:

```
Incoming request
     │
     ├─ has "X-Api-Key" header? ──► ApiKeyAuthenticationHandler (custom scheme)
     │                                  validates against ApiKeys table (SQL Server)
     │
     └─ otherwise ─────────────────► JwtBearer scheme
                                        validates the signed JWT from /api/auth/login
     │
     ▼
Same ClaimsPrincipal shape either way → [Authorize] / [Authorize(Roles = "Admin")]
```

Because `Product.OwnerUserId` points at a `User.Id` in a *different physical database*, there is no database-level foreign key between them — `ProductService` enforces ownership (or admin bypass) in code instead.

## Project structure

```
KeyBasedAuthenticationTemplate_API/            (solution root)
├── env/
│   ├── .env                                   ← your real secrets (gitignored)
│   └── .example.env                           ← committed template, copy this to .env
└── KeyBasedAuthenticationTemplate_API/        (project root)
    ├── Authentication/                        ← custom API key auth scheme
    ├── Common/
    │   ├── Exceptions/                        ← ConflictException, NotFoundException, etc.
    │   └── ClaimsPrincipalExtensions.cs        ← GetUserId() / IsAdmin() helpers
    ├── Configuration/                         ← JwtSettings, ApiKeySettings (bound from config)
    ├── Controllers/                           ← thin HTTP endpoints
    ├── Data/
    │   ├── AuthDbContext.cs                   ← SQL Server: Users, ApiKeys
    │   ├── AppDbContext.cs                    ← PostgreSQL: Products
    │   ├── Migrations/AuthDb/ , Migrations/AppDb/
    │   └── Seed/DbInitializer.cs              ← Development-only demo data
    ├── DTOs/                                  ← request/response models, by feature
    ├── Entities/                              ← User, ApiKey, Product, UserRole
    ├── ErrorHandling/GlobalExceptionHandler.cs
    ├── Extensions/                            ← DI/auth wiring, keeps Program.cs short
    ├── Helpers/                               ← JwtTokenGenerator, ApiKeyGenerator
    ├── OpenApi/SecuritySchemeTransformer.cs   ← registers Bearer + ApiKey schemes for Scalar
    ├── Repository/
    │   ├── Interfaces/
    │   └── DB/                                ← EF Core-backed repository implementations
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
- A local **SQL Server** instance (used for `Users`/`ApiKeys`)
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
   - `Jwt__Secret` — a long random string (e.g. `openssl rand -base64 48`)

   > `.env` is read when you run `dotnet run`/`dotnet watch`. `launchSettings.json` is read when you launch via Visual Studio (F5). They are **not** wired together — if you change a secret, update both.

3. **Apply migrations** (there are two independent `DbContext`s — always pass `--context`):
   ```bash
   dotnet ef database update --context AuthDbContext
   dotnet ef database update --context AppDbContext
   ```
   (Running the app in Development also auto-applies any pending migrations on startup, so this step is optional but useful if you want the databases ready before first run.)

4. **Run the app**
   ```bash
   dotnet run
   ```
   On first run in Development, `DbInitializer` seeds two demo users and five sample products (see below).

5. **Open the API docs** — Scalar's UI is served at:
   ```
   http://localhost:5090/scalar
   ```
   (or `https://localhost:7116/scalar` on the https profile)

## Seeded demo accounts

Created automatically by `Data/Seed/DbInitializer.cs` the first time you run in Development:

| Username | Email | Password | Role |
|---|---|---|---|
| `admin` | admin@example.com | `Admin123!` | Admin |
| `user` | user@example.com | `User123!` | User |

Five sample `Products` are also seeded, owned by `user`. Seeding is idempotent — it checks for existing rows before inserting, so re-running the app won't duplicate data.

## Authentication guide

### 1. Username/password → JWT

Register a new account:

```bash
curl -X POST http://localhost:5090/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"you@example.com","username":"you","password":"Passw0rd!"}'
```

Or log in with an existing one:

```bash
curl -X POST http://localhost:5090/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"usernameOrEmail":"admin","password":"Admin123!"}'
```

Both return:

```json
{
  "accessToken": "eyJhbGciOi...",
  "expiresAtUtc": "2026-10-03T16:51:00Z",
  "userId": "b52f2e9e-acf2-4958-a7c1-6aefeb6bcc22",
  "username": "admin",
  "role": "Admin"
}
```

Use `accessToken` as a Bearer token on subsequent requests:

```bash
curl http://localhost:5090/api/products \
  -H "Authorization: Bearer <accessToken>"
```

The token expires after `Jwt__ExpiryMinutes` (default 60 minutes) — there is no refresh token flow in this template; log in again once it expires.

### 2. API keys

API keys are generated by an **authenticated** caller (via JWT or an existing API key) and belong to that user. The key has the shape `{prefix}_{keyId}.{secret}` — e.g. `kbat_8f3a1b2c9d4e.Xy7h2K...`.

**Create a key:**

```bash
curl -X POST http://localhost:5090/api/apikeys \
  -H "Authorization: Bearer <accessToken>" \
  -H "Content-Type: application/json" \
  -d '{"name":"My CI pipeline", "expiresInDays": 90}'
```

```json
{
  "id": "4e27c7e0-74c0-4856-8136-3e902229a2ee",
  "name": "My CI pipeline",
  "fullKey": "kbat_c1c99e627fcf2b48.aRYNQKZtCPMcvB1XN3oK14sZxSn3d3bZvFCtA-BsLCg",
  "createdAtUtc": "2026-10-03T15:51:15Z",
  "expiresAtUtc": "2027-01-01T15:51:15Z"
}
```

> ⚠️ **`fullKey` is only ever returned here.** The database only stores a SHA-256 hash of its secret portion — there is no "show key again" endpoint. Save it immediately.

**List your keys** (no secrets included):

```bash
curl http://localhost:5090/api/apikeys -H "Authorization: Bearer <accessToken>"
```

**Revoke a key:**

```bash
curl -X DELETE http://localhost:5090/api/apikeys/<id> -H "Authorization: Bearer <accessToken>"
```

Omit `expiresInDays` (or send `null`) for a key that never expires.

### 3. Calling a protected endpoint with either credential

Once you have a key, it works exactly like a JWT on any protected endpoint — just swap the header:

```bash
curl http://localhost:5090/api/products -H "X-Api-Key: kbat_c1c99e627fcf2b48.aRYNQKZtCPMcvB1XN3oK14sZxSn3d3bZvFCtA-BsLCg"
```

If both a `Authorization: Bearer` header and an `X-Api-Key` header are absent, you'll get a `401`. If `X-Api-Key` is present, it takes priority over any bearer token on the same request.

## API reference

### Auth — `/api/auth` (no authentication required)

| Method | Path | Body | Description |
|---|---|---|---|
| POST | `/api/auth/register` | `{ email, username, password }` | Create a new `User` account (role: `User`), returns a JWT |
| POST | `/api/auth/login` | `{ usernameOrEmail, password }` | Authenticate, returns a JWT |

### API keys — `/api/apikeys` (requires authentication)

| Method | Path | Body | Description |
|---|---|---|---|
| POST | `/api/apikeys` | `{ name, expiresInDays? }` | Create a new API key for the caller; returns the full key **once** |
| GET | `/api/apikeys` | — | List the caller's own keys (no secrets) |
| DELETE | `/api/apikeys/{id}` | — | Revoke one of the caller's own keys |

### Products — `/api/products` (requires authentication)

A demo CRUD resource, scoped to the caller unless they're an `Admin`.

| Method | Path | Body | Description |
|---|---|---|---|
| GET | `/api/products` | — | List products owned by the caller (all products if `Admin`) |
| GET | `/api/products/{id}` | — | Get one product (403 if you don't own it and aren't `Admin`) |
| POST | `/api/products` | `{ name, description?, price, stock }` | Create a product owned by the caller |
| PUT | `/api/products/{id}` | `{ name, description?, price, stock }` | Update a product you own (or any, as `Admin`) |
| DELETE | `/api/products/{id}` | — | Delete a product you own (or any, as `Admin`) |

### Users — `/api/users` (requires `Admin` role)

| Method | Path | Body | Description |
|---|---|---|---|
| GET | `/api/users` | — | List all users (id, email, username, role, active, created date — no password hashes) |

## Database & migrations

This project uses **two independent `DbContext`s against two different database engines** — always specify `--context` with EF Core CLI commands:

```bash
# Create a new migration
dotnet ef migrations add <Name> --context AuthDbContext --output-dir Data/Migrations/AuthDb
dotnet ef migrations add <Name> --context AppDbContext  --output-dir Data/Migrations/AppDb

# Apply migrations
dotnet ef database update --context AuthDbContext
dotnet ef database update --context AppDbContext
```

**Naming conventions differ by database**, intentionally:

- **SQL Server** (`AuthDbContext`) — PascalCase tables/columns (`Users`, `ApiKeys`, `PasswordHash`, ...), matching C# property names directly.
- **PostgreSQL** (`AppDbContext`) — snake_case tables/columns (`products`, `owner_user_id`, `created_at_utc`, ...), enforced automatically for every property via `EFCore.NamingConventions`' `.UseSnakeCaseNamingConvention()`. Don't add manual `[Column]`/`[Table]` attributes to work around casing — the convention already handles every entity in that context.

## Configuration reference

All of these are read as environment variables (double-underscore maps to nested config, e.g. `Jwt__Secret` → `Jwt:Secret`), set in both `env/.env` and `Properties/launchSettings.json`:

| Key | Example | Description |
|---|---|---|
| `ConnectionStrings__AuthDb` | `Server=localhost,1433;Database=AuthDb;User Id=sa;Password=...;TrustServerCertificate=True;` | SQL Server connection string |
| `ConnectionStrings__AppDb` | `Host=localhost;Port=5432;Database=AppDb;Username=postgres;Password=...` | PostgreSQL connection string |
| `Jwt__Secret` | *(long random string)* | HMAC-SHA256 signing key for JWTs |
| `Jwt__Issuer` | `KeyBasedAuthenticationTemplate` | JWT `iss` claim |
| `Jwt__Audience` | `KeyBasedAuthenticationTemplate.Client` | JWT `aud` claim |
| `Jwt__ExpiryMinutes` | `60` | Access token lifetime |
| `ApiKey__Prefix` | `kbat` | Prefix used in generated API keys (`{prefix}_{keyId}.{secret}`) |

## Error response format

Business-rule failures return [`ProblemDetails`](https://datatracker.ietf.org/doc/html/rfc7807) JSON with the appropriate status code:

```json
// 409 Conflict - duplicate registration
{ "title": "A user with that email or username already exists.", "status": 409, "instance": "/api/auth/register" }

// 401 Unauthorized - bad credentials
{ "title": "Invalid credentials.", "status": 401, "instance": "/api/auth/login" }
```

`[ApiController]`'s built-in model validation handles malformed request bodies (missing fields, bad email format, etc.) and returns a standard `400` validation-errors payload automatically.

## Extending this template

- **Add a new field to `Products`**: edit `Entities/Product.cs`, then `dotnet ef migrations add <Name> --context AppDbContext -o Data/Migrations/AppDb` — snake_case naming is applied automatically.
- **Add a new SQL Server-backed entity**: add it to `Entities/`, register a `DbSet` + Fluent config in `AuthDbContext.OnModelCreating`, then generate a migration with `--context AuthDbContext`.
- **Add a new role**: extend the `UserRole` enum in `Entities/UserRole.cs` — `[Authorize(Roles = nameof(UserRole.YourRole))]` works immediately, no other wiring needed.
- **Add a new protected resource**: follow the `Products` pattern — `Entities/` → `Repository/Interfaces` + `Repository/DB` → `Services/Interfaces` + `Services` → `Controllers`, registering the new repository/service in `Extensions/ApplicationServiceExtensions.cs`.
