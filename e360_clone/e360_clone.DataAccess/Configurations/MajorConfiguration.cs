using e360_clone.BusinessObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace e360_clone.DataAccess.Configurations
{
    public class MajorConfiguration : IEntityTypeConfiguration<Major>
    {
        public void Configure(EntityTypeBuilder<Major> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.MajorCode)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(e => e.MajorName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.MajorGroup)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            builder.HasIndex(e => e.MajorCode)
                .IsUnique();
        }
    }
}
