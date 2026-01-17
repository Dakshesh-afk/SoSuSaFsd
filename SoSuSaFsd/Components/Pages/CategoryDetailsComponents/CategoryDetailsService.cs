using SoSuSaFsd.Data;
using SoSuSaFsd.Domain;
using SoSuSaFsd.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace SoSuSaFsd.Components.Pages.CategoryDetailsComponents
{
    public interface ICategoryDetailsService
    {
        Task<Users?> LoadCurrentUserAsync(string? userId);
        Task LoadCategoryDataAsync(CategoryDetailsState state, int categoryId, string? userId);
        Task<bool> SubmitReportAsync(int postId, string userId, string reason, string? details);
        Task<bool> CreatePostAsync(Posts post);
        Task<bool> SubmitAccessRequestAsync(string userId, int categoryId, string reason);
    }

    public class CategoryDetailsService : ICategoryDetailsService
    {
        private readonly IDbContextFactory<SoSuSaFsdContext> _contextFactory;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPostService _postService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryDetailsService> _logger;

        public CategoryDetailsService(
            IDbContextFactory<SoSuSaFsdContext> contextFactory,
            IServiceScopeFactory scopeFactory,
            IPostService postService,
            ICategoryService categoryService,
            ILogger<CategoryDetailsService> logger)
        {
            _contextFactory = contextFactory;
            _scopeFactory = scopeFactory;
            _postService = postService;
            _categoryService = categoryService;
            _logger = logger;
        }

        public async Task<Users?> LoadCurrentUserAsync(string? userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId)) return null;

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

        public async Task LoadCategoryDataAsync(CategoryDetailsState state, int categoryId, string? userId)
        {
            try
            {
                _logger.LogInformation("Loading category data for category {CategoryId}", categoryId);

                // Load category and posts
                state.VerifiedCategories = await _categoryService.GetVerifiedCategoriesAsync(5);
                state.CurrentCategory = await _categoryService.GetCategoryByIdAsync(categoryId);
                
                if (state.CurrentCategory != null)
                {
                    state.Title = "#" + state.CurrentCategory.CategoryName;
                }

                state.CategoryPosts = await _postService.GetCategoryPostsAsync(categoryId);

                // Load user-specific data
                if (!string.IsNullOrEmpty(userId) && state.CurrentCategory != null)
                {
                    state.IsFollowing = await _categoryService.IsUserFollowingAsync(userId, state.CurrentCategory.Id);
                    state.HasApprovedAccess = await _categoryService.HasApprovedAccessAsync(userId, state.CurrentCategory.Id);
                    state.HasPendingRequest = await _categoryService.HasPendingAccessRequestAsync(userId, state.CurrentCategory.Id);
                    state.RecentCategories = await _categoryService.GetRecentCategoriesAsync(userId, 5);
                    state.FollowedCategories = await _categoryService.GetFollowedCategoriesAsync(userId);
                }

                _logger.LogInformation("Successfully loaded category data");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category data");
                throw;
            }
        }

        public async Task<bool> SubmitReportAsync(int postId, string userId, string reason, string? details)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var existingReport = await context.Reports
                    .FirstOrDefaultAsync(r => r.PostID == postId && r.ReporterID == userId);

                if (existingReport != null)
                {
                    _logger.LogWarning("User {UserId} already reported post {PostId}", userId, postId);
                    return false;
                }

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
                _logger.LogInformation("Report submitted for post {PostId}", postId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting report");
                throw;
            }
        }

        public async Task<bool> CreatePostAsync(Posts post)
        {
            try
            {
                await _postService.CreatePostAsync(post);
                _logger.LogInformation("Post created successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                throw;
            }
        }

        public async Task<bool> SubmitAccessRequestAsync(string userId, int categoryId, string reason)
        {
            try
            {
                var hasPending = await _categoryService.HasPendingAccessRequestAsync(userId, categoryId);
                if (hasPending)
                {
                    _logger.LogWarning("User {UserId} already has pending request for category {CategoryId}", userId, categoryId);
                    return false;
                }

                var request = new CategoryAccessRequests
                {
                    UserId = userId,
                    CategoryId = categoryId,
                    Reason = reason,
                    Status = "Pending",
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now
                };

                await _categoryService.CreateAccessRequestAsync(request);
                _logger.LogInformation("Access request submitted");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting access request");
                throw;
            }
        }
    }
}
