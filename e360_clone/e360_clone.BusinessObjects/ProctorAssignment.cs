namespace e360_clone.BusinessObjects
{
    public class ProctorAssignment
    {
        public int Id { get; set; }
        public int ExamScheduleId { get; set; }
        public int LecturerId { get; set; }
        public string Role { get; set; } = string.Empty; // ChiefProctor, Proctor
        public DateTime AssignedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = string.Empty; // Assigned, Confirmed, Declined, Completed
        public string Notes { get; set; } = string.Empty;
    }
}
