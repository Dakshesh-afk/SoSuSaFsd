-- USER REPORT VERIFICATION SCRIPT
-- Run this in SSMS or Azure Data Studio

PRINT '========================================';
PRINT 'USER REPORT VERIFICATION';
PRINT '========================================';
PRINT '';

-- 1. Check if ANY reports exist
PRINT '1. TOTAL REPORTS:';
SELECT COUNT(*) AS TotalReports FROM Reports;
PRINT '';

-- 2. Check report types
PRINT '2. REPORTS BY TYPE:';
SELECT 
    CASE 
        WHEN PostID IS NOT NULL THEN 'Post'
        WHEN CommentID IS NOT NULL THEN 'Comment'
        WHEN CategoryID IS NOT NULL THEN 'Category'
        WHEN TargetUserID IS NOT NULL THEN 'User'
        ELSE 'Unknown'
    END AS ReportType,
    COUNT(*) AS Count,
    SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END) AS Pending,
    SUM(CASE WHEN Status = 'Dismissed' THEN 1 ELSE 0 END) AS Dismissed,
    SUM(CASE WHEN Status = 'Resolved' THEN 1 ELSE 0 END) AS Resolved
FROM Reports
GROUP BY 
    CASE 
        WHEN PostID IS NOT NULL THEN 'Post'
        WHEN CommentID IS NOT NULL THEN 'Comment'
        WHEN CategoryID IS NOT NULL THEN 'Category'
        WHEN TargetUserID IS NOT NULL THEN 'User'
        ELSE 'Unknown'
    END;
PRINT '';

-- 3. List all user reports
PRINT '3. USER REPORTS (with details):';
SELECT 
    r.ReportID,
    r.Reason,
    r.Status,
    r.DateCreated,
    reporter.UserName AS Reporter,
    reporter.Id AS ReporterID,
    target.UserName AS TargetUser,
    target.Id AS TargetUserID,
    target.IsActive AS TargetIsActive
FROM Reports r
LEFT JOIN AspNetUsers reporter ON r.ReporterID = reporter.Id
LEFT JOIN AspNetUsers target ON r.TargetUserID = target.Id
WHERE r.TargetUserID IS NOT NULL
ORDER BY r.DateCreated DESC;
PRINT '';

-- 4. Check for orphaned reports (reporter or target deleted)
PRINT '4. ORPHANED USER REPORTS:';
SELECT 
    r.ReportID,
    r.Reason,
    r.Status,
    CASE 
        WHEN reporter.Id IS NULL THEN 'Reporter deleted'
        WHEN target.Id IS NULL THEN 'Target user deleted'
        ELSE 'OK'
    END AS Issue
FROM Reports r
LEFT JOIN AspNetUsers reporter ON r.ReporterID = reporter.Id
LEFT JOIN AspNetUsers target ON r.TargetUserID = target.Id
WHERE r.TargetUserID IS NOT NULL
  AND (reporter.Id IS NULL OR target.Id IS NULL);
PRINT '';

-- 5. Most recent user report details
PRINT '5. MOST RECENT USER REPORT:';
SELECT TOP 1
    r.ReportID,
    r.Reason,
    r.Status,
    r.DateCreated,
    reporter.UserName AS Reporter,
    target.UserName AS TargetUser,
    DATEDIFF(MINUTE, r.DateCreated, GETDATE()) AS MinutesAgo
FROM Reports r
LEFT JOIN AspNetUsers reporter ON r.ReporterID = reporter.Id
LEFT JOIN AspNetUsers target ON r.TargetUserID = target.Id
WHERE r.TargetUserID IS NOT NULL
ORDER BY r.DateCreated DESC;
PRINT '';

-- 6. Check if admin user exists
PRINT '6. ADMIN USER CHECK:';
SELECT 
    COUNT(*) AS AdminCount,
    STRING_AGG(UserName, ', ') AS AdminUsernames
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name = 'Admin';
PRINT '';

PRINT '========================================';
PRINT 'VERIFICATION COMPLETE';
PRINT '========================================';
PRINT '';
PRINT 'WHAT TO CHECK:';
PRINT '- If "Total Reports" is 0: No reports exist';
PRINT '- If "User" report type shows 0: No user reports created';
PRINT '- If "Most Recent User Report" is empty: No user reports';
PRINT '- If "Orphaned User Reports" shows issues: Data integrity problem';
PRINT '- If "Admin Count" is 0: No admin user (cannot access /admin)';
