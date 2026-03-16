using e360_clone.BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace e360_clone.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamRoom> ExamRooms { get; set; }
        public DbSet<ExamSchedule> ExamSchedules { get; set; }
        public DbSet<ProctorAssignment> ProctorAssignments { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Account> Accounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Student configuration
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StudentCode).HasMaxLength(20).IsRequired();
                entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
                entity.HasIndex(e => e.StudentCode).IsUnique();
            });

            // Lecturer configuration
            modelBuilder.Entity<Lecturer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EmployeeCode).HasMaxLength(20).IsRequired();
                entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Department).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
                entity.HasIndex(e => e.EmployeeCode).IsUnique();
            });

            // Subject configuration
            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SubjectCode).HasMaxLength(20).IsRequired();
                entity.Property(e => e.SubjectName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.SubjectType).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
                entity.HasIndex(e => e.SubjectCode).IsUnique();
            });

            // Class configuration
            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ClassCode).HasMaxLength(20).IsRequired();
                entity.Property(e => e.ClassName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.AcademicYear).HasMaxLength(20);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
                entity.HasIndex(e => e.ClassCode).IsUnique();
            });

            // ExamRoom configuration
            modelBuilder.Entity<ExamRoom>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RoomCode).HasMaxLength(20).IsRequired();
                entity.Property(e => e.RoomName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Building).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Available");
                entity.HasIndex(e => e.RoomCode).IsUnique();
            });

            // Exam configuration
            modelBuilder.Entity<Exam>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ExamCode).HasMaxLength(20).IsRequired();
                entity.Property(e => e.ExamName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.ExamType).HasMaxLength(50);
                entity.Property(e => e.AcademicYear).HasMaxLength(20);
                entity.Property(e => e.Semester).HasMaxLength(20);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Planned");
                entity.HasIndex(e => e.ExamCode).IsUnique();
            });

            // ExamSchedule configuration
            modelBuilder.Entity<ExamSchedule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Scheduled");
                entity.Property(e => e.Notes).HasMaxLength(500);
            });

            // ProctorAssignment configuration
            modelBuilder.Entity<ProctorAssignment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Role).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Assigned");
                entity.Property(e => e.Notes).HasMaxLength(500);
            });

            // Grade configuration
            modelBuilder.Entity<Grade>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ScoreType).HasMaxLength(50);
                entity.Property(e => e.LetterGrade).HasMaxLength(5);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Draft");
            });

            // Attendance configuration
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Present");
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.Violation).HasMaxLength(200);
            });

            // Relationships
            modelBuilder.Entity<Exam>()
                .HasOne<Subject>()
                .WithMany()
                .HasForeignKey(e => e.SubjectId);

            modelBuilder.Entity<Exam>()
                .HasOne<Class>()
                .WithMany()
                .HasForeignKey(e => e.ClassId);

            modelBuilder.Entity<Exam>()
                .HasOne<ExamRoom>()
                .WithMany()
                .HasForeignKey(e => e.RoomId);

            modelBuilder.Entity<ExamSchedule>()
                .HasOne<Exam>()
                .WithMany()
                .HasForeignKey(es => es.ExamId);

            modelBuilder.Entity<ExamSchedule>()
                .HasOne<ExamRoom>()
                .WithMany()
                .HasForeignKey(es => es.RoomId);

            modelBuilder.Entity<ProctorAssignment>()
                .HasOne<ExamSchedule>()
                .WithMany()
                .HasForeignKey(pa => pa.ExamScheduleId);

            modelBuilder.Entity<ProctorAssignment>()
                .HasOne<Lecturer>()
                .WithMany()
                .HasForeignKey(pa => pa.LecturerId);

            modelBuilder.Entity<Grade>()
                .HasOne<Student>()
                .WithMany()
                .HasForeignKey(g => g.StudentId);

            modelBuilder.Entity<Grade>()
                .HasOne<Exam>()
                .WithMany()
                .HasForeignKey(g => g.ExamId);

            modelBuilder.Entity<Attendance>()
                .HasOne<ExamSchedule>()
                .WithMany()
                .HasForeignKey(a => a.ExamScheduleId);

            modelBuilder.Entity<Attendance>()
                .HasOne<Student>()
                .WithMany()
                .HasForeignKey(a => a.StudentId);

            modelBuilder.Entity<Class>()
                .HasOne<Subject>()
                .WithMany()
                .HasForeignKey(c => c.MajorId);

            // Account configuration
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
                entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Role).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();

                // Relationships
                entity.HasOne(a => a.Student)
                    .WithMany()
                    .HasForeignKey(a => a.StudentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(a => a.Lecturer)
                    .WithMany()
                    .HasForeignKey(a => a.LecturerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
