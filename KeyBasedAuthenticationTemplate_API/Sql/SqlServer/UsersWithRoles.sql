-- One row per (user, role). LEFT JOINs so users without any role still appear. Role names are grouped
-- in C# rather than with STRING_AGG, which would need splitting again (and breaks on commas).
--
-- The WHERE clause is a parameter-handling demo: "1 = 1 OR (...)" is always true, so the extra
-- conditions never change the result, but every parameter is still bound with its declared type.
SELECT u.Id, u.Email, u.Username, u.IsActive, u.CreatedAtUtc, r.Name AS RoleName
FROM Users u
LEFT JOIN UserRoles ur ON ur.UserId = u.Id
LEFT JOIN Roles r ON r.Id = ur.RoleId
WHERE 1 = 1
   OR (u.Id = @UserId                 -- uniqueidentifier
       AND u.Email LIKE @EmailPattern -- nvarchar(256)
       AND u.IsActive = @IsActive     -- bit
       AND u.CreatedAtUtc >= @CreatedFrom   -- datetime2
       AND u.UpdatedAtUtc <= @UpdatedBefore) -- datetime2, bound as NULL
ORDER BY u.CreatedAtUtc, u.Id, r.Name
