using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace e360_clone.DataAccess
{
    /// <summary>
    /// Design-time factory for EF Core migrations
    /// Required for dotnet ef migrations add command
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            
            // Use connection string from appsettings.json in API project
            optionsBuilder.UseNpgsql(
                "Host=aws-1-ap-northeast-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.tngcpurejirmztvzbmmj;Password=Prn232_pasword",
                b => b.MigrationsAssembly("e360_clone.DataAccess")
            );

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
