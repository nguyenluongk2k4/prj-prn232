using Bogus;
using e360_clone.BusinessObjects;
using e360_clone.BusinessObjects.Helpers;
using e360_clone.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace e360_clone.Seeders
{
    public class ManualSeeder
    {
        public static async Task SeedAsync()
        {
            var connectionString = ResolveConnectionString();
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString);

            using var context = new AppDbContext(optionsBuilder.Options);

            await SeedAccountsAsync(context);
            await SeedSubjectsAsync(context);
            await SeedStudentsAsync(context);
            await SeedStudentAccountsAsync(context);
            await SeedStudentSubjectsAsync(context);
        }

        private static async Task SeedAccountsAsync(AppDbContext context)
        {
            if (await context.Accounts.AnyAsync())
            {
                Console.WriteLine($"Database already has {await context.Accounts.CountAsync()} accounts.");
                return;
            }

            Console.WriteLine("Seeding accounts...");

            var defaultPassword = "123456";
            var passwordHash = PasswordHelper.HashPassword(defaultPassword);

            var accounts = new List<Account>
            {
                new Account { Username = "superadmin", Email = "superadmin@e360.com", PasswordHash = passwordHash, Role = "SuperAdmin", FullName = "Super Administrator", Status = "Active", CreatedAt = DateTime.UtcNow },
                new Account { Username = "admin", Email = "admin@e360.com", PasswordHash = passwordHash, Role = "Admin", FullName = "System Administrator", Status = "Active", CreatedAt = DateTime.UtcNow },
                new Account { Username = "academicstaff", Email = "academicstaff@e360.com", PasswordHash = passwordHash, Role = "AcademicStaff", FullName = "Nguyen Van Academic", Status = "Active", CreatedAt = DateTime.UtcNow },
                new Account { Username = "teacher", Email = "teacher@e360.com", PasswordHash = passwordHash, Role = "Teacher", FullName = "Tran Van Teacher", Status = "Active", CreatedAt = DateTime.UtcNow },
                new Account { Username = "student", Email = "student@e360.com", PasswordHash = passwordHash, Role = "Student", FullName = "Nguyen Van Student", Status = "Active", CreatedAt = DateTime.UtcNow }
            };

            await context.Accounts.AddRangeAsync(accounts);
            await context.SaveChangesAsync();

            Console.WriteLine($"Seeded {accounts.Count} accounts. Default password: 123456");
        }

        private static async Task SeedStudentsAsync(AppDbContext context)
        {
            var classes = await context.Classes.AsNoTracking().ToListAsync();
            if (classes.Count == 0)
            {
                Console.WriteLine("No classes found. Skip student seeding.");
                return;
            }

            var faker = new Faker("vi");
            var genderValues = new[] { "Nam", "Nữ" };
            var allCodes = new HashSet<string>(
                await context.Students.Select(s => s.StudentCode).ToListAsync());

            foreach (var cls in classes)
            {
                var targetCount = cls.StudentCount > 0 ? cls.StudentCount : 25;
                var existingCount = await context.Students.CountAsync(s => s.ClassId == cls.Id);
                if (existingCount >= targetCount)
                {
                    continue;
                }

                var existingCodes = new HashSet<string>(
                    await context.Students.Where(s => s.ClassId == cls.Id)
                        .Select(s => s.StudentCode)
                        .ToListAsync());

                var toAdd = targetCount - existingCount;
                var students = new List<Student>(toAdd);

                var index = existingCount + 1;
                for (var i = 0; i < toAdd; i++)
                {
                    var code = BuildStudentCode(cls, index, existingCodes, allCodes);
                    existingCodes.Add(code);
                    allCodes.Add(code);

                    var gender = faker.PickRandom(genderValues);
                    var fullName = faker.Name.FullName();
                    var email = $"{code.ToLowerInvariant()}@fpt.edu.vn";
                    var phone = faker.Random.ReplaceNumbers("0#########");

                    var dob = faker.Date.Between(DateTime.Today.AddYears(-24), DateTime.Today.AddYears(-18)).Date;
                    var student = new Student
                    {
                        StudentCode = code,
                        FullName = fullName,
                        Gender = gender,
                        DateOfBirth = DateTime.SpecifyKind(dob, DateTimeKind.Utc),
                        Email = email,
                        PhoneNumber = phone,
                        Address = faker.Address.FullAddress(),
                        ClassId = cls.Id,
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow
                    };

                    students.Add(student);
                    index++;
                }

                if (students.Count > 0)
                {
                    await context.Students.AddRangeAsync(students);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"Seeded {students.Count} students for class {cls.ClassCode}.");
                }
            }
        }

        private static async Task SeedStudentAccountsAsync(AppDbContext context)
        {
            var accounts = await context.Accounts.ToListAsync();
            var students = await context.Students.AsNoTracking().ToListAsync();
            var passwordHash = PasswordHelper.HashPassword("123456");

            var byStudentId = accounts.Where(a => a.StudentId.HasValue)
                .ToDictionary(a => a.StudentId!.Value, a => a);
            var byUsername = accounts.ToDictionary(a => a.Username, a => a);
            var byEmail = accounts.ToDictionary(a => a.Email, a => a);

            var toAdd = new List<Account>();
            var updated = 0;
            var skipped = 0;
            foreach (var student in students)
            {
                var username = student.StudentCode;
                var email = student.Email;

                if (byStudentId.TryGetValue(student.Id, out var existing))
                {
                    existing.Username = username;
                    existing.Email = email;
                    existing.Role = "Student";
                    existing.FullName = student.FullName;
                    existing.PhoneNumber = student.PhoneNumber;
                    existing.Status = "Active";
                    existing.UpdatedAt = DateTime.UtcNow;
                    updated++;
                    continue;
                }

                if (byUsername.ContainsKey(username) || byEmail.ContainsKey(email))
                {
                    // Avoid hijacking existing non-student accounts
                    skipped++;
                    continue;
                }

                var account = new Account
                {
                    Username = username,
                    Email = email,
                    PasswordHash = passwordHash,
                    Role = "Student",
                    FullName = student.FullName,
                    PhoneNumber = student.PhoneNumber,
                    Status = "Active",
                    StudentId = student.Id,
                    CreatedAt = DateTime.UtcNow
                };

                toAdd.Add(account);
                byUsername[username] = account;
                byEmail[email] = account;
                byStudentId[student.Id] = account;
            }

            if (toAdd.Count > 0)
            {
                await context.Accounts.AddRangeAsync(toAdd);
                await context.SaveChangesAsync();
                Console.WriteLine($"Seeded {toAdd.Count} student accounts.");
                if (updated > 0)
                {
                    Console.WriteLine($"Updated {updated} existing student accounts.");
                }
                if (skipped > 0)
                {
                    Console.WriteLine($"Skipped {skipped} students due to username/email conflicts.");
                }
            }
            else
            {
                Console.WriteLine("No new student accounts to seed.");
                if (updated > 0)
                {
                    Console.WriteLine($"Updated {updated} existing student accounts.");
                }
                if (skipped > 0)
                {
                    Console.WriteLine($"Skipped {skipped} students due to username/email conflicts.");
                }
            }
        }

        private static async Task SeedSubjectsAsync(AppDbContext context)
        {
            var seeds = BuildSubjectSeeds();
            var existing = await context.Subjects.ToDictionaryAsync(s => s.SubjectCode);

            var toAdd = new List<Subject>();
            foreach (var seed in seeds)
            {
                if (existing.TryGetValue(seed.SubjectCode, out var subject))
                {
                    subject.SubjectName = seed.SubjectName;
                    subject.Department = seed.Department;
                    subject.SubjectType = seed.SubjectType;
                    subject.Credits = seed.Credits;
                    subject.TheoryHours = seed.TheoryHours;
                    subject.PracticeHours = seed.PracticeHours;
                    subject.Status = seed.Status;
                    subject.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    toAdd.Add(new Subject
                    {
                        SubjectCode = seed.SubjectCode,
                        SubjectName = seed.SubjectName,
                        Department = seed.Department,
                        SubjectType = seed.SubjectType,
                        Credits = seed.Credits,
                        TheoryHours = seed.TheoryHours,
                        PracticeHours = seed.PracticeHours,
                        Status = seed.Status,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (toAdd.Count > 0)
            {
                await context.Subjects.AddRangeAsync(toAdd);
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"Seeded/updated {seeds.Count} subjects.");
        }

        private static async Task SeedStudentSubjectsAsync(AppDbContext context)
        {
            var subjects = await context.Subjects.AsNoTracking().ToListAsync();
            if (subjects.Count == 0)
            {
                Console.WriteLine("No subjects found. Skip student-subject seeding.");
                return;
            }

            var subjectMap = subjects.ToDictionary(s => s.SubjectCode, s => s.Id);
            var majorCodeMap = await context.Majors.AsNoTracking()
                .ToDictionaryAsync(m => m.Id, m => m.MajorCode);

            var classes = await context.Classes.AsNoTracking().ToListAsync();
            var students = await context.Students.AsNoTracking().ToListAsync();

            var existing = await context.StudentSubjects
                .Select(s => new { s.StudentId, s.SubjectId, s.Semester })
                .ToListAsync();
            var existingSet = new HashSet<(int StudentId, int SubjectId, int Semester)>(
                existing.Select(x => (x.StudentId, x.SubjectId, x.Semester)));

            var semesterDefault = 1;
            var academicYearDefault = $"{DateTime.UtcNow.Year}-{DateTime.UtcNow.Year + 1}";

            var toAdd = new List<StudentSubject>();
            foreach (var cls in classes)
            {
                if (!majorCodeMap.TryGetValue(cls.MajorId, out var majorCode))
                    continue;

                var subjectCodes = ResolveSubjectCodesForMajor(majorCode);
                if (subjectCodes.Count == 0)
                    continue;

                var classStudents = students.Where(s => s.ClassId == cls.Id).ToList();
                if (classStudents.Count == 0)
                    continue;

                var semester = cls.Semester > 0 ? cls.Semester : semesterDefault;
                var academicYear = string.IsNullOrWhiteSpace(cls.AcademicYear) ? academicYearDefault : cls.AcademicYear;

                foreach (var student in classStudents)
                {
                    foreach (var code in subjectCodes)
                    {
                        if (!subjectMap.TryGetValue(code, out var subjectId))
                            continue;

                        var key = (student.Id, subjectId, semester);
                        if (existingSet.Contains(key))
                            continue;

                        toAdd.Add(new StudentSubject
                        {
                            StudentId = student.Id,
                            SubjectId = subjectId,
                            ClassId = cls.Id,
                            AcademicYear = academicYear,
                            Semester = semester,
                            Status = "Enrolled",
                            TotalSessions = 0,
                            PresentSessions = 0,
                            AbsentSessions = 0,
                            CreatedAt = DateTime.UtcNow
                        });

                        existingSet.Add(key);
                    }
                }
            }

            if (toAdd.Count > 0)
            {
                await context.StudentSubjects.AddRangeAsync(toAdd);
                await context.SaveChangesAsync();
                Console.WriteLine($"Seeded {toAdd.Count} student-subject enrollments.");
            }
            else
            {
                Console.WriteLine("No new student-subject enrollments to seed.");
            }
        }

        private static List<string> ResolveSubjectCodesForMajor(string majorCode)
        {
            var codes = new List<string>();

            switch (majorCode)
            {
                case "SE":
                    codes.AddRange(new[]
                    {
                        "PRF192", "PRO192", "DSA201", "DBI202", "OSG202", "NWC203", "WED201", "SWT301", "SWP391", "OJT"
                    });
                    break;
                case "AI":
                    codes.AddRange(new[]
                    {
                        "MLN111", "DLP301", "NLP301", "CV301"
                    });
                    break;
                case "IA":
                    codes.AddRange(new[]
                    {
                        "IAS201", "NSC203", "CRY303", "ETH202"
                    });
                    break;
                case "BUS":
                    codes.AddRange(new[]
                    {
                        "BUS101", "MKT201", "MKT301", "FIN202"
                    });
                    break;
                case "GD":
                case "MM":
                    codes.AddRange(new[]
                    {
                        "GD101", "MM201", "UXD301"
                    });
                    break;
                case "EN":
                case "JPN":
                case "KOR":
                case "CHI":
                    codes.AddRange(new[]
                    {
                        "ENG101", "ENG206", "JPN101", "KOR101"
                    });
                    break;
            }

            codes.AddRange(new[] { "SKI101", "CSI104", "ENT101" });
            return codes;
        }

        private static List<SubjectSeed> BuildSubjectSeeds()
        {
            return new List<SubjectSeed>
            {
                new SubjectSeed("PRF192", "Programming Fundamentals", "Software Engineering"),
                new SubjectSeed("PRO192", "Object-Oriented Programming", "Software Engineering"),
                new SubjectSeed("DSA201", "Data Structures & Algorithms", "Software Engineering"),
                new SubjectSeed("DBI202", "Database Systems", "Software Engineering"),
                new SubjectSeed("OSG202", "Operating Systems", "Software Engineering"),
                new SubjectSeed("NWC203", "Computer Networking", "Software Engineering"),
                new SubjectSeed("WED201", "Web Development", "Software Engineering"),
                new SubjectSeed("SWT301", "Software Testing", "Software Engineering"),
                new SubjectSeed("SWP391", "Software Project", "Software Engineering"),
                new SubjectSeed("OJT", "On-the-Job Training", "Software Engineering"),

                new SubjectSeed("MLN111", "Machine Learning", "Artificial Intelligence"),
                new SubjectSeed("DLP301", "Deep Learning", "Artificial Intelligence"),
                new SubjectSeed("NLP301", "Natural Language Processing", "Artificial Intelligence"),
                new SubjectSeed("CV301", "Computer Vision", "Artificial Intelligence"),

                new SubjectSeed("IAS201", "Information Security", "Information Assurance"),
                new SubjectSeed("NSC203", "Network Security", "Information Assurance"),
                new SubjectSeed("CRY303", "Cryptography", "Information Assurance"),
                new SubjectSeed("ETH202", "Ethical Hacking", "Information Assurance"),

                new SubjectSeed("BUS101", "Business Fundamentals", "Business"),
                new SubjectSeed("MKT201", "Marketing Principles", "Business"),
                new SubjectSeed("MKT301", "Digital Marketing", "Business"),
                new SubjectSeed("FIN202", "Finance", "Business"),

                new SubjectSeed("GD101", "Graphic Design", "Design"),
                new SubjectSeed("MM201", "Multimedia Communication", "Design"),
                new SubjectSeed("UXD301", "UI/UX Design", "Design"),

                new SubjectSeed("ENG101", "English Level 1", "Language"),
                new SubjectSeed("ENG206", "English Level 6", "Language"),
                new SubjectSeed("JPN101", "Japanese Level 1", "Language"),
                new SubjectSeed("KOR101", "Korean Level 1", "Language"),

                new SubjectSeed("SKI101", "Soft Skills", "General Skills"),
                new SubjectSeed("CSI104", "Critical Thinking", "General Skills"),
                new SubjectSeed("ENT101", "Entrepreneurship", "General Skills")
            };
        }

        private sealed record SubjectSeed(
            string SubjectCode,
            string SubjectName,
            string Department,
            string SubjectType = "Core",
            int Credits = 3,
            int TheoryHours = 30,
            int PracticeHours = 15,
            string Status = "Active");

        private static string BuildStudentCode(
            Class cls,
            int index,
            HashSet<string> existingForClass,
            HashSet<string> existingAll)
        {
            var attempt = index;
            string code;
            do
            {
                var cohort = ResolveCohort(cls);
                code = $"HE{cohort:00}{attempt:0000}";
                attempt++;
            }
            while (existingForClass.Contains(code) || existingAll.Contains(code));

            return code;
        }

        private static int ResolveCohort(Class cls)
        {
            if (cls.Cohort > 0)
                return cls.Cohort;

            if (string.IsNullOrWhiteSpace(cls.ClassCode))
                return 0;

            var match = System.Text.RegularExpressions.Regex.Match(cls.ClassCode, @"\d{2}");
            if (!match.Success)
                return 0;

            return int.TryParse(match.Value, out var cohort) ? cohort : 0;
        }

        private static string ResolveConnectionString()
        {
            var baseDir = AppContext.BaseDirectory;
            var dataAccessPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "e360_clone.DataAccess"));

            var builder = new ConfigurationBuilder()
                .SetBasePath(dataAccessPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                   ?? configuration.GetConnectionString("MyCnn");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string is missing. Please set DefaultConnection or MyCnn in e360_clone.DataAccess/appsettings.json");
            }

            return connectionString;
        }
    }
}
