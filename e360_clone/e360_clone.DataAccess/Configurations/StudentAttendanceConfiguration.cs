using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using e360_clone.BusinessObjects;

namespace e360_clone.DataAccess.Configurations
{
    public class StudentAttendanceConfiguration : IEntityTypeConfiguration<StudentAttendance>
    {
        public void Configure(EntityTypeBuilder<StudentAttendance> builder)
        {
            builder.HasKey(sa => sa.Id);

            // Indexes
            builder.HasIndex(sa => new { sa.CourseSessionId, sa.StudentId }).IsUnique();
            builder.HasIndex(sa => sa.StudentId);
            builder.HasIndex(sa => sa.CourseSessionId);

            // Relationships
            builder.HasOne(sa => sa.Session)
                .WithMany(cs => cs.Attendances)
                .HasForeignKey(sa => sa.CourseSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sa => sa.StudentSubject)
                .WithMany(ss => ss.Attendances)
                .HasForeignKey(sa => sa.StudentSubjectId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
