using collectiontracker.Models;
using Microsoft.EntityFrameworkCore;

namespace collectiontracker.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
        }
        
        public DbSet<Figures> Figures { get; set; }
    }
}
