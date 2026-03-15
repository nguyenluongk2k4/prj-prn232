namespace e360_clone.BusinessObjects
{
    public class ExamRoom
    {
        public int Id { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string Floor { get; set; } = string.Empty;
        public bool HasComputer { get; set; }
        public bool HasProjector { get; set; }
        public string Status { get; set; } = string.Empty; // Available, InUse, Maintenance
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
