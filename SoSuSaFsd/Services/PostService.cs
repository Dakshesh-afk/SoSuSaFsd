using SoSuSaFsd.Data;
using SoSuSaFsd.Domain;
using Microsoft.EntityFrameworkCore;

namespace SoSuSaFsd.Services
{
    public interface IPostService
    {
        // Posts
        Task<List<Posts>> GetCategoryPostsAsync(int categoryId);
        Task<List<Posts>> GetUserFeedPostsAsync(List<int> followedCategoryIds);
        Task<List<Posts>> GetUserPostsAsync(string userId);
        Task CreatePostAsync(Posts post);
        
        // Likes
        Task<bool> ToggleLikeAsync(int postId, string userId);
        Task<int> GetLikeCountAsync(int postId);
        
        // Comments
        Task<List<Comments>> GetPostCommentsAsync(int postId);
        Task CreateCommentAsync(Comments comment);
        
        // Reports
        Task<bool> HasUserReportedAsync(int postId, string userId);
        Task CreateReportAsync(Reports report);
        Task<bool> IsReportDismissedAsync(int postId);
    }

    public class PostService : IPostService
    {
        private readonly IDbContextFactory<SoSuSaFsdContext> _contextFactory;

        public PostService(IDbContextFactory<SoSuSaFsdContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // ========== POSTS ==========
        public async Task<List<Posts>> GetCategoryPostsAsync(int categoryId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Posts
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Media)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.DateCreated)
                .ToListAsync();
        }

        public async Task<List<Posts>> GetUserFeedPostsAsync(List<int> followedCategoryIds)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Posts
                .Where(p => followedCategoryIds.Contains(p.CategoryId))
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Media)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.DateCreated)
                .ToListAsync();
        }

        public async Task<List<Posts>> GetUserPostsAsync(string userId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Posts
                .Where(p => p.UserId == userId)
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Media)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.DateCreated)
                .ToListAsync();
        }

        public async Task CreatePostAsync(Posts post)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Posts.Add(post);
            await context.SaveChangesAsync();
        }

        // ========== LIKES ==========
        public async Task<bool> ToggleLikeAsync(int postId, string userId)
        {
            using var context = _contextFactory.CreateDbContext();
            var existingLike = await context.PostLikes
                .FirstOrDefaultAsync(l => l.PostID == postId && l.UserID == userId);

            if (existingLike != null)
            {
                context.PostLikes.Remove(existingLike);
                await context.SaveChangesAsync();
                return false; // Unliked
            }
            else
            {
                var newLike = new PostLikes
                {
                    PostID = postId,
                    UserID = userId,
                    LikedAt = DateTime.Now
                };
                context.PostLikes.Add(newLike);
                await context.SaveChangesAsync();
                return true; // Liked
            }
        }

        public async Task<int> GetLikeCountAsync(int postId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.PostLikes.CountAsync(l => l.PostID == postId);
        }

        // ========== COMMENTS ==========
        public async Task<List<Comments>> GetPostCommentsAsync(int postId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Comments
                .Where(c => c.PostID == postId)
                .Include(c => c.User)
                .OrderBy(c => c.DateCreated)
                .ToListAsync();
        }

        public async Task CreateCommentAsync(Comments comment)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Comments.Add(comment);
            await context.SaveChangesAsync();
        }

        // ========== REPORTS ==========
        public async Task<bool> HasUserReportedAsync(int postId, string userId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Reports.AnyAsync(r =>
                r.PostID == postId && r.ReporterID == userId);
        }

        public async Task CreateReportAsync(Reports report)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Reports.Add(report);
            await context.SaveChangesAsync();
        }

        public async Task<bool> IsReportDismissedAsync(int postId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Reports.AnyAsync(r =>
                r.PostID == postId && r.Status == "Dismissed");
        }
    }
}
