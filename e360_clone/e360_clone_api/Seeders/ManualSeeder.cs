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
            await SeedLecturersAsync(context);
            await SeedLecturerAccountsAsync(context);
            await SeedStudentsAsync(context);
            await SeedStudentAccountsAsync(context);
            await SeedStudentSubjectsAsync(context);
            await SeedTeachingAssignmentsAsync(context);
            await SeedExamSchedulesAsync(context);
            await SeedStudentExamsAsync(context);
            await AllocateExamRoomsAsync(context);
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

        private static async Task SeedLecturersAsync(AppDbContext context)
        {
            if (await context.Lecturers.AnyAsync())
            {
                Console.WriteLine($"Database already has {await context.Lecturers.CountAsync()} lecturers.");
                return;
            }

            Console.WriteLine("Seeding lecturers...");
            var faker = new Faker("vi");
            var genderValues = new[] { "Nam", "Nữ" };

            var lecturers = new List<Lecturer>();
            for (var i = 1; i <= 12; i++)
            {
                var code = $"GV{i:0000}";
                var gender = faker.PickRandom(genderValues);
                var dob = faker.Date.Between(DateTime.Today.AddYears(-50), DateTime.Today.AddYears(-28)).Date;

                lecturers.Add(new Lecturer
                {
                    EmployeeCode = code,
                    FullName = faker.Name.FullName(),
                    Gender = gender,
                    DateOfBirth = DateTime.SpecifyKind(dob, DateTimeKind.Utc),
                    Email = $"{code.ToLowerInvariant()}@fpt.edu.vn",
                    PhoneNumber = faker.Random.ReplaceNumbers("0#########"),
                    Department = faker.PickRandom(new[] { "Software Engineering", "AI", "Business", "Design", "Language" }),
                    Position = faker.PickRandom(new[] { "Lecturer", "Senior Lecturer" }),
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.Lecturers.AddRangeAsync(lecturers);
            await context.SaveChangesAsync();
            Console.WriteLine($"Seeded {lecturers.Count} lecturers.");
        }

        private static async Task SeedLecturerAccountsAsync(AppDbContext context)
        {
            var lecturers = await context.Lecturers.AsNoTracking().ToListAsync();
            if (lecturers.Count == 0)
            {
                Console.WriteLine("No lecturers found. Skip lecturer account seeding.");
                return;
            }

            var accounts = await context.Accounts.ToListAsync();
            var passwordHash = PasswordHelper.HashPassword("123456");

            var byUsername = accounts.ToDictionary(a => a.Username, a => a);
            var byEmail = accounts.ToDictionary(a => a.Email, a => a);

            var teacherAccounts = accounts.Where(a => a.Role == "Teacher").ToList();
            var firstLecturer = lecturers.FirstOrDefault();
            if (firstLecturer != null)
            {
                var primaryTeacher = teacherAccounts.FirstOrDefault();
                if (primaryTeacher != null && primaryTeacher.LecturerId == null)
                {
                    primaryTeacher.LecturerId = firstLecturer.Id;
                    primaryTeacher.FullName = firstLecturer.FullName;
                    primaryTeacher.Email = firstLecturer.Email;
                    primaryTeacher.UpdatedAt = DateTime.UtcNow;
                }
            }

            var toAdd = new List<Account>();
            foreach (var lecturer in lecturers)
            {
                if (accounts.Any(a => a.LecturerId == lecturer.Id))
                    continue;

                var username = lecturer.EmployeeCode.ToLowerInvariant();
                if (byUsername.ContainsKey(username) || byEmail.ContainsKey(lecturer.Email))
                    continue;

                var account = new Account
                {
                    Username = username,
                    Email = lecturer.Email,
                    PasswordHash = passwordHash,
                    Role = "Teacher",
                    FullName = lecturer.FullName,
                    PhoneNumber = lecturer.PhoneNumber,
                    Status = "Active",
                    LecturerId = lecturer.Id,
                    CreatedAt = DateTime.UtcNow
                };
                toAdd.Add(account);
                byUsername[username] = account;
                byEmail[lecturer.Email] = account;
            }

            if (toAdd.Count > 0)
            {
                await context.Accounts.AddRangeAsync(toAdd);
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"Seeded {toAdd.Count} lecturer accounts.");
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

        private static async Task SeedTeachingAssignmentsAsync(AppDbContext context)
        {
            var lecturers = await context.Lecturers.AsNoTracking().ToListAsync();
            var classes = await context.Classes.AsNoTracking().ToListAsync();
            var subjects = await context.Subjects.AsNoTracking().ToListAsync();
            var majors = await context.Majors.AsNoTracking().ToDictionaryAsync(m => m.Id, m => m.MajorCode);

            if (lecturers.Count == 0 || classes.Count == 0 || subjects.Count == 0)
            {
                Console.WriteLine("Missing lecturers/classes/subjects. Skip teaching assignment seeding.");
                return;
            }

            var subjectByCode = subjects.ToDictionary(s => s.SubjectCode, s => s.Id);
            var existing = await context.TeachingAssignments
                .Select(t => new { t.LecturerId, t.SubjectId, t.ClassId, t.Semester })
                .ToListAsync();
            var existingSet = new HashSet<(int LecturerId, int SubjectId, int ClassId, string Semester)>(
                existing.Select(x => (x.LecturerId, x.SubjectId, x.ClassId, x.Semester)));

            var rand = new Random();
            var toAdd = new List<TeachingAssignment>();

            foreach (var lecturer in lecturers)
            {
                var assignedClasses = classes.OrderBy(_ => rand.Next()).Take(3).ToList();
                foreach (var cls in assignedClasses)
                {
                    if (!majors.TryGetValue(cls.MajorId, out var majorCode))
                        continue;

                    var subjectCodes = ResolveSubjectCodesForMajor(majorCode)
                        .Where(code => subjectByCode.ContainsKey(code))
                        .ToList();
                    if (subjectCodes.Count == 0)
                        continue;

                    var takeCount = Math.Min(subjectCodes.Count, 2);
                    var selected = subjectCodes.OrderBy(_ => rand.Next()).Take(takeCount);

                    foreach (var code in selected)
                    {
                        var subjectId = subjectByCode[code];
                        var semester = cls.Semester > 0 ? cls.Semester.ToString() : "1";

                        var key = (lecturer.Id, subjectId, cls.Id, semester);
                        if (existingSet.Contains(key))
                            continue;

                        toAdd.Add(new TeachingAssignment
                        {
                            LecturerId = lecturer.Id,
                            SubjectId = subjectId,
                            ClassId = cls.Id,
                            AcademicYear = string.IsNullOrWhiteSpace(cls.AcademicYear)
                                ? $"{DateTime.UtcNow.Year}-{DateTime.UtcNow.Year + 1}"
                                : cls.AcademicYear,
                            Semester = semester,
                            Status = "Active",
                            CreatedAt = DateTime.UtcNow
                        });
                        existingSet.Add(key);
                    }
                }
            }

            if (toAdd.Count > 0)
            {
                await context.TeachingAssignments.AddRangeAsync(toAdd);
                await context.SaveChangesAsync();
                Console.WriteLine($"Seeded {toAdd.Count} teaching assignments.");
            }
            else
            {
                Console.WriteLine("No new teaching assignments to seed.");
            }
        }

        private static async Task SeedStudentExamsAsync(AppDbContext context)
        {
            var exams = await context.Exams.AsNoTracking().ToListAsync();
            if (exams.Count == 0)
            {
                Console.WriteLine("No exams found. Skip student-exam seeding.");
                return;
            }

            var studentSubjects = await context.StudentSubjects.AsNoTracking().ToListAsync();
            var existing = await context.StudentExams
                .Select(se => new { se.ExamId, se.StudentId })
                .ToListAsync();
            var existingSet = new HashSet<(int ExamId, int StudentId)>(existing.Select(x => (x.ExamId, x.StudentId)));

            var toAdd = new List<StudentExam>();
            foreach (var exam in exams)
            {
                var semesterFilter = ParseSemester(exam.Semester);
                var candidates = studentSubjects.Where(ss =>
                        ss.SubjectId == exam.SubjectId &&
                        (ss.ClassId == null || ss.ClassId == exam.ClassId || ss.ClassId == 0))
                    .ToList();

                if (!string.IsNullOrWhiteSpace(exam.AcademicYear))
                {
                    candidates = candidates
                        .Where(ss => ss.AcademicYear == exam.AcademicYear)
                        .ToList();
                }

                if (semesterFilter.HasValue)
                {
                    candidates = candidates
                        .Where(ss => ss.Semester == semesterFilter.Value)
                        .ToList();
                }

                foreach (var ss in candidates)
                {
                    var key = (exam.Id, ss.StudentId);
                    if (existingSet.Contains(key))
                        continue;

                    toAdd.Add(new StudentExam
                    {
                        ExamId = exam.Id,
                        StudentId = ss.StudentId,
                        Status = "Registered",
                        CreatedAt = DateTime.UtcNow
                    });
                    existingSet.Add(key);
                }
            }

            if (toAdd.Count > 0)
            {
                await context.StudentExams.AddRangeAsync(toAdd);
                await context.SaveChangesAsync();
                Console.WriteLine($"Seeded {toAdd.Count} student-exam records.");
            }
            else
            {
                Console.WriteLine("No new student-exam records to seed.");
            }
        }

        private static async Task SeedExamSchedulesAsync(AppDbContext context)
        {
            var rooms = await context.ExamRooms.AsNoTracking()
                .Where(r => r.Status == "Available" || string.IsNullOrWhiteSpace(r.Status))
                .ToListAsync();
            if (rooms.Count == 0)
            {
                Console.WriteLine("No rooms found. Skip exam schedule seeding.");
                return;
            }

            var subjects = await context.Subjects.AsNoTracking().ToListAsync();
            var classes = await context.Classes.AsNoTracking().ToListAsync();
            var majors = await context.Majors.AsNoTracking()
                .ToDictionaryAsync(m => m.Id, m => m.MajorCode);

            if (subjects.Count == 0 || classes.Count == 0)
            {
                Console.WriteLine("No subjects or classes found. Skip exam schedule seeding.");
                return;
            }

            var subjectByCode = subjects.ToDictionary(s => s.SubjectCode, s => s);
            var adminAccountId = await context.Accounts
                .Where(a => a.Role == "Admin" || a.Role == "SuperAdmin")
                .Select(a => a.Id)
                .FirstOrDefaultAsync();

            if (adminAccountId == 0)
            {
                adminAccountId = await context.Accounts.Select(a => a.Id).FirstOrDefaultAsync();
            }

            var year = DateTime.UtcNow.Year;
            var targetDates = new[]
            {
                UtcDate(year, 3, 22),
                UtcDate(year, 3, 23),
                UtcDate(year, 3, 24)
            };

            var slotTimes = new List<(TimeSpan Start, TimeSpan End)>
            {
                (new TimeSpan(7, 30, 0), new TimeSpan(9, 10, 0)),
                (new TimeSpan(9, 20, 0), new TimeSpan(10, 40, 0)),
                (new TimeSpan(10, 50, 0), new TimeSpan(12, 20, 0)),
                (new TimeSpan(12, 50, 0), new TimeSpan(14, 20, 0)),
                (new TimeSpan(14, 30, 0), new TimeSpan(16, 0, 0)),
                (new TimeSpan(16, 10, 0), new TimeSpan(17, 40, 0))
            };

            var existing = await context.Exams
                .Where(e => targetDates.Contains(e.ExamDate.Date))
                .Select(e => new { e.ExamDate, e.StartTime, e.EndTime, e.SubjectId, e.ClassId })
                .ToListAsync();
            var existingSet = new HashSet<string>(existing.Select(e => BuildExamKey(e.ExamDate.Date, e.StartTime, e.EndTime, e.SubjectId, e.ClassId)));

            var rand = new Random();
            var seeds = new List<ExamSeed>();

            foreach (var cls in classes)
            {
                if (!majors.TryGetValue(cls.MajorId, out var majorCode))
                    continue;

                var subjectCodes = ResolveSubjectCodesForMajor(majorCode)
                    .Where(code => subjectByCode.ContainsKey(code))
                    .ToList();
                if (subjectCodes.Count == 0)
                    continue;

                var takeCount = Math.Min(subjectCodes.Count, 3);
                var selectedCodes = subjectCodes.OrderBy(_ => rand.Next()).Take(takeCount).ToList();

                foreach (var code in selectedCodes)
                {
                    var subject = subjectByCode[code];
                    var date = targetDates[rand.Next(targetDates.Length)];
                    var slot = slotTimes[rand.Next(slotTimes.Count)];
                    var key = BuildExamKey(date, slot.Start, slot.End, subject.Id, cls.Id);

                    if (existingSet.Contains(key) || seeds.Any(s => s.Key == key))
                        continue;

                    seeds.Add(new ExamSeed
                    {
                        ClassId = cls.Id,
                        ClassCode = cls.ClassCode,
                        SubjectId = subject.Id,
                        SubjectCode = subject.SubjectCode,
                        SubjectName = subject.SubjectName,
                        ExamDate = date,
                        StartTime = slot.Start,
                        EndTime = slot.End,
                        AcademicYear = string.IsNullOrWhiteSpace(cls.AcademicYear) ? $"{year}-{year + 1}" : cls.AcademicYear,
                        Semester = cls.Semester > 0 ? cls.Semester.ToString() : "1"
                    });
                }
            }

            if (seeds.Count == 0)
            {
                Console.WriteLine("No new exam schedules to seed.");
                return;
            }

            var roomAssignments = new Dictionary<(DateTime Date, TimeSpan Start, TimeSpan End, int SubjectId), int>();
            var grouped = seeds.GroupBy(s => new { s.ExamDate, s.StartTime, s.EndTime }).ToList();
            foreach (var group in grouped)
            {
                var subjectsInSlot = group.Select(s => s.SubjectId).Distinct().OrderBy(_ => rand.Next()).ToList();
                var shuffledRooms = rooms.OrderBy(_ => rand.Next()).ToList();
                if (shuffledRooms.Count == 0)
                    break;

                var roomIndex = 0;
                var cursor = 0;
                while (cursor < subjectsInSlot.Count)
                {
                    var remaining = subjectsInSlot.Count - cursor;
                    var chunk = remaining >= 3 ? rand.Next(2, 4) : Math.Min(remaining, 2);
                    var room = shuffledRooms[roomIndex % shuffledRooms.Count];

                    for (var i = 0; i < chunk; i++)
                    {
                        var subjectId = subjectsInSlot[cursor + i];
                        roomAssignments[(group.Key.ExamDate, group.Key.StartTime, group.Key.EndTime, subjectId)] = room.Id;
                    }

                    cursor += chunk;
                    roomIndex++;
                }
            }

            var examsToAdd = new List<Exam>();
            foreach (var seed in seeds)
            {
                var roomId = roomAssignments.TryGetValue((seed.ExamDate, seed.StartTime, seed.EndTime, seed.SubjectId), out var assigned)
                    ? assigned
                    : rooms[rand.Next(rooms.Count)].Id;

                var duration = (int)(seed.EndTime - seed.StartTime).TotalMinutes;

                var exam = new Exam
                {
                    ExamCode = BuildExamCode(seed),
                    ExamName = $"{seed.SubjectCode} - {seed.ClassCode}",
                    ExamType = "ClassExam",
                    SubjectId = seed.SubjectId,
                    ClassId = seed.ClassId,
                    ExamDate = seed.ExamDate,
                    StartTime = seed.StartTime,
                    EndTime = seed.EndTime,
                    Duration = duration,
                    RoomId = roomId,
                    AcademicYear = seed.AcademicYear,
                    Semester = seed.Semester,
                    Status = "Planned",
                    Notes = "Seeded schedule",
                    CreatedBy = adminAccountId,
                    CreatedAt = DateTime.UtcNow
                };

                examsToAdd.Add(exam);
            }

            if (examsToAdd.Count > 0)
            {
                await context.Exams.AddRangeAsync(examsToAdd);
                await context.SaveChangesAsync();
                Console.WriteLine($"Seeded {examsToAdd.Count} exam schedules for 22-24/03.");
            }
        }

        private static async Task AllocateExamRoomsAsync(AppDbContext context)
        {
            try
            {
                await context.ExamRoomAllocations.AsNoTracking().Take(1).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ExamRoomAllocations table missing or unavailable. Skip room allocation. ({ex.Message})");
                return;
            }

            var exams = await context.Exams.AsNoTracking().ToListAsync();
            var rooms = await context.ExamRooms.AsNoTracking()
                .Where(r => r.Status == "Available" || string.IsNullOrWhiteSpace(r.Status))
                .ToListAsync();

            if (exams.Count == 0 || rooms.Count == 0)
            {
                Console.WriteLine("No exams or rooms found. Skip room allocation.");
                return;
            }

            var rand = new Random();
            var grouped = exams.GroupBy(e => new { e.ExamDate.Date, e.StartTime, e.EndTime }).ToList();

            foreach (var slot in grouped)
            {
                var slotExamIds = slot.Select(e => e.Id).ToList();
                var studentExams = await context.StudentExams
                    .Where(se => slotExamIds.Contains(se.ExamId))
                    .ToListAsync();

                var allocationsToRemove = await context.ExamRoomAllocations
                    .Where(a => slotExamIds.Contains(a.ExamId))
                    .ToListAsync();
                if (allocationsToRemove.Count > 0)
                {
                    context.ExamRoomAllocations.RemoveRange(allocationsToRemove);
                    await context.SaveChangesAsync();
                }

                var examQueues = studentExams
                    .GroupBy(se => se.ExamId)
                    .ToDictionary(g => g.Key, g => new Queue<int>(g.Select(x => x.StudentId).OrderBy(_ => rand.Next())));

                var remainingExamIds = new HashSet<int>(examQueues.Keys.Where(id => examQueues[id].Count > 0));
                if (remainingExamIds.Count == 0)
                    continue;

                var shuffledRooms = rooms.OrderBy(_ => rand.Next()).ToList();
                var allocations = new List<ExamRoomAllocation>();

                foreach (var room in shuffledRooms)
                {
                    if (remainingExamIds.Count == 0)
                        break;

                    var capacity = room.Capacity > 0 ? Math.Min(room.Capacity, 30) : 30;
                    var subjectCount = remainingExamIds.Count >= 3 ? rand.Next(2, 4) : Math.Min(remainingExamIds.Count, 2);
                    if (subjectCount <= 0)
                        break;

                    var selectedExams = remainingExamIds
                        .OrderByDescending(id => examQueues[id].Count)
                        .ThenBy(_ => rand.Next())
                        .Take(subjectCount)
                        .ToList();
                    var active = new List<int>(selectedExams);
                    var seatNumber = 1;

                    while (capacity > 0 && active.Count > 0)
                    {
                        for (var i = 0; i < active.Count && capacity > 0; i++)
                        {
                            var examId = active[i];
                            var queue = examQueues[examId];
                            if (queue.Count == 0)
                            {
                                active.RemoveAt(i);
                                i--;
                                continue;
                            }

                            var studentId = queue.Dequeue();
                            allocations.Add(new ExamRoomAllocation
                            {
                                ExamId = examId,
                                StudentId = studentId,
                                RoomId = room.Id,
                                SeatNumber = seatNumber,
                                CreatedAt = DateTime.UtcNow
                            });
                            seatNumber++;
                            capacity--;
                        }
                    }

                    foreach (var examId in selectedExams)
                    {
                        if (examQueues[examId].Count == 0)
                        {
                            remainingExamIds.Remove(examId);
                        }
                    }
                }

                if (allocations.Count > 0)
                {
                    await context.ExamRoomAllocations.AddRangeAsync(allocations);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"Allocated {allocations.Count} students to rooms for slot {slot.Key.Date:yyyy-MM-dd} {slot.Key.StartTime:hh\\:mm}-{slot.Key.EndTime:hh\\:mm}.");
                }
            }
        }

        private static int? ParseSemester(string? semester)
        {
            if (string.IsNullOrWhiteSpace(semester))
                return null;

            return int.TryParse(semester, out var value) ? value : null;
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

        private sealed record ExamSeed
        {
            public int ClassId { get; init; }
            public string ClassCode { get; init; } = string.Empty;
            public int SubjectId { get; init; }
            public string SubjectCode { get; init; } = string.Empty;
            public string SubjectName { get; init; } = string.Empty;
            public DateTime ExamDate { get; init; }
            public TimeSpan StartTime { get; init; }
            public TimeSpan EndTime { get; init; }
            public string AcademicYear { get; init; } = string.Empty;
            public string Semester { get; init; } = string.Empty;

            public string Key => BuildExamKey(ExamDate, StartTime, EndTime, SubjectId, ClassId);
        }

        private static string BuildExamKey(DateTime date, TimeSpan start, TimeSpan end, int subjectId, int classId)
        {
            return $"{date:yyyyMMdd}-{start:hhmm}-{end:hhmm}-{subjectId}-{classId}";
        }

        private static string BuildExamCode(ExamSeed seed)
        {
            // Keep within varchar(20) while staying unique per class/subject/slot
            return $"EX{seed.SubjectId}-{seed.ClassId}-{seed.ExamDate:MMdd}{seed.StartTime:hhmm}";
        }

        private static DateTime UtcDate(int year, int month, int day)
        {
            return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
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
