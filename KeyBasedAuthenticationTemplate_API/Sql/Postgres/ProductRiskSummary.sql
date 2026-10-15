-- Products grouped by cost band x stock band, with a risk level per group.
-- "High" cost = price >= @HighPrice, "High" stock = stock >= @HighStock; high cost + high stock is the
-- risky corner (expensive inventory sitting unsold).
--
-- Aliases are quoted so PostgreSQL keeps their case: they must match the destination column names in
-- stg.ProductRiskSummary, which SqlBulkCopy maps by name.
--
-- The WHERE clause is a parameter-handling demo: "1 = 1 OR (...)" is always true, so the extra
-- conditions never change the result, but every parameter is still bound with its declared type.
WITH t AS (
    SELECT price, stock, price * stock AS stock_value,
           price >= @HighPrice AS high_cost,
           stock >= @HighStock AS high_stock
    FROM products
    WHERE 1 = 1
       OR (price >= @MinPrice                 -- numeric(18,2)
           AND stock >= @MinStock             -- integer
           AND owner_user_id = @OwnerUserId   -- uuid
           AND name ILIKE @NamePattern        -- text
           AND created_at_utc >= @CreatedFrom -- timestamptz
           AND updated_at_utc <= @UpdatedBefore -- timestamptz, bound as NULL
           AND (stock > 0) = @InStockOnly)    -- boolean
)
SELECT @SnapshotAtUtc AS "SnapshotAtUtc",
       CASE WHEN high_cost AND high_stock THEN 'High risk'
            WHEN high_cost THEN 'Medium risk (costly, low stock)'
            WHEN high_stock THEN 'Medium risk (cheap, overstocked)'
            ELSE 'Low risk' END AS "RiskLevel",
       CASE WHEN high_cost THEN 'High' ELSE 'Low' END AS "CostBand",
       CASE WHEN high_stock THEN 'High' ELSE 'Low' END AS "StockBand",
       count(*)::int AS "ProductCount",
       sum(stock)::bigint AS "TotalUnits",
       round(sum(stock_value), 2) AS "InventoryValue"
FROM t
GROUP BY high_cost, high_stock
