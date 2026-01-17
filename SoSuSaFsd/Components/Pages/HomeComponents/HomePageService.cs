using SoSuSaFsd.Data;
using SoSuSaFsd.Domain;
using SoSuSaFsd.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace SoSuSaFsd.Components.Pages.HomeComponents
{
    /// <summary>
    /// Service interface for Home page business logic
    /// </summary>
    public interface IHomePageService
    {
        Task<Users?> LoadCurrentUserAsync(string? userId = null);
        Task LoadUserDataAsync(HomePageState state, string userId);
        Task<List<Posts>> SearchPostsAsync(string searchTerm);
        Task<List<Users>> SearchUsersAsync(string searchTerm);
        Task<bool> SubmitReportAsync(int postId, string userId, string reason, string? details = null);
        Task<bool> UpdateUserProfileAsync(Users user, string displayName, string bio, string? profileImage);
        Task RefreshPostLikesAsync(List<Posts> posts, int postId);
    }

    /// <summary>
    /// Handles business logic for the Home page
    /// Reduces Home.razor complexity by extracting data operations
    /// </summary>
    public class HomePageService : IHomePageService
    {
        private readonly IDbContextFactory<SoSuSaFsdContext> _contextFactory;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPostService _postService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<HomePageService> _logger;

        public HomePageService(
            IDbContextFactory<SoSuSaFsdContext> contextFactory,
            IServiceScopeFactory scopeFactory,
            IPostService postService,
            ICategoryService categoryService,
            ILogger<HomePageService> logger)
        {
            _contextFactory = contextFactory;
            _scopeFactory = scopeFactory;
            _postService = postService;
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Loads the current authenticated user
        /// </summary>
        public async Task<Users?> LoadCurrentUserAsync(string? userId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return null;
                }

                using var scope = _scopeFactory.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();
                return await userManager.FindByIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading current user");
                return null;
            }
        }

        /// <summary>
        /// Loads all data required for the home page
        /// </summary>
        public async Task LoadUserDataAsync(HomePageState state, string userId)
        {
            try
            {
                _logger.LogInformation("Loading home page data for user {UserId}", userId);

                // Load categories
                state.VerifiedCategories = await _categoryService.GetVerifiedCategoriesAsync(5);
                state.VerifiedCategoriesForRequest = await _categoryService.GetVerifiedCategoriesAsync(100);
                state.RecentCategories = await _categoryService.GetRecentCategoriesAsync(userId, 5);
                state.FollowedCategories = await _categoryService.GetFollowedCategoriesAsync(userId);

                // Load feed posts
                var followedCategoryIds = state.FollowedCategories.Select(c => c.Id).ToList();
                if (followedCategoryIds.Any())
                {
                    state.FeedPosts = await _postService.GetUserFeedPostsAsync(followedCategoryIds);
                }
                else
                {
                    state.FeedPosts = new List<Posts>();
                }

                // Load user posts
                state.UserPosts = await _postService.GetUserPostsAsync(userId);

                // Load access requests
                using var context = _contextFactory.CreateDbContext();
                state.UserAccessRequests = await context.CategoryAccessRequests
                    .Where(r => r.UserId == userId)
                    .Include(r => r.Category)
                    .OrderByDescending(r => r.DateCreated)
                    .ToListAsync();

                _logger.LogInformation("Successfully loaded home page data for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user data for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Searches for posts matching the search term
        /// </summary>
        public async Task<List<Posts>> SearchPostsAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new List<Posts>();
                }

                _logger.LogInformation("Searching posts with term: {SearchTerm}", searchTerm);

                using var context = _contextFactory.CreateDbContext();
                var term = searchTerm.ToLower();

                var results = await context.Posts
                    .Where(p => p.Content.ToLower().Contains(term) ||
                               p.User.UserName.ToLower().Contains(term))
                    .Include(p => p.User)
                    .Include(p => p.Category)
                    .Include(p => p.Media)
                    .Include(p => p.Likes)
                    .OrderByDescending(p => p.DateCreated)
                    .Take(10)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} posts matching '{SearchTerm}'", results.Count, searchTerm);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching posts with term: {SearchTerm}", searchTerm);
                return new List<Posts>();
            }
        }

        /// <summary>
        /// Searches for users matching the search term
        /// </summary>
        public async Task<List<Users>> SearchUsersAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new List<Users>();
                }

                _logger.LogInformation("Searching users with term: {SearchTerm}", searchTerm);

                using var context = _contextFactory.CreateDbContext();
                var term = searchTerm.ToLower();

                var results = await context.Users
                    .Where(u => u.UserName.ToLower().Contains(term) ||
                               u.DisplayName.ToLower().Contains(term))
                    .OrderBy(u => u.DisplayName ?? u.UserName)
                    .Take(10)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} users matching '{SearchTerm}'", results.Count, searchTerm);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users with term: {SearchTerm}", searchTerm);
                return new List<Users>();
            }
        }

        /// <summary>
        /// Submits a report for a post
        /// </summary>
        public async Task<bool> SubmitReportAsync(int postId, string userId, string reason, string? details = null)
        {
            try
            {
                _logger.LogInformation("Submitting report for post {PostId} by user {UserId}", postId, userId);

                using var context = _contextFactory.CreateDbContext();

                // Check for existing report
                var existingReport = await context.Reports
                    .FirstOrDefaultAsync(r => r.PostID == postId && r.ReporterID == userId);

                if (existingReport != null)
                {
                    _logger.LogWarning("User {UserId} already reported post {PostId}", userId, postId);
                    return false;
                }

                // Create report
                string finalReason = reason;
                if (!string.IsNullOrWhiteSpace(details))
                {
                    finalReason = $"{reason}: {details}";
                }

                var report = new Reports
                {
                    PostID = postId,
                    ReporterID = userId,
                    Reason = finalReason,
                    Status = "Pending",
                    DateCreated = DateTime.Now
                };

                await _postService.CreateReportAsync(report);

                _logger.LogInformation("Report submitted successfully for post {PostId}", postId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting report for post {PostId}", postId);
                throw;
            }
        }

        /// <summary>
        /// Updates user profile information
        /// </summary>
        public async Task<bool> UpdateUserProfileAsync(Users user, string displayName, string bio, string? profileImage)
        {
            try
            {
                _logger.LogInformation("Updating profile for user {UserId}", user.Id);

                using var scope = _scopeFactory.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();

                var userToUpdate = await userManager.FindByIdAsync(user.Id);
                if (userToUpdate == null)
                {
                    _logger.LogWarning("User {UserId} not found for profile update", user.Id);
                    return false;
                }

                userToUpdate.DisplayName = displayName;
                userToUpdate.Bio = bio;
                if (!string.IsNullOrEmpty(profileImage))
                {
                    userToUpdate.ProfileImage = profileImage;
                }

                var result = await userManager.UpdateAsync(userToUpdate);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Profile updated successfully for user {UserId}", user.Id);
                    return true;
                }

                _logger.LogWarning("Profile update failed for user {UserId}: {Errors}",
                    user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", user.Id);
                throw;
            }
        }

        /// <summary>
        /// Refreshes like data for a specific post
        /// </summary>
        public async Task RefreshPostLikesAsync(List<Posts> posts, int postId)
        {
            try
            {
                var post = posts.FirstOrDefault(p => p.Id == postId);
                if (post != null)
                {
                    using var context = _contextFactory.CreateDbContext();
                    post.Likes = await context.PostLikes
                        .Where(l => l.PostID == postId)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing likes for post {PostId}", postId);
            }
        }
    }
}
