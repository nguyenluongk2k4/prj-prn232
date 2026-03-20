using e360_clone.BusinessObjects;
using e360_clone.BusinessObjects.Helpers;
using e360_clone.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.Seeders
{
    public class ManualSeeder
    {
        public static async Task SeedAsync()
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql("Host=103.72.56.152;Port=5433;Database=postgres;Username=postgres;Password=GYM2Esz09utEFCm34MXACE9EYEFZMBGB");

            using var context = new AppDbContext(optionsBuilder.Options);

            // Check if accounts already exist
            if (await context.Accounts.AnyAsync())
            {
                Console.WriteLine($"✅ Database already has {await context.Accounts.CountAsync()} accounts");
                return;
            }

            Console.WriteLine("🌱 Seeding accounts...");

            var defaultPassword = "123456";
            var passwordHash = PasswordHelper.HashPassword(defaultPassword);

            var accounts = new List<Account>
            {
                new Account { Username = "superadmin", Email = "superadmin@e360.com", PasswordHash = passwordHash, Role = "SuperAdmin", FullName = "Super Administrator", Status = "Active", CreatedAt = DateTime.UtcNow },
                new Account { Username = "admin", Email = "admin@e360.com", PasswordHash = passwordHash, Role = "Admin", FullName = "System Administrator", Status = "Active", CreatedAt = DateTime.UtcNow },
                new Account { Username = "student", Email = "student@e360.com", PasswordHash = passwordHash, Role = "Student", FullName = "Nguyen Van Student", Status = "Active", CreatedAt = DateTime.UtcNow },
                new Account { Username = "teacher", Email = "teacher@e360.com", PasswordHash = passwordHash, Role = "Teacher", FullName = "Tran Van Teacher", Status = "Active", CreatedAt = DateTime.UtcNow },
                new Account { Username = "parent", Email = "parent@e360.com", PasswordHash = passwordHash, Role = "Parent", FullName = "Le Van Parent", Status = "Active", CreatedAt = DateTime.UtcNow },
                new Account { Username = "librarian", Email = "librarian@e360.com", PasswordHash = passwordHash, Role = "Librarian", FullName = "Pham Van Librarian", Status = "Active", CreatedAt = DateTime.UtcNow }
            };

            await context.Accounts.AddRangeAsync(accounts);
            await context.SaveChangesAsync();

            Console.WriteLine($"✅ Successfully seeded {accounts.Count} accounts!");
            Console.WriteLine("\n📋 Account list:");
            foreach (var acc in accounts)
            {
                Console.WriteLine($"  ✓ {acc.Username} | {acc.Email} | {acc.Role} | Password: 123456");
            }
        }
    }
}
