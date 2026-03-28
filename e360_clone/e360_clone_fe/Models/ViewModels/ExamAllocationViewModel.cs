namespace e360_clone_fe.Models.ViewModels
{
    public class ExamAllocationStudentViewModel
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassCode { get; set; } = string.Empty;
    }

    public class ExamAllocationRoomViewModel
    {
        public int RoomId { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public int Capacity { get; set; }
    }

    public class ExamAllocationItemViewModel
    {
        public int StudentId { get; set; }
        public int RoomId { get; set; }
        public int SeatNumber { get; set; }
    }

    public class ExamAllocationSummaryViewModel
    {
        public int ExamId { get; set; }
        public int SubjectId { get; set; }
        public int ClassId { get; set; }
        public int StudentCount { get; set; }
        public int AllocatedCount { get; set; }
        public List<ExamAllocationStudentViewModel> Students { get; set; } = new();
        public List<ExamAllocationRoomViewModel> Rooms { get; set; } = new();
        public List<ExamAllocationItemViewModel> Allocations { get; set; } = new();
    }

    public class ExamAllocationAutoRequestViewModel
    {
        public List<int> RoomIds { get; set; } = new();
    }

    public class ExamAllocationSaveRequestViewModel
    {
        public List<ExamAllocationItemViewModel> Allocations { get; set; } = new();
    }
}
