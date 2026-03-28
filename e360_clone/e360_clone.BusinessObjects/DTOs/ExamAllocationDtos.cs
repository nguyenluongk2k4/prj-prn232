namespace e360_clone.BusinessObjects.DTOs
{
    public class ExamAllocationStudentDto
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassCode { get; set; } = string.Empty;
    }

    public class ExamAllocationRoomDto
    {
        public int RoomId { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public int Capacity { get; set; }
    }

    public class ExamAllocationItemDto
    {
        public int StudentId { get; set; }
        public int RoomId { get; set; }
        public int SeatNumber { get; set; }
    }

    public class ExamAllocationSummaryDto
    {
        public int ExamId { get; set; }
        public int SubjectId { get; set; }
        public int ClassId { get; set; }
        public int StudentCount { get; set; }
        public int AllocatedCount { get; set; }
        public List<ExamAllocationStudentDto> Students { get; set; } = new();
        public List<ExamAllocationRoomDto> Rooms { get; set; } = new();
        public List<ExamAllocationItemDto> Allocations { get; set; } = new();
    }

    public class ExamAllocationSaveRequestDto
    {
        public List<ExamAllocationItemDto> Allocations { get; set; } = new();
    }

    public class ExamAllocationAutoRequestDto
    {
        public List<int> RoomIds { get; set; } = new();
    }
}
