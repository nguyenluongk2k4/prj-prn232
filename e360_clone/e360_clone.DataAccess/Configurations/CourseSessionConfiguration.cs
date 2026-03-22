using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using e360_clone.BusinessObjects;

namespace e360_clone.DataAccess.Configurations
{
    public class CourseSessionConfiguration : IEntityTypeConfiguration<CourseSession>
    {
        public void Configure(EntityTypeBuilder<CourseSession> builder)
        {
            builder.HasKey(cs => cs.Id);

            // Indexes
            builder.HasIndex(cs => new { cs.SubjectId, cs.ClassId, cs.SessionNumber });
            builder.HasIndex(cs => cs.Date);

            // Relationships
            builder.HasOne(cs => cs.Subject)
                .WithMany()
                .HasForeignKey(cs => cs.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cs => cs.Class)
                .WithMany()
                .HasForeignKey(cs => cs.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            // HasMany for attendances
            builder.HasMany(cs => cs.Attendances)
                .WithOne(sa => sa.Session)
                .HasForeignKey(sa => sa.CourseSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
