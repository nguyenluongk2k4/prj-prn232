using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using e360_clone.BusinessObjects;

namespace e360_clone.DataAccess.Configurations
{
    public class ExamFormConfiguration : IEntityTypeConfiguration<ExamForm>
    {
        public void Configure(EntityTypeBuilder<ExamForm> builder)
        {
            builder.HasKey(ef => ef.Id);

            builder.HasIndex(ef => ef.ExamId);

            builder.HasOne(ef => ef.Exam)
                .WithMany()
                .HasForeignKey(ef => ef.ExamId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
