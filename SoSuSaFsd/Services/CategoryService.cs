using SoSuSaFsd.Data;
using SoSuSaFsd.Domain;
using Microsoft.EntityFrameworkCore;

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

        public CategoryService(IDbContextFactory<SoSuSaFsdContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // ========== CATEGORY RETRIEVAL ==========
        public async Task<Categories?> GetCategoryByIdAsync(int categoryId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
        }

        public async Task<Categories?> GetCategoryByNameAsync(string categoryName)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Categories
                .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == categoryName.ToLower());
        }

        public async Task<List<Categories>> GetFollowedCategoriesAsync(string userId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.CategoryFollows
                .Where(cf => cf.UserId == userId)
                .Include(cf => cf.Category)
                .Select(cf => cf.Category!)
                .ToListAsync();
        }

        public async Task<List<Categories>> GetVerifiedCategoriesAsync(int take = 5)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Categories
                .Where(c => c.IsVerified == true)
                .OrderByDescending(c => c.DateCreated)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<Categories>> GetRecentCategoriesAsync(string userId, int take = 5)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Categories
                .Where(c => c.CreatedBy == userId)
                .OrderByDescending(c => c.DateCreated)
                .Take(take)
                .ToListAsync();
        }

        // ========== FOLLOW CHECKS ==========
        public async Task<bool> IsUserFollowingAsync(string userId, int categoryId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.CategoryFollows.AnyAsync(cf =>
                cf.UserId == userId && cf.CategoryId == categoryId);
        }

        public async Task ToggleFollowAsync(string userId, int categoryId)
        {
            using var context = _contextFactory.CreateDbContext();
            var existing = await context.CategoryFollows
                .FirstOrDefaultAsync(cf => cf.UserId == userId && cf.CategoryId == categoryId);

            if (existing != null)
            {
                context.CategoryFollows.Remove(existing);
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
            }

            await context.SaveChangesAsync();
        }

        // ========== ACCESS REQUESTS ==========
        public async Task<bool> HasPendingAccessRequestAsync(string userId, int categoryId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.CategoryAccessRequests.AnyAsync(r =>
                r.UserId == userId && r.CategoryId == categoryId && r.Status == "Pending");
        }

        public async Task<bool> HasApprovedAccessAsync(string userId, int categoryId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.CategoryAccessRequests.AnyAsync(r =>
                r.UserId == userId && r.CategoryId == categoryId && r.Status == "Approved");
        }

        public async Task CreateAccessRequestAsync(CategoryAccessRequests request)
        {
            using var context = _contextFactory.CreateDbContext();
            context.CategoryAccessRequests.Add(request);
            await context.SaveChangesAsync();
        }
    }
}
