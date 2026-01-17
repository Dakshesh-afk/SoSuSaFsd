using SoSuSaFsd.Data;
using SoSuSaFsd.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SoSuSaFsd.Services
{
    public interface ICategoryService
    {
        Task<Categories?> GetCategoryByIdAsync(int categoryId);
        Task<Categories?> GetCategoryByNameAsync(string categoryName);
        Task<List<Categories>> GetFollowedCategoriesAsync(string userId);
        Task<List<Categories>> GetVerifiedCategoriesAsync(int take = 5);
        Task<List<Categories>> GetRecentCategoriesAsync(string userId, int take = 5);
        Task<bool> IsUserFollowingAsync(string userId, int categoryId);
        Task<bool> HasPendingAccessRequestAsync(string userId, int categoryId);
        Task<bool> HasApprovedAccessAsync(string userId, int categoryId);
        Task ToggleFollowAsync(string userId, int categoryId);
        Task CreateAccessRequestAsync(CategoryAccessRequests request);
    }

    public class CategoryService : ICategoryService
    {
        private readonly IDbContextFactory<SoSuSaFsdContext> _contextFactory;
        private readonly ILogger<CategoryService> _logger;
        private readonly IErrorHandlingService _errorHandler;

        public CategoryService(
            IDbContextFactory<SoSuSaFsdContext> contextFactory,
            ILogger<CategoryService> logger,
            IErrorHandlingService errorHandler)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            _errorHandler = errorHandler;
        }

        // ========== CATEGORY RETRIEVAL ==========
        public async Task<Categories?> GetCategoryByIdAsync(int categoryId)
        {
            try
            {
                _logger.LogInformation("Getting category {CategoryId}", categoryId);
                
                using var context = _contextFactory.CreateDbContext();
                var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
                
                if (category == null)
                {
                    _logger.LogWarning("Category {CategoryId} not found", categoryId);
                }
                
                return category;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"GetCategoryByIdAsync(categoryId: {categoryId})");
                throw;
            }
        }

        public async Task<Categories?> GetCategoryByNameAsync(string categoryName)
        {
            try
            {
                _logger.LogInformation("Getting category by name: {CategoryName}", categoryName);
                
                using var context = _contextFactory.CreateDbContext();
                var category = await context.Categories
                    .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == categoryName.ToLower());
                    
                if (category == null)
                {
                    _logger.LogWarning("Category '{CategoryName}' not found", categoryName);
                }
                
                return category;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"GetCategoryByNameAsync(categoryName: {categoryName})");
                throw;
            }
        }

        public async Task<List<Categories>> GetFollowedCategoriesAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Getting followed categories for user {UserId}", userId);
                
                using var context = _contextFactory.CreateDbContext();
                var categories = await context.CategoryFollows
                    .Where(cf => cf.UserId == userId)
                    .Include(cf => cf.Category)
                    .Select(cf => cf.Category!)
                    .ToListAsync();
                    
                _logger.LogInformation("User {UserId} follows {Count} categories", userId, categories.Count);
                return categories;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"GetFollowedCategoriesAsync(userId: {userId})", userId);
                throw;
            }
        }

        public async Task<List<Categories>> GetVerifiedCategoriesAsync(int take = 5)
        {
            try
            {
                _logger.LogInformation("Getting {Count} verified categories", take);
                
                using var context = _contextFactory.CreateDbContext();
                var categories = await context.Categories
                    .Where(c => c.IsVerified == true)
                    .OrderByDescending(c => c.DateCreated)
                    .Take(take)
                    .ToListAsync();
                    
                _logger.LogInformation("Retrieved {Count} verified categories", categories.Count);
                return categories;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"GetVerifiedCategoriesAsync(take: {take})");
                throw;
            }
        }

        public async Task<List<Categories>> GetRecentCategoriesAsync(string userId, int take = 5)
        {
            try
            {
                _logger.LogInformation("Getting {Count} recent categories for user {UserId}", take, userId);
                
                using var context = _contextFactory.CreateDbContext();
                var categories = await context.Categories
                    .Where(c => c.CreatedBy == userId)
                    .OrderByDescending(c => c.DateCreated)
                    .Take(take)
                    .ToListAsync();
                    
                _logger.LogInformation("User {UserId} created {Count} recent categories", userId, categories.Count);
                return categories;
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"GetRecentCategoriesAsync(userId: {userId})", userId);
                throw;
            }
        }

        // ========== FOLLOW CHECKS ==========
        public async Task<bool> IsUserFollowingAsync(string userId, int categoryId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.CategoryFollows.AnyAsync(cf =>
                    cf.UserId == userId && cf.CategoryId == categoryId);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"IsUserFollowingAsync(userId: {userId}, categoryId: {categoryId})", userId);
                throw;
            }
        }

        public async Task ToggleFollowAsync(string userId, int categoryId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var existing = await context.CategoryFollows
                    .FirstOrDefaultAsync(cf => cf.UserId == userId && cf.CategoryId == categoryId);

                if (existing != null)
                {
                    context.CategoryFollows.Remove(existing);
                    _logger.LogInformation("User {UserId} unfollowed category {CategoryId}", userId, categoryId);
                }
                else
                {
                    context.CategoryFollows.Add(new CategoryFollows
                    {
                        UserId = userId,
                        CategoryId = categoryId,
                        DateCreated = DateTime.Now,
                        DateUpdated = DateTime.Now
                    });
                    _logger.LogInformation("User {UserId} followed category {CategoryId}", userId, categoryId);
                }

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"ToggleFollowAsync(userId: {userId}, categoryId: {categoryId})", userId);
                throw;
            }
        }

        // ========== ACCESS REQUESTS ==========
        public async Task<bool> HasPendingAccessRequestAsync(string userId, int categoryId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.CategoryAccessRequests.AnyAsync(r =>
                    r.UserId == userId && r.CategoryId == categoryId && r.Status == "Pending");
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"HasPendingAccessRequestAsync(userId: {userId}, categoryId: {categoryId})", userId);
                throw;
            }
        }

        public async Task<bool> HasApprovedAccessAsync(string userId, int categoryId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.CategoryAccessRequests.AnyAsync(r =>
                    r.UserId == userId && r.CategoryId == categoryId && r.Status == "Approved");
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, $"HasApprovedAccessAsync(userId: {userId}, categoryId: {categoryId})", userId);
                throw;
            }
        }

        public async Task CreateAccessRequestAsync(CategoryAccessRequests request)
        {
            try
            {
                _logger.LogInformation("Creating access request for user {UserId} to category {CategoryId}. Reason: {Reason}", 
                    request.UserId, request.CategoryId, request.Reason);
                
                using var context = _contextFactory.CreateDbContext();
                context.CategoryAccessRequests.Add(request);
                await context.SaveChangesAsync();
                
                _logger.LogInformation("Access request created successfully with ID {RequestId}", request.Id);
            }
            catch (Exception ex)
            {
                _errorHandler.LogError(ex, "CreateAccessRequestAsync", request.UserId);
                throw;
            }
        }
    }
}
