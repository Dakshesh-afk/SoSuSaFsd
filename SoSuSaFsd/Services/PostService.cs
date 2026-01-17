using SoSuSaFsd.Data;
using SoSuSaFsd.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<PostService> _logger;
        private readonly IErrorHandlingService _errorHandler;

        public PostService(
            IDbContextFactory<SoSuSaFsdContext> contextFactory,
            ILogger<PostService> logger,
            IErrorHandlingService errorHandler)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            _errorHandler = errorHandler;
        }

        // ========== POSTS ==========
        public async Task<List<Posts>> GetCategoryPostsAsync(int categoryId)
        {
            try
            {
                _logger.LogInformation("Getting posts for category {CategoryId}", categoryId);
                
                using var context = _contextFactory.CreateDbContext();
                var posts = await context.Posts
                    .Where(p => p.CategoryId == categoryId)
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .Include(p => p.Media)
                    .Include(p => p.Likes)
                    .OrderByDescending(p => p.DateCreated)
                    .ToListAsync();
                    
                _logger.LogInformation("Retrieved {Count} posts for category {CategoryId}", posts.Count, categoryId);
                return posts;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"GetCategoryPostsAsync(categoryId: {categoryId})");
                throw;
            }
        }

        public async Task<List<Posts>> GetUserFeedPostsAsync(List<int> followedCategoryIds)
        {
            try
            {
                _logger.LogInformation("Getting feed posts for {Count} categories", followedCategoryIds.Count);
                
                using var context = _contextFactory.CreateDbContext();
                var posts = await context.Posts
                    .Where(p => followedCategoryIds.Contains(p.CategoryId))
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .Include(p => p.Media)
                    .Include(p => p.Likes)
                    .OrderByDescending(p => p.DateCreated)
                    .ToListAsync();
                    
                _logger.LogInformation("Retrieved {Count} feed posts", posts.Count);
                return posts;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, "GetUserFeedPostsAsync");
                throw;
            }
        }

        public async Task<List<Posts>> GetUserPostsAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Getting posts for user {UserId}", userId);
                
                using var context = _contextFactory.CreateDbContext();
                var posts = await context.Posts
                    .Where(p => p.UserId == userId)
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .Include(p => p.Media)
                    .Include(p => p.Likes)
                    .OrderByDescending(p => p.DateCreated)
                    .ToListAsync();
                    
                _logger.LogInformation("Retrieved {Count} posts for user {UserId}", posts.Count, userId);
                return posts;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"GetUserPostsAsync(userId: {userId})", userId);
                throw;
            }
        }

        public async Task CreatePostAsync(Posts post)
        {
            try
            {
                _logger.LogInformation("Creating post for user {UserId} in category {CategoryId}", 
                    post.UserId, post.CategoryId);
                
                using var context = _contextFactory.CreateDbContext();
                context.Posts.Add(post);
                await context.SaveChangesAsync();
                
                _logger.LogInformation("Post created successfully with ID {PostId}", post.Id);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, "CreatePostAsync", post.UserId);
                throw;
            }
        }

        // ========== LIKES ==========
        public async Task<bool> ToggleLikeAsync(int postId, string userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var existingLike = await context.PostLikes
                    .FirstOrDefaultAsync(l => l.PostID == postId && l.UserID == userId);

                if (existingLike != null)
                {
                    context.PostLikes.Remove(existingLike);
                    await context.SaveChangesAsync();
                    _logger.LogInformation("User {UserId} unliked post {PostId}", userId, postId);
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
                    _logger.LogInformation("User {UserId} liked post {PostId}", userId, postId);
                    return true; // Liked
                }
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"ToggleLikeAsync(postId: {postId})", userId);
                throw;
            }
        }

        public async Task<int> GetLikeCountAsync(int postId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.PostLikes.CountAsync(l => l.PostID == postId);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"GetLikeCountAsync(postId: {postId})");
                throw;
            }
        }

        // ========== COMMENTS ==========
        public async Task<List<Comments>> GetPostCommentsAsync(int postId)
        {
            try
            {
                _logger.LogInformation("Getting comments for post {PostId}", postId);
                
                using var context = _contextFactory.CreateDbContext();
                var comments = await context.Comments
                    .Where(c => c.PostID == postId)
                    .Include(c => c.User)
                    .OrderBy(c => c.DateCreated)
                    .ToListAsync();
                    
                _logger.LogInformation("Retrieved {Count} comments for post {PostId}", comments.Count, postId);
                return comments;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"GetPostCommentsAsync(postId: {postId})");
                throw;
            }
        }

        public async Task CreateCommentAsync(Comments comment)
        {
            try
            {
                _logger.LogInformation("Creating comment for post {PostId} by user {UserId}", 
                    comment.PostID, comment.UserID);
                
                using var context = _contextFactory.CreateDbContext();
                context.Comments.Add(comment);
                await context.SaveChangesAsync();
                
                _logger.LogInformation("Comment created successfully with ID {CommentId}", comment.Id);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, "CreateCommentAsync", comment.UserID);
                throw;
            }
        }

        // ========== REPORTS ==========
        public async Task<bool> HasUserReportedAsync(int postId, string userId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Reports.AnyAsync(r =>
                    r.PostID == postId && r.ReporterID == userId);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"HasUserReportedAsync(postId: {postId})", userId);
                throw;
            }
        }

        public async Task CreateReportAsync(Reports report)
        {
            try
            {
                _logger.LogWarning("Report created for post {PostId} by user {UserId}. Reason: {Reason}", 
                    report.PostID, report.ReporterID, report.Reason);
                
                using var context = _contextFactory.CreateDbContext();
                context.Reports.Add(report);
                await context.SaveChangesAsync();
                
                _logger.LogInformation("Report created successfully with ID {ReportId}", report.ReportID);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, "CreateReportAsync", report.ReporterID);
                throw;
            }
        }

        public async Task<bool> IsReportDismissedAsync(int postId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Reports.AnyAsync(r =>
                    r.PostID == postId && r.Status == "Dismissed");
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"IsReportDismissedAsync(postId: {postId})");
                throw;
            }
        }
    }
}
