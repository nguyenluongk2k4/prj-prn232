using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using e360_clone.BusinessObjects;

namespace e360_clone.DataAccess.Configurations
{
    public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
    {
        public void Configure(EntityTypeBuilder<Subject> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.SubjectCode)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(e => e.SubjectName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(e => e.SubjectType)
                .HasMaxLength(50);

            builder.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            builder.HasIndex(e => e.SubjectCode)
                .IsUnique();
        }
    }
}
