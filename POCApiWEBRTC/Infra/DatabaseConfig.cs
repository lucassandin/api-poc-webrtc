using Microsoft.EntityFrameworkCore;

namespace POCApiWEBRTC.Infra
{
    public static class DatabaseConfig
    {
        public static void AddDatabaseConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(GetApiConnectionString(configuration))
           .EnableServiceProviderCaching());

            services.BuildServiceProvider().GetService<ApplicationDbContext>()?.Database.Migrate();
        }

        public static string GetApiConnectionString(IConfiguration configuration)
            => configuration.GetConnectionString("SqlConnection");
    }
}