using e360_clone.BusinessObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace e360_clone.DataAccess.Configurations
{
    public class TeachingAssignmentConfiguration : IEntityTypeConfiguration<TeachingAssignment>
    {
        public void Configure(EntityTypeBuilder<TeachingAssignment> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.AcademicYear)
                .HasMaxLength(20);

            builder.Property(e => e.Semester)
                .HasMaxLength(20);

            builder.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            builder.HasIndex(e => new { e.LecturerId, e.SubjectId, e.ClassId, e.AcademicYear, e.Semester })
                .IsUnique();
        }
    }
}
