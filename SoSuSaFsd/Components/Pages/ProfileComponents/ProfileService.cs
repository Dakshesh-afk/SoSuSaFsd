using Microsoft.EntityFrameworkCore;
using SoSuSaFsd.Data;
using SoSuSaFsd.Domain;

namespace SoSuSaFsd.Components.Pages.ProfileComponents
{
    /// <summary>
    /// Service interface for profile-specific operations
    /// </summary>
    public interface IProfileService
    {
        Task<Users?> GetUserByUsernameAsync(string username);
        Task<int> GetFollowerCountAsync(string userId);
        Task<bool> SubmitUserReportAsync(string targetUserId, string reporterId, string reason);
    }

    /// <summary>
    /// Service for profile page operations
    /// Handles user profile data retrieval and user reporting
    /// </summary>
    public class ProfileService : IProfileService
    {
        private readonly IDbContextFactory<SoSuSaFsdContext> _dbFactory;
        private readonly ILogger<ProfileService> _logger;

        public ProfileService(
            IDbContextFactory<SoSuSaFsdContext> dbFactory,
            ILogger<ProfileService> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        /// <summary>
        /// Gets a user by their username
        /// </summary>
        /// <param name="username">Username to search for</param>
        /// <returns>User if found, null otherwise</returns>
        public async Task<Users?> GetUserByUsernameAsync(string username)
        {
            try
            {
                using var context = _dbFactory.CreateDbContext();
                return await context.Users
                    .FirstOrDefaultAsync(u => u.UserName == username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by username: {Username}", username);
                return null;
            }
        }

        /// <summary>
        /// Calculates the number of followers a user has
        /// Followers are users who follow categories created by this user
        /// </summary>
        /// <param name="userId">User ID to count followers for</param>
        /// <returns>Number of unique followers</returns>
        public async Task<int> GetFollowerCountAsync(string userId)
        {
            try
            {
                using var context = _dbFactory.CreateDbContext();
                
                // Get all categories created by this user
                var userCategoryIds = await context.Categories
                    .Where(c => c.CreatedBy == userId)
                    .Select(c => c.Id)
                    .ToListAsync();

                // Count unique users following these categories
                var followerCount = await context.CategoryFollows
                    .Where(cf => userCategoryIds.Contains(cf.CategoryId))
                    .Select(cf => cf.UserId)
                    .Distinct()
                    .CountAsync();

                return followerCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting follower count for user: {UserId}", userId);
                return 0;
            }
        }

        /// <summary>
        /// Submits a report against a user
        /// </summary>
        /// <param name="targetUserId">User being reported</param>
        /// <param name="reporterId">User submitting the report</param>
        /// <param name="reason">Reason for the report</param>
        /// <returns>True if report was submitted, false if duplicate</returns>
        public async Task<bool> SubmitUserReportAsync(string targetUserId, string reporterId, string reason)
        {
            try
            {
                using var context = _dbFactory.CreateDbContext();

                // Check for existing report
                var existingReport = await context.Reports
                    .FirstOrDefaultAsync(r => 
                        r.TargetUserID == targetUserId && 
                        r.ReporterID == reporterId);

                if (existingReport != null)
                {
                    _logger.LogInformation("User {ReporterId} already reported user {TargetUserId}", 
                        reporterId, targetUserId);
                    return false;
                }

                // Create new report
                var newReport = new Reports
                {
                    TargetUserID = targetUserId,
                    ReporterID = reporterId,
                    Reason = reason,
                    Status = "Pending",
                    DateCreated = DateTime.Now
                };

                context.Reports.Add(newReport);
                await context.SaveChangesAsync();

                _logger.LogInformation("User report submitted: Reporter={ReporterId}, Target={TargetUserId}", 
                    reporterId, targetUserId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting user report: Reporter={ReporterId}, Target={TargetUserId}", 
                    reporterId, targetUserId);
                throw;
            }
        }
    }
}
