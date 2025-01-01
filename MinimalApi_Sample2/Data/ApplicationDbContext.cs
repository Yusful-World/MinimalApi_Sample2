using Microsoft.EntityFrameworkCore;
using MinimalApi_Sample2.Models;

namespace MinimalApi_Sample2.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) 
        {
            
        }

        public DbSet<User> Users { get; set; }


    }
}
