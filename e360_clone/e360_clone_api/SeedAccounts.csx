using e360_clone.BusinessObjects;
using e360_clone.BusinessObjects.Helpers;
using e360_clone.DataAccess;

// Manual seed script - Run with: dotnet script SeedAccounts.csx

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql("Host=aws-1-ap-northeast-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.tngcpurejirmztvzbmmj;Password=Prn232_pasword");

using var context = new AppDbContext(optionsBuilder.Options);

// Check if accounts already exist
if (context.Accounts.Any())
{
    Console.WriteLine($"✅ Database already has {context.Accounts.Count()} accounts");
    foreach (var acc in context.Accounts)
    {
        Console.WriteLine($"  - {acc.Username} ({acc.Email}) - Role: {acc.Role}");
    }
    return;
}

Console.WriteLine("🌱 Seeding accounts...");

var defaultPassword = "123456";
var passwordHash = PasswordHelper.HashPassword(defaultPassword);

var accounts = new List<Account>
{
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

context.Accounts.AddRange(accounts);
context.SaveChanges();

Console.WriteLine($"✅ Successfully seeded {accounts.Count} accounts!");
Console.WriteLine("\n📋 Account list:");
foreach (var acc in accounts)
{
    Console.WriteLine($"  ✓ {acc.Username} | {acc.Email} | {acc.Role} | Password: 123456");
}
