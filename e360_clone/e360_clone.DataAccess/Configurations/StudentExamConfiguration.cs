using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using e360_clone.BusinessObjects;

namespace e360_clone.DataAccess.Configurations
{
    public class StudentExamConfiguration : IEntityTypeConfiguration<StudentExam>
    {
        public void Configure(EntityTypeBuilder<StudentExam> builder)
        {
            builder.HasKey(se => se.Id);

            // Unique constraint
            builder.HasIndex(se => new { se.ExamId, se.StudentId }).IsUnique();
            builder.HasIndex(se => se.StudentId);
            builder.HasIndex(se => se.ExamId);

            // Relationships
            builder.HasOne(se => se.Exam)
                .WithMany()
                .HasForeignKey(se => se.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(se => se.Student)
                .WithMany()
                .HasForeignKey(se => se.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
