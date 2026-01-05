using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SoSuSaFsd.Domain;

namespace SoSuSaFsd.Data
{
    public class SoSuSaFsdContext : IdentityDbContext<Users>
    {
        public SoSuSaFsdContext(DbContextOptions<SoSuSaFsdContext> options)
            : base(options)
        {
        }

        public DbSet<SoSuSaFsd.Domain.Users> Users { get; set; } = default!;
        public DbSet<SoSuSaFsd.Domain.Posts> Posts { get; set; } = default!;
        public DbSet<SoSuSaFsd.Domain.Comments> Comments { get; set; } = default!;
        public DbSet<SoSuSaFsd.Domain.Categories> Categories { get; set; } = default!;
        public DbSet<SoSuSaFsd.Domain.CategoryFollows> CategoryFollows { get; set; } = default!;
        public DbSet<SoSuSaFsd.Domain.PostMedia> PostMedia { get; set; } = default!;
        public DbSet<SoSuSaFsd.Domain.CategoryAccessRequests> CategoryAccessRequests { get; set; } = default!;
    }
}