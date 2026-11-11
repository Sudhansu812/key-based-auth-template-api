# API-key-only auth: change summary

Branch: `feature/api-key-only-auth` (local only, never pushed)
Base: `39493f3` (feature/report-automation)
Commits: `b7883b3` (main change), `2b13bfa` (reset script fallback)

This branch removes user auth (users, passwords, JWT) and keeps API keys as the only credential. Keys get an `OwnerUsername` column and any number of roles through a new `ApiKeyRoles` join table, which replaces the `AccessLevel` column.

**Impact levels**
- **High:** changes authentication, authorization, the database schema or what an endpoint allows. Review these first.
- **Medium:** feature logic, contracts or queries that changed shape (users → API keys, user id → owner username).
- **Low:** wiring, renames, comments, configuration and docs.

Totals (git, base → HEAD): 51 modified, 8 added, 26 removed, 6 renamed or moved.

---

## High impact

| Status | File | What changed |
|---|---|---|
| Modified | `KeyBasedAuthenticationTemplate_API/Authentication/ApiKeyAuthenticationHandler.cs` | Loads the key's roles; issues one role claim per role plus an `owner_username` claim (no more `access_level`) |
| Modified | `KeyBasedAuthenticationTemplate_API/Extensions/AuthenticationServiceExtensions.cs` | JWT and the JWT-or-key policy scheme removed; `ApiKey` is the only scheme; role-based policies |
| Added | `KeyBasedAuthenticationTemplate_API/Authorization/Policies.cs` | `ReadAccess` / `WriteAccess` / `ReportAccess` policy names |
| Modified | `KeyBasedAuthenticationTemplate_API/Common/SystemRoleNames.cs` | System roles are now `Admin`, `Reader`, `Writer`, `Reporter` |
| Modified | `KeyBasedAuthenticationTemplate_API/Common/ClaimsPrincipalExtensions.cs` | `GetApiKeyId()` / `GetOwnerUsername()` / `IsAdmin()`; user helpers removed |
| Modified | `KeyBasedAuthenticationTemplate_API/Controllers/ApiKeysController.cs` | Admin-role keys manage keys; new `PUT /api/apikeys/{id}/roles` |
| Modified | `KeyBasedAuthenticationTemplate_API/Services/ApiKeyService.cs` | Create with owner and roles, replace roles, revoke; refuses (409) to revoke or demote the last active Admin key |
| Modified | `KeyBasedAuthenticationTemplate_API/Controllers/ProductsController.cs` | Role policies; ownership by owner username; Admin and Reporter can read all owners' products |
| Modified | `KeyBasedAuthenticationTemplate_API/Services/ProductService.cs` | Ownership checks use the owner username instead of a user id |
| Modified | `KeyBasedAuthenticationTemplate_API/Entities/ApiKey.cs` | `AccessLevel` removed; `OwnerUsername` and `Roles` added |
| Modified | `KeyBasedAuthenticationTemplate_API/Entities/Role.cs` | Linked to `ApiKeys` instead of `Users`; `Description` added |
| Modified | `KeyBasedAuthenticationTemplate_API/Entities/Product.cs` | `OwnerUserId` (Guid) → `OwnerUsername` (string) |
| Modified | `KeyBasedAuthenticationTemplate_API/Data/AuthDbContext.cs` | `Users`/`UserRoles` removed; `ApiKeyRoles` many-to-many added |
| Modified | `KeyBasedAuthenticationTemplate_API/Data/AppDbContext.cs` | `owner_username` column and index |
| Renamed | `Data/Migrations/AuthDb/20261003165711_InitialCreate*.cs` → `20261111045240_InitialCreate*.cs` | Regenerated AuthDb schema (dev databases must be reset) |
| Renamed | `Data/Migrations/AppDb/20261003161533_InitialCreate*.cs` → `20261111045244_InitialCreate*.cs` | Regenerated AppDb schema |
| Modified | `Data/Migrations/AuthDb/AuthDbContextModelSnapshot.cs`, `Data/Migrations/AppDb/AppDbContextModelSnapshot.cs` | Snapshots for the new schema |
| Modified | `KeyBasedAuthenticationTemplate_API/Data/Seed/DbInitializer.cs` | Seeds 4 roles, 4 fixed dev API keys and 7 products, and creates the `stg` table |
| Modified | `KeyBasedAuthenticationTemplate_API/Repository/DB/ApiKeyRepository.cs`, `Repository/Interfaces/IApiKeyRepository.cs` | Queries include roles; active-Admin count for the lockout guard |
| Removed | `KeyBasedAuthenticationTemplate_API/Controllers/AuthController.cs` | `/api/auth/register` and `/api/auth/login` are gone |
| Removed | `KeyBasedAuthenticationTemplate_API/Controllers/UsersController.cs` | `/api/users` endpoints are gone |
| Removed | `KeyBasedAuthenticationTemplate_API/Services/AuthService.cs`, `Services/Interfaces/IAuthService.cs` | Login and registration logic |
| Removed | `KeyBasedAuthenticationTemplate_API/Services/UserService.cs`, `Services/Interfaces/IUserService.cs` | User listing and role assignment |
| Removed | `KeyBasedAuthenticationTemplate_API/Helpers/JwtTokenGenerator.cs`, `Helpers/Interfaces/IJwtTokenGenerator.cs` | JWT issuing |
| Removed | `KeyBasedAuthenticationTemplate_API/Entities/User.cs` | User entity / `Users` table |
| Removed | `KeyBasedAuthenticationTemplate_API/Entities/ApiKeyAccessLevel.cs` | `ReadOnly`/`ReadWrite` enum, replaced by roles |

## Medium impact

| Status | File | What changed |
|---|---|---|
| Modified | `KeyBasedAuthenticationTemplate_API/Controllers/ReportsController.cs` | Users reports replaced by `ef/apikeys` and `dapper/apikeys` |
| Modified | `KeyBasedAuthenticationTemplate_API/Services/ReportService.cs`, `Services/Interfaces/IReportService.cs` | Users report → API keys report |
| Renamed | `Repository/DapperDB/UserDapperRepository.cs` → `Repository/DapperDB/ApiKeyDapperRepository.cs` | Dapper API keys report (same parameter-binding demo) |
| Added | `KeyBasedAuthenticationTemplate_API/Repository/Interfaces/IApiKeyDapperRepository.cs` | Interface for the above |
| Added | `KeyBasedAuthenticationTemplate_API/Sql/SqlServer/ApiKeysWithRoles.sql` | Keys with their roles (never selects the hash) |
| Added | `KeyBasedAuthenticationTemplate_API/Sql/SqlServer/CreateStagingTables.sql` | Idempotent `stg.ProductRiskSummary` creation, run by the dev seed |
| Modified | `KeyBasedAuthenticationTemplate_API/Sql/Postgres/ProductsAll.sql`, `Sql/Postgres/ProductRiskSummary.sql` | `owner_user_id` → `owner_username` |
| Modified | `KeyBasedAuthenticationTemplate_API/Repository/DapperDB/ProductDapperRepository.cs`, `Repository/DapperDB/NoOpFilterValues.cs` | Owner parameter bound as text; API key filter values |
| Modified | `KeyBasedAuthenticationTemplate_API/Repository/DB/ProductRepository.cs`, `Repository/Interfaces/IProductRepository.cs` | Filter by owner username |
| Modified | `KeyBasedAuthenticationTemplate_API/Services/Interfaces/IProductService.cs`, `Services/Interfaces/IApiKeyService.cs` | Signatures follow the service changes |
| Modified | `KeyBasedAuthenticationTemplate_API/Repository/DB/RoleRepository.cs`, `Repository/Interfaces/IRoleRepository.cs` | Look up roles by name instead of by id |
| Modified | `KeyBasedAuthenticationTemplate_API/DTOs/ApiKeys/CreateApiKeyRequest.cs` | `ownerUsername` and `roles[]` replace `accessLevel` |
| Modified | `KeyBasedAuthenticationTemplate_API/DTOs/ApiKeys/ApiKeyCreatedResponse.cs` | Returns owner and roles |
| Added | `KeyBasedAuthenticationTemplate_API/DTOs/ApiKeys/AssignApiKeyRolesRequest.cs` | Body for `PUT /api/apikeys/{id}/roles` |
| Moved | `API/DTOs/ApiKeys/ApiKeySummaryResponse.cs` → `KeyBasedAuthenticationTemplate_Contracts/ApiKeys/ApiKeySummaryResponse.cs` | Shared with Automation; owner and roles added |
| Modified | `KeyBasedAuthenticationTemplate_Contracts/Products/ProductResponse.cs` | `OwnerUserId` → `OwnerUsername` |
| Added | `KeyBasedAuthenticationTemplate_Automation/Reports/Definitions/ApiKeysEfReport.cs`, `ApiKeysDapperReport.cs` | New Automation reports |
| Modified | `KeyBasedAuthenticationTemplate_Automation/Reports/Definitions/ReportColumns.cs` | API keys columns; "Owner Username" product column |
| Modified | `KeyBasedAuthenticationTemplate_Automation/Program.cs`, `KeyBasedAuthenticationTemplate_Automation/appsettings.json` | Registers and configures `apikeys-ef` / `apikeys-dapper` |
| Added | `reset-dev-db.ps1` | Drops both dev databases (or reverts their migrations) so they re-seed |
| Removed | `KeyBasedAuthenticationTemplate_API/Authorization/WriteAccessRequirement.cs`, `WriteAccessAuthorizationHandler.cs` | Replaced by role policies |
| Removed | `KeyBasedAuthenticationTemplate_API/Authorization/ReportAccessRequirement.cs`, `ReportAccessAuthorizationHandler.cs` | Replaced by role policies |
| Removed | `KeyBasedAuthenticationTemplate_API/Repository/DB/UserRepository.cs`, `Repository/Interfaces/IUserRepository.cs`, `Repository/Interfaces/IUserDapperRepository.cs` | User data access |
| Removed | `KeyBasedAuthenticationTemplate_API/Sql/SqlServer/UsersWithRoles.sql` | Users report query |
| Removed | `KeyBasedAuthenticationTemplate_API/DTOs/Auth/AuthResponse.cs`, `LoginRequest.cs`, `RegisterRequest.cs` | Login/registration DTOs |
| Removed | `KeyBasedAuthenticationTemplate_API/DTOs/Users/AssignUserRolesRequest.cs` | User role assignment DTO |
| Removed | `KeyBasedAuthenticationTemplate_Contracts/Users/UserSummaryResponse.cs` | Users report contract |
| Removed | `KeyBasedAuthenticationTemplate_Automation/Reports/Definitions/UsersEfReport.cs`, `UsersDapperReport.cs` | Users Automation reports |

## Low impact

| Status | File | What changed |
|---|---|---|
| Modified | `KeyBasedAuthenticationTemplate_API/Authentication/ApiKeyAuthenticationOptions.cs` | `owner_username` claim type replaces `access_level` |
| Modified | `KeyBasedAuthenticationTemplate_API/Controllers/NotificationsController.cs`, `Controllers/StagingController.cs` | Use the new policy names (same rules: Reporter + Writer) |
| Modified | `KeyBasedAuthenticationTemplate_API/Services/RoleService.cs`, `DTOs/Roles/CreateRoleRequest.cs`, `DTOs/Roles/RoleResponse.cs` | Optional role `description` |
| Modified | `KeyBasedAuthenticationTemplate_API/Extensions/ApplicationServiceExtensions.cs` | DI: user/JWT/password-hasher registrations removed, API key Dapper repo added |
| Modified | `KeyBasedAuthenticationTemplate_API/OpenApi/SecuritySchemeTransformer.cs` | Bearer scheme removed from the Scalar docs |
| Modified | `KeyBasedAuthenticationTemplate_API/Data/IDbConnectionFactory.cs` | Comment only |
| Modified | `KeyBasedAuthenticationTemplate_API/KeyBasedAuthenticationTemplate_API.csproj` | `Microsoft.AspNetCore.Authentication.JwtBearer` package removed |
| Modified | `KeyBasedAuthenticationTemplate_API/appsettings.json`, `Properties/launchSettings.json.example`, `env/.example.env` | `Jwt` settings removed; `.example.env` has the dev automation key |
| Modified | `README.md` | Rewritten for API-key-only auth, roles, dev keys and the reset steps |
| Removed | `KeyBasedAuthenticationTemplate_API/Configuration/JwtSettings.cs` | JWT options class |

Not committed and left as they were: your 1-line whitespace change in `KeyBasedAuthenticationTemplate_API/Program.cs`, `KeyBasedAuthenticationTemplate_API/KeyBasedAuthenticationTemplate_API.md`, `KeyBasedAuthenticationTemplate_API/tomd.sh`, and this file.

---

## Table creation scripts

These are generated from the EF Core migrations (`dotnet ef migrations script`). In Development the API applies them itself on startup. Use these scripts to create the schema by hand, for example on a server where the app doesn't run migrations.

### SQL Server: AuthDb (roles, API keys)

```sql
CREATE TABLE [Roles] (
    [Id]           uniqueidentifier NOT NULL,
    [Name]         nvarchar(64)     NOT NULL,
    [Description]  nvarchar(256)    NULL,
    [IsSystemRole] bit              NOT NULL,
    [CreatedAtUtc] datetime2        NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
);

CREATE TABLE [ApiKeys] (
    [Id]            uniqueidentifier NOT NULL,
    [Name]          nvarchar(128)    NOT NULL,
    [OwnerUsername] nvarchar(64)     NOT NULL,
    [KeyId]         nvarchar(32)     NOT NULL,
    [KeyHash]       nvarchar(128)    NOT NULL,
    [CreatedAtUtc]  datetime2        NOT NULL,
    [ExpiresAtUtc]  datetime2        NULL,
    [RevokedAtUtc]  datetime2        NULL,
    [LastUsedAtUtc] datetime2        NULL,
    CONSTRAINT [PK_ApiKeys] PRIMARY KEY ([Id])
);

CREATE TABLE [ApiKeyRoles] (
    [ApiKeyId] uniqueidentifier NOT NULL,
    [RoleId]   uniqueidentifier NOT NULL,
    CONSTRAINT [PK_ApiKeyRoles] PRIMARY KEY ([ApiKeyId], [RoleId]),
    CONSTRAINT [FK_ApiKeyRoles_ApiKeys_ApiKeyId] FOREIGN KEY ([ApiKeyId]) REFERENCES [ApiKeys] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ApiKeyRoles_Roles_RoleId]     FOREIGN KEY ([RoleId])   REFERENCES [Roles] ([Id])   ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_Roles_Name]           ON [Roles] ([Name]);
CREATE UNIQUE INDEX [IX_ApiKeys_KeyId]        ON [ApiKeys] ([KeyId]);
CREATE INDEX        [IX_ApiKeys_OwnerUsername] ON [ApiKeys] ([OwnerUsername]);
CREATE INDEX        [IX_ApiKeyRoles_RoleId]   ON [ApiKeyRoles] ([RoleId]);
```

### SQL Server: AuthDb staging table (not managed by EF)

The same script as `Sql/SqlServer/CreateStagingTables.sql`. It is idempotent.

```sql
IF SCHEMA_ID(N'stg') IS NULL
    EXEC(N'CREATE SCHEMA stg');

IF OBJECT_ID(N'stg.ProductRiskSummary', N'U') IS NULL
    CREATE TABLE stg.ProductRiskSummary (
        SnapshotAtUtc  DATETIME2(0)  NOT NULL CONSTRAINT DF_stg_ProductRiskSummary_SnapshotAtUtc DEFAULT SYSUTCDATETIME(),
        RiskLevel      NVARCHAR(64)  NOT NULL,
        CostBand       NVARCHAR(16)  NOT NULL,
        StockBand      NVARCHAR(16)  NOT NULL,
        ProductCount   INT           NOT NULL,
        TotalUnits     BIGINT        NOT NULL,
        InventoryValue DECIMAL(18,2) NOT NULL
    );
```

### PostgreSQL: AppDb (products)

```sql
CREATE TABLE products (
    id             uuid                     NOT NULL,
    name           character varying(256)   NOT NULL,
    description    text,
    price          numeric(18,2)            NOT NULL,
    stock          integer                  NOT NULL,
    owner_username character varying(64)    NOT NULL,
    created_at_utc timestamp with time zone NOT NULL,
    updated_at_utc timestamp with time zone,
    CONSTRAINT pk_products PRIMARY KEY (id)
);

CREATE INDEX ix_products_owner_username ON products (owner_username);
```

### Optional: mark the migrations as applied

Only needed if you create the tables by hand **and** later run the app with migrations enabled. Without these rows, EF would try to create the tables again.

```sql
-- SQL Server (AuthDb)
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId]    nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32)  NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261111045240_InitialCreate', N'10.0.13');
```

```sql
-- PostgreSQL (AppDb)
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    migration_id    character varying(150) NOT NULL,
    product_version character varying(32)  NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);
INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES ('20261111045244_InitialCreate', '10.0.13');
```

---

## Role insertion scripts (SQL Server, AuthDb)

These are the four system roles the authorization policies depend on. The names must match `Common/SystemRoleNames.cs` exactly. The script is idempotent: it only inserts roles that don't exist yet.

```sql
INSERT INTO [Roles] ([Id], [Name], [Description], [IsSystemRole], [CreatedAtUtc])
SELECT NEWID(), v.[Name], v.[Description], 1, SYSUTCDATETIME()
FROM (VALUES
    (N'Admin',    N'Manages API keys and roles; full access to every owner''s products and all endpoints.'),
    (N'Reader',   N'Reads the products of its own owner.'),
    (N'Writer',   N'Creates, updates and deletes the products of its own owner; sends notifications and loads staging (with Reporter).'),
    (N'Reporter', N'Report endpoints, notifications and staging; reads every owner''s products.')
) AS v([Name], [Description])
WHERE NOT EXISTS (SELECT 1 FROM [Roles] r WHERE r.[Name] = v.[Name]);
```

To give an existing key a role by hand (the API's `PUT /api/apikeys/{id}/roles` does the same):

```sql
INSERT INTO [ApiKeyRoles] ([ApiKeyId], [RoleId])
SELECT k.[Id], r.[Id]
FROM [ApiKeys] k
JOIN [Roles] r ON r.[Name] = N'Reader'           -- role to grant
WHERE k.[KeyId] = N'<keyId>'                      -- the part between "kbat_" and "."
  AND NOT EXISTS (SELECT 1 FROM [ApiKeyRoles] kr WHERE kr.[ApiKeyId] = k.[Id] AND kr.[RoleId] = r.[Id]);
```
