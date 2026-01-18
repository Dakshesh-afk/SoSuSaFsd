using SoSuSaFsd.Data;
using SoSuSaFsd.Domain;
using Microsoft.EntityFrameworkCore;

namespace SoSuSaFsd.Services
{
    public interface IAdminService
    {
        Task<List<Users>> GetAllUsersAsync();
        Task<List<Categories>> GetAllCategoriesAsync();
        Task<List<CategoryAccessRequests>> GetAccessRequestsAsync();
        Task<List<Reports>> GetAllReportsAsync();
        Task<Dictionary<string, List<Categories>>> GetUserVerifiedCategoriesMapAsync();
        Task RemoveUserCategoryAccessAsync(string userId, int categoryId);
        Task DismissReportGroupAsync(int reportId);
        Task UndoDismissAsync(int reportId);
        Task DeleteReportedContentAsync(int reportId);
        Task DeleteCategoryAsync(int categoryId);
        Task<bool> ToggleCategoryVerificationAsync(int categoryId);
        Task ApproveAccessRequestAsync(int requestId);
        Task RejectAccessRequestAsync(int requestId);
        Task<bool> ToggleUserBanAsync(string userId);
    }

    public class AdminService : IAdminService
    {
        private readonly IDbContextFactory<SoSuSaFsdContext> _contextFactory;

        public AdminService(IDbContextFactory<SoSuSaFsdContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Users>> GetAllUsersAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Users.ToListAsync();
        }

        public async Task<List<Categories>> GetAllCategoriesAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Categories.ToListAsync();
        }

        public async Task<List<CategoryAccessRequests>> GetAccessRequestsAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.CategoryAccessRequests
                .Include(r => r.User)
                .Include(r => r.Category)
                .OrderByDescending(r => r.DateCreated)
                .ToListAsync();
        }

        public async Task<List<Reports>> GetAllReportsAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.Post).ThenInclude(p => p.User)
                .Include(r => r.Post).ThenInclude(p => p.Media)
                .Include(r => r.Comment)
                .Include(r => r.Category)
                .Include(r => r.TargetUser)
                .OrderByDescending(r => r.DateCreated)
                .ToListAsync();
        }

        public async Task<Dictionary<string, List<Categories>>> GetUserVerifiedCategoriesMapAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            
            // Get categories from approved access requests
            var approvedRequests = await context.CategoryAccessRequests
                .Where(r => r.Status == "Approved")
                .Include(r => r.Category)
                .ToListAsync();

            var requestCategories = approvedRequests
                .Where(r => r.Category != null)
                .GroupBy(r => r.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => r.Category!).ToList()
                );

            // Get verified categories created by users
            var verifiedCategories = await context.Categories
                .Where(c => c.IsVerified == true && !string.IsNullOrEmpty(c.CreatedBy))
                .ToListAsync();

            var createdCategories = verifiedCategories
                .GroupBy(c => c.CreatedBy!)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList()
                );

            // Combine both: access requests + owned verified categories
            var result = new Dictionary<string, List<Categories>>();

            // Add from access requests
            foreach (var kvp in requestCategories)
            {
                result[kvp.Key] = kvp.Value;
            }

            // Add from created categories (avoid duplicates)
            foreach (var kvp in createdCategories)
            {
                if (result.ContainsKey(kvp.Key))
                {
                    // Add categories not already in the list
                    foreach (var category in kvp.Value)
                    {
                        if (!result[kvp.Key].Any(c => c.Id == category.Id))
                        {
                            result[kvp.Key].Add(category);
                        }
                    }
                }
                else
                {
                    result[kvp.Key] = kvp.Value;
                }
            }

            return result;
        }

        public async Task RemoveUserCategoryAccessAsync(string userId, int categoryId)
        {
            using var context = _contextFactory.CreateDbContext();
            
            // Check if user is the creator of this category
            var category = await context.Categories.FindAsync(categoryId);
            if (category != null && category.CreatedBy == userId)
            {
                // Cannot remove access for category creator
                // Category creators always have access to their own categories
                return;
            }
            
            // Remove access request if exists
            var request = await context.CategoryAccessRequests
                .FirstOrDefaultAsync(r => r.UserId == userId && r.CategoryId == categoryId && r.Status == "Approved");

            if (request != null)
            {
                context.CategoryAccessRequests.Remove(request);
                await context.SaveChangesAsync();
            }
        }

        public async Task DismissReportGroupAsync(int reportId)
        {
            using var context = _contextFactory.CreateDbContext();
            var mainReport = await context.Reports.FindAsync(reportId);
            if (mainReport == null) return;

            var relatedReports = await context.Reports.Where(r =>
                (mainReport.PostID != null && r.PostID == mainReport.PostID) ||
                (mainReport.CommentID != null && r.CommentID == mainReport.CommentID) ||
                (mainReport.CategoryID != null && r.CategoryID == mainReport.CategoryID) ||
                (mainReport.TargetUserID != null && r.TargetUserID == mainReport.TargetUserID)
            ).ToListAsync();

            foreach (var r in relatedReports)
            {
                r.Status = "Dismissed";
            }

            await context.SaveChangesAsync();
        }

        public async Task UndoDismissAsync(int reportId)
        {
            using var context = _contextFactory.CreateDbContext();
            var mainReport = await context.Reports.FindAsync(reportId);
            if (mainReport == null) return;

            var relatedReports = await context.Reports.Where(r =>
                (mainReport.PostID != null && r.PostID == mainReport.PostID) ||
                (mainReport.CommentID != null && r.CommentID == mainReport.CommentID) ||
                (mainReport.CategoryID != null && r.CategoryID == mainReport.CategoryID) ||
                (mainReport.TargetUserID != null && r.TargetUserID == mainReport.TargetUserID)
            ).ToListAsync();

            foreach (var r in relatedReports)
            {
                if (r.Status == "Dismissed")
                {
                    r.Status = "Pending";
                }
            }

            await context.SaveChangesAsync();
        }

        public async Task DeleteReportedContentAsync(int reportId)
        {
            using var context = _contextFactory.CreateDbContext();
            var report = await context.Reports.FindAsync(reportId);
            if (report == null) return;

            if (report.PostID != null)
            {
                int postId = report.PostID.Value;
                var relatedReports = await context.Reports.Where(r => r.PostID == postId).ToListAsync();

                foreach (var r in relatedReports)
                {
                    r.Status = "Resolved";
                    r.PostID = null;
                }

                var post = await context.Posts.FindAsync(postId);
                if (post != null) context.Posts.Remove(post);
            }
            else if (report.CommentID != null)
            {
                int commentId = report.CommentID.Value;
                var relatedReports = await context.Reports.Where(r => r.CommentID == commentId).ToListAsync();

                foreach (var r in relatedReports)
                {
                    r.Status = "Resolved";
                    r.CommentID = null;
                }

                var comment = await context.Comments.FindAsync(commentId);
                if (comment != null) context.Comments.Remove(comment);
            }

            await context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            using var context = _contextFactory.CreateDbContext();
            var category = await context.Categories.FindAsync(categoryId);
            if (category != null)
            {
                context.Categories.Remove(category);
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> ToggleCategoryVerificationAsync(int categoryId)
        {
            using var context = _contextFactory.CreateDbContext();
            var category = await context.Categories.FindAsync(categoryId);
            if (category == null) return false;

            category.IsVerified = !category.IsVerified;
            category.DateUpdated = DateTime.Now;
            await context.SaveChangesAsync();
            return category.IsVerified;
        }

        public async Task ApproveAccessRequestAsync(int requestId)
        {
            using var context = _contextFactory.CreateDbContext();
            var request = await context.CategoryAccessRequests.FindAsync(requestId);
            if (request != null)
            {
                request.Status = "Approved";
                request.DateUpdated = DateTime.Now;
                await context.SaveChangesAsync();
            }
        }

        public async Task RejectAccessRequestAsync(int requestId)
        {
            using var context = _contextFactory.CreateDbContext();
            var request = await context.CategoryAccessRequests.FindAsync(requestId);
            if (request != null)
            {
                request.Status = "Rejected";
                request.DateUpdated = DateTime.Now;
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> ToggleUserBanAsync(string userId)
        {
            using var context = _contextFactory.CreateDbContext();
            var user = await context.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsActive = !user.IsActive;
            user.DateUpdated = DateTime.Now;

            await context.SaveChangesAsync();
            return user.IsActive;
        }
    }
}