using System.ComponentModel.DataAnnotations;

namespace e360_clone.BusinessObjects
{
    public class ExamRoomAllocation
    {
        public int Id { get; set; }

        [Required]
        public int ExamId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int RoomId { get; set; }

        public int SeatNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Exam? Exam { get; set; }
        public Student? Student { get; set; }
        public ExamRoom? Room { get; set; }
    }
}
