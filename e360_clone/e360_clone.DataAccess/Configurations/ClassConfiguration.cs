using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using e360_clone.BusinessObjects;

namespace e360_clone.DataAccess.Configurations
{
    public class ClassConfiguration : IEntityTypeConfiguration<Class>
    {
        public void Configure(EntityTypeBuilder<Class> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.ClassCode)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(e => e.ClassName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.AcademicYear)
                .HasMaxLength(20);

            builder.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            builder.HasIndex(e => e.ClassCode)
                .IsUnique();
        }
    }
}
