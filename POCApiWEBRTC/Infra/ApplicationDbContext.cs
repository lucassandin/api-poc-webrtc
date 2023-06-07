using Microsoft.EntityFrameworkCore;
using POCApiWEBRTC.Models;

namespace POCApiWEBRTC.Infra
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<SessionModel> Session { get; set; }
    }
}