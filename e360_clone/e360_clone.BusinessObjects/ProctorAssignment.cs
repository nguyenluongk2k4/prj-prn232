namespace e360_clone.BusinessObjects
{
    public class ProctorAssignment
    {
        public int Id { get; set; }
        public int ExamId { get; set; }                        // FK trực tiếp vào Exam
        public int LecturerId { get; set; }
        public string Role { get; set; } = string.Empty;      // ChiefProctor, Proctor
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = string.Empty;    // Assigned, Confirmed, Declined, Completed
        public string Notes { get; set; } = string.Empty;
    }
}
