using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using e360_clone.BusinessObjects;

namespace e360_clone.DataAccess.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Username)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.Email)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.Role)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            builder.HasIndex(e => e.Username)
                .IsUnique();

            builder.HasIndex(e => e.Email)
                .IsUnique();

            // Relationships
            builder.HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(a => a.Lecturer)
                .WithMany()
                .HasForeignKey(a => a.LecturerId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
