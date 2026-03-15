namespace e360_clone.BusinessObjects.Enums
{
    public enum ExamType
    {
        ClassExam,          // Thi lớp học
        GraduationExam      // Thi tốt nghiệp
    }

    public enum ExamStatus
    {
        Planned,            // Đã lên kế hoạch
        Scheduled,          // Đã có lịch
        InProgress,         // Đang diễn ra
        Completed,          // Hoàn thành
        Cancelled           // Đã hủy
    }

    public enum ProctorRole
    {
        ChiefProctor,       // Giám thị chính
        Proctor             // Giám thị
    }

    public enum ProctorStatus
    {
        Assigned,           // Đã phân công
        Confirmed,          // Đã xác nhận
        Declined,           // Đã từ chối
        Completed           // Đã hoàn thành
    }

    public enum GradeStatus
    {
        Draft,              // Nháp
        Submitted,          // Đã submit
        Approved,           // Đã duyệt
        Published           // Đã công bố
    }

    public enum AttendanceStatus
    {
        Present,            // Có mặt
        Late,               // Đi muộn
        Absent,             // Vắng mặt
        Excused             // Vắng có phép
    }

    public enum ViolationType
    {
        None,               // Không có
        Cheating,           // Gian lận
        Disruptive,         // Gây mất trật tự
        EarlySubmission,    // Nộp bài sớm
        HealthIssue         // Vấn đề sức khỏe
    }

    public enum SubjectType
    {
        Core,               // Môn bắt buộc
        Elective,           // Môn tự chọn
        General             // Môn đại cương
    }

    public enum RoomStatus
    {
        Available,          // Sẵn sàng
        InUse,              // Đang sử dụng
        Maintenance         // Bảo trì
    }
}
