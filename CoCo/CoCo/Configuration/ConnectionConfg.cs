using CoCo.Data;
using CoCo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CoCo.Configuration
{
    public class ConnectionConfg
    {

        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DatabaseConnection");
            services.AddDbContext<CocoDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            );
            services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<CocoDbContext>().AddDefaultTokenProviders();
        }

    }
}
