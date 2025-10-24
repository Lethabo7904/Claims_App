using ClaimsApp.Models;
using Microsoft.EntityFrameworkCore;
using ClaimsApp;

namespace ClaimsApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<ClaimRecord> Claims { get; set; } = null!;
    }
}
