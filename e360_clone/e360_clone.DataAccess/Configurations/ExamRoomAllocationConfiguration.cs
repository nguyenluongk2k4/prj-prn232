using e360_clone.BusinessObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace e360_clone.DataAccess.Configurations
{
    public class ExamRoomAllocationConfiguration : IEntityTypeConfiguration<ExamRoomAllocation>
    {
        public void Configure(EntityTypeBuilder<ExamRoomAllocation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.ExamId, x.StudentId }).IsUnique();
            builder.HasIndex(x => x.RoomId);
            builder.HasIndex(x => x.ExamId);
            builder.HasIndex(x => x.RoomId, "IX_ExamRoomAllocations_RoomId_SeatNumber");

            builder.HasOne(x => x.Exam)
                .WithMany()
                .HasForeignKey(x => x.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Student)
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Room)
                .WithMany()
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
