using CoCo.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoCo.Data
{
    public class CocoDbContext : IdentityDbContext<ApplicationUser>
    {
        public CocoDbContext(DbContextOptions<CocoDbContext> options)
            : base(options)
        {
        }
        public DbSet<ApplicationUser> user { get; set; }
        public DbSet<CommentModel>  comment { get; set; }
        public DbSet<NewsModel> news { get; set; }
        
    }
}
