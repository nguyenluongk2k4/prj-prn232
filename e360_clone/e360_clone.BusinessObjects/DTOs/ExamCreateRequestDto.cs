namespace e360_clone.BusinessObjects.DTOs
{
    public class ExamCreateRequestDto
    {
        public Exam Exam { get; set; } = new Exam();
        public List<int> RoomIds { get; set; } = new();
        public bool ApplyAllClasses { get; set; }
    }

    public class ClassLookupDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}
