-- Staging tables are not managed by EF migrations. DbInitializer runs this (Development only) so a
-- freshly dropped/recreated AuthDb is immediately usable; it is idempotent and safe to re-run.
-- Outside Development, run it once by hand against the AuthDb database.
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
