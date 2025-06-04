using Microsoft.EntityFrameworkCore;
using SkillBridge.API.Models;

namespace SkillBridge.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<User> Users { get; set; }
    }
}