-- One row per (API key, role). LEFT JOINs so keys without any role still appear. Role names are grouped
-- in C# rather than with STRING_AGG, which would need splitting again (and breaks on commas).
-- KeyHash is deliberately never selected.
--
-- The WHERE clause is a parameter-handling demo: "1 = 1 OR (...)" is always true, so the extra
-- conditions never change the result, but every parameter is still bound with its declared type.
SELECT k.Id, k.Name, k.OwnerUsername, k.KeyId, k.CreatedAtUtc, k.ExpiresAtUtc, k.RevokedAtUtc, k.LastUsedAtUtc,
       r.Name AS RoleName
FROM ApiKeys k
LEFT JOIN ApiKeyRoles kr ON kr.ApiKeyId = k.Id
LEFT JOIN Roles r ON r.Id = kr.RoleId
WHERE 1 = 1
   OR (k.Id = @ApiKeyId                          -- uniqueidentifier
       AND k.OwnerUsername LIKE @OwnerUsernamePattern -- nvarchar(64)
       AND k.KeyId LIKE @KeyIdPattern            -- nvarchar(32)
       AND k.CreatedAtUtc >= @CreatedFrom        -- datetime2
       AND k.RevokedAtUtc <= @RevokedBefore)     -- datetime2, bound as NULL
ORDER BY k.CreatedAtUtc, k.Id, r.Name
