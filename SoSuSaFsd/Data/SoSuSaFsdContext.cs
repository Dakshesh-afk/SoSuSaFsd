using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SoSuSaFsd.Domain;

namespace SoSuSaFsd.Data
{
    public class SoSuSaFsdContext : DbContext
    {
        public SoSuSaFsdContext (DbContextOptions<SoSuSaFsdContext> options)
            : base(options)
        {
        }

        public DbSet<SoSuSaFsd.Domain.Users> Users { get; set; } = default!;
        public DbSet<SoSuSaFsd.Domain.Posts> Posts { get; set; } = default!;
        public DbSet<SoSuSaFsd.Domain.Comments> Comments { get; set; } = default!;
    }
}
