using e360_clone.Seeders;

Console.WriteLine("🌱 Starting database seed...\n");

try
{
    await ManualSeeder.SeedAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner: {ex.InnerException.Message}");
    }
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
