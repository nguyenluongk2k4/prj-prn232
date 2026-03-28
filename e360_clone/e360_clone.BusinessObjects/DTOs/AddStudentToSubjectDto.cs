namespace e360_clone.BusinessObjects.DTOs
{
    public class AddStudentToSubjectRequestDto
    {
        public int SubjectId { get; set; }
        public int ClassId { get; set; }
        public List<int> StudentIds { get; set; } = new();
    }
}
