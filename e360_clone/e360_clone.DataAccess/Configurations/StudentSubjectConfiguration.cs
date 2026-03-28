using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using e360_clone.BusinessObjects;

namespace e360_clone.DataAccess.Configurations
{
    public class StudentSubjectConfiguration : IEntityTypeConfiguration<StudentSubject>
    {
        public void Configure(EntityTypeBuilder<StudentSubject> builder)
        {
            builder.HasKey(ss => ss.Id);

            // Indexes
            builder.HasIndex(ss => new { ss.StudentId, ss.SubjectId, ss.TermId });
            builder.HasIndex(ss => ss.StudentId);
            builder.HasIndex(ss => ss.SubjectId);

            // Relationships
            builder.HasOne(ss => ss.Student)
                .WithMany()
                .HasForeignKey(ss => ss.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ss => ss.Subject)
                .WithMany()
                .HasForeignKey(ss => ss.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ss => ss.Class)
                .WithMany()
                .HasForeignKey(ss => ss.ClassId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(ss => ss.Term)
                .WithMany()
                .HasForeignKey(ss => ss.TermId)
                .OnDelete(DeleteBehavior.Restrict);

            // HasMany for attendances (through StudentSubject -> StudentAttendances)
            // Note: Attendances are linked through CourseSession, not directly
        }
    }
}
