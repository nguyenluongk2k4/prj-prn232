using System.Linq;
using e360_clone.BusinessObjects;
using e360_clone.BusinessObjects.Helpers;
using e360_clone.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.Seeders
{
    public static class AccountSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Check if accounts already exist
            if (await context.Accounts.AnyAsync())
            {
                return; // Already seeded
            }

            var defaultPassword = "123456";
            var passwordHash = PasswordHelper.HashPassword(defaultPassword);

            var accounts = new List<Account>
            {
                // Super Admin
                new Account
                {
                    Username = "superadmin",
                    Email = "superadmin@e360.com",
                    PasswordHash = passwordHash,
                    Role = "SuperAdmin",
                    FullName = "Super Administrator",
                    Status = "Active",
                    CreatedAt = DateTime.Now
                },
                // Admin
                new Account
                {
                    Username = "admin",
                    Email = "admin@e360.com",
                    PasswordHash = passwordHash,
                    Role = "Admin",
                    FullName = "System Administrator",
                    Status = "Active",
                    CreatedAt = DateTime.Now
                },
                // Student
                new Account
                {
                    Username = "student",
                    Email = "student@e360.com",
                    PasswordHash = passwordHash,
                    Role = "Student",
                    FullName = "Nguyen Van Student",
                    Status = "Active",
                    CreatedAt = DateTime.Now
                },
                // Teacher
                new Account
                {
                    Username = "teacher",
                    Email = "teacher@e360.com",
                    PasswordHash = passwordHash,
                    Role = "Teacher",
                    FullName = "Tran Van Teacher",
                    Status = "Active",
                    CreatedAt = DateTime.Now
                },
                // Parent
                new Account
                {
                    Username = "parent",
                    Email = "parent@e360.com",
                    PasswordHash = passwordHash,
                    Role = "Parent",
                    FullName = "Le Van Parent",
                    Status = "Active",
                    CreatedAt = DateTime.Now
                },
                // Librarian
                new Account
                {
                    Username = "librarian",
                    Email = "librarian@e360.com",
                    PasswordHash = passwordHash,
                    Role = "Librarian",
                    FullName = "Pham Van Librarian",
                    Status = "Active",
                    CreatedAt = DateTime.Now
                }
            };

            await context.Accounts.AddRangeAsync(accounts);
            await context.SaveChangesAsync();
        }
    }
}
