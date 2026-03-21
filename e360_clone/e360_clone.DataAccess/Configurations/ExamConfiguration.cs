using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using e360_clone.BusinessObjects;

namespace e360_clone.DataAccess.Configurations
{
    public class ExamConfiguration : IEntityTypeConfiguration<Exam>
    {
        public void Configure(EntityTypeBuilder<Exam> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.ExamCode)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(e => e.ExamName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(e => e.ExamType)
                .HasMaxLength(50);

            builder.Property(e => e.AcademicYear)
                .HasMaxLength(20);

            builder.Property(e => e.Semester)
                .HasMaxLength(20);

            builder.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Planned");

            builder.HasIndex(e => e.ExamCode)
                .IsUnique();
        }
    }
}
