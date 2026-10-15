-- All products, newest first. PostgreSQL uses snake_case; columns are aliased back to the DTO property names.
--
-- The WHERE clause is a parameter-handling demo: "1 = 1 OR (...)" is always true, so the extra
-- conditions never change the result, but every parameter is still bound with its declared type.
SELECT id AS Id,
       name AS Name,
       description AS Description,
       price AS Price,
       stock AS Stock,
       owner_user_id AS OwnerUserId,
       created_at_utc AS CreatedAtUtc,
       updated_at_utc AS UpdatedAtUtc
FROM products
WHERE 1 = 1
   OR (price >= @MinPrice                 -- numeric(18,2)
       AND stock >= @MinStock             -- integer
       AND owner_user_id = @OwnerUserId   -- uuid
       AND name ILIKE @NamePattern        -- text
       AND created_at_utc >= @CreatedFrom -- timestamptz
       AND updated_at_utc <= @UpdatedBefore -- timestamptz, bound as NULL
       AND (stock > 0) = @InStockOnly)    -- boolean
ORDER BY created_at_utc DESC
