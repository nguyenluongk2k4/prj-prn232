using e360_clone.BusinessObjects.DTOs;
using e360_clone.BusinessObjects.DTOs;
using e360_clone_fe.Models.ViewModels;
using e360_clone_fe.Services;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone_fe.Controllers
{
    public class DashboardController : BaseController
    {
        public DashboardController(IApiService apiService, ILogger<DashboardController> logger)
            : base(apiService, logger)
        {
        }

        // Main dashboard - auto redirect based on role
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            
            if (string.IsNullOrEmpty(role))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Redirect to role-specific dashboard
            return role switch
            {
                "Student" => RedirectToAction("Student"),
                "Teacher" => RedirectToAction("Teacher"),
                "Parent" => RedirectToAction("Parent"),
                "Admin" or "SuperAdmin" or "Staff" => RedirectToAction("School"),
                _ => RedirectToAction("LMS")
            };
        }

        // Admin/School Dashboard - Admin, Staff ONLY
        [HttpGet]
        public async Task<IActionResult> School()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || !IsAdminOrStaff(role))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }
            ViewData["Role"] = role;
            var summaryResponse = await _apiService.GetAsync<AdminDashboardSummaryDto>("dashboard/summary");
            var model = summaryResponse.Success && summaryResponse.Data != null
                ? summaryResponse.Data
                : new AdminDashboardSummaryDto();
            return View(model);
        }

        // Student Dashboard - Student ONLY
        [HttpGet]
        public async Task<IActionResult> Student()
        {
            var authResult = RequireRole("Student");
            if (authResult != null) return authResult;

            ViewData["Role"] = "Student";
            var model = new StudentDashboardViewModel
            {
                StudentName = HttpContext.Session.GetString("FullName") ?? "Sinh viên",
                AvatarUrl = HttpContext.Session.GetString("AvatarUrl") ?? "/assets/images/thumbs/student-profile.png"
            };

            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (!studentId.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sinh viên cho tài khoản này.";
                return View(model);
            }

            var classesResponse = await _apiService.GetAsync<List<ClassFormViewModel>>(
                "classes",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var subjectsResponse = await _apiService.GetAsync<List<SubjectFormViewModel>>(
                "subjects",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var roomsResponse = await _apiService.GetAsync<List<ExamRoomViewModel>>(
                "rooms",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });

            var classMap = classesResponse.Success && classesResponse.Data != null
                ? classesResponse.Data.ToDictionary(c => c.Id, c => c)
                : new Dictionary<int, ClassFormViewModel>();
            var subjectMap = subjectsResponse.Success && subjectsResponse.Data != null
                ? subjectsResponse.Data.ToDictionary(s => s.Id, s => s)
                : new Dictionary<int, SubjectFormViewModel>();
            var roomMap = roomsResponse.Success && roomsResponse.Data != null
                ? roomsResponse.Data.ToDictionary(r => r.Id, r => r)
                : new Dictionary<int, ExamRoomViewModel>();

            var studentResponse = await _apiService.GetAsync<StudentViewModel>($"students/{studentId.Value}");
            if (studentResponse.Success && studentResponse.Data != null)
            {
                model.StudentCode = studentResponse.Data.StudentCode;
                model.ClassId = studentResponse.Data.ClassId;
                if (classMap.TryGetValue(studentResponse.Data.ClassId, out var studentClass))
                {
                    model.ClassCode = studentClass.ClassCode;
                }
            }

            var fromDate = DateTime.Today;
            var toDate = DateTime.Today.AddDays(14);
            var examsResponse = await _apiService.GetAsync<List<StudentExamScheduleDto>>(
                "exams/student",
                new Dictionary<string, string>
                {
                    { "studentId", studentId.Value.ToString() },
                    { "fromDate", fromDate.ToString("yyyy-MM-dd") },
                    { "toDate", toDate.ToString("yyyy-MM-dd") }
                });

            var exams = examsResponse.Success && examsResponse.Data != null
                ? examsResponse.Data
                : new List<StudentExamScheduleDto>();

            model.UpcomingExamCount = exams.Count;
            model.UpcomingExams = exams
                .OrderBy(x => x.ExamDate)
                .ThenBy(x => x.StartTime)
                .Take(6)
                .Select(x =>
                {
                    subjectMap.TryGetValue(x.SubjectId, out var subject);
                    classMap.TryGetValue(x.ClassId, out var cls);
                    roomMap.TryGetValue(x.RoomId, out var room);
                    return new StudentDashboardExamItemViewModel
                    {
                        ExamId = x.Id,
                        ExamDate = x.ExamDate,
                        StartTime = x.StartTime,
                        EndTime = x.EndTime,
                        SubjectCode = subject?.SubjectCode ?? $"Môn #{x.SubjectId}",
                        SubjectName = subject?.SubjectName ?? string.Empty,
                        ClassCode = cls?.ClassCode ?? string.Empty,
                        RoomCode = room?.RoomCode ?? string.Empty,
                        SeatNumber = x.SeatNumber
                    };
                })
                .ToList();

            var enrollmentsResponse = await _apiService.GetAsync<List<StudentSubjectViewModel>>(
                "student-subjects",
                new Dictionary<string, string> { { "studentId", studentId.Value.ToString() } });

            if (enrollmentsResponse.Success && enrollmentsResponse.Data != null)
            {
                model.SubjectCount = enrollmentsResponse.Data
                    .Select(x => x.SubjectId)
                    .Distinct()
                    .Count();
            }

            return View(model);
        }

        // Teacher Dashboard - Teacher ONLY
        [HttpGet]
        public async Task<IActionResult> Teacher()
        {
            var authResult = RequireRole("Teacher");
            if (authResult != null) return authResult;

            ViewData["Role"] = "Teacher";
            var model = new TeacherDashboardViewModel
            {
                TeacherName = HttpContext.Session.GetString("FullName") ?? "Giảng viên",
                Email = HttpContext.Session.GetString("Email") ?? string.Empty,
                AvatarUrl = HttpContext.Session.GetString("AvatarUrl") ?? "/assets/images/thumbs/teacher-profile.png"
            };

            var lecturerId = HttpContext.Session.GetInt32("LecturerId");
            var email = HttpContext.Session.GetString("Email");

            var lecturersResponse = await _apiService.GetAsync<List<LecturerViewModel>>(
                "lecturers",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });

            if (!lecturerId.HasValue && !string.IsNullOrWhiteSpace(email) &&
                lecturersResponse.Success && lecturersResponse.Data != null)
            {
                var match = lecturersResponse.Data.FirstOrDefault(l =>
                    string.Equals(l.Email, email, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    lecturerId = match.Id;
                    HttpContext.Session.SetInt32("LecturerId", match.Id);
                }
            }

            if (!lecturerId.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy giảng viên cho tài khoản này.";
                return View(model);
            }

            var lecturerInfo = lecturersResponse.Success && lecturersResponse.Data != null
                ? lecturersResponse.Data.FirstOrDefault(l => l.Id == lecturerId.Value)
                : null;
            if (lecturerInfo != null)
            {
                model.TeacherName = lecturerInfo.FullName;
                model.TeacherCode = lecturerInfo.EmployeeCode;
                model.Email = lecturerInfo.Email;
            }

            var proctorResponse = await _apiService.GetAsync<List<ProctorAssignmentViewModel>>(
                "proctors",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var examsResponse = await _apiService.GetAsync<List<ExamViewModel>>(
                "exams",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var classResponse = await _apiService.GetAsync<List<ClassFormViewModel>>(
                "classes",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var subjectResponse = await _apiService.GetAsync<List<SubjectFormViewModel>>(
                "subjects",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var roomResponse = await _apiService.GetAsync<List<ExamRoomViewModel>>(
                "rooms",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });

            var classMap = classResponse.Success && classResponse.Data != null
                ? classResponse.Data.ToDictionary(c => c.Id, c => c)
                : new Dictionary<int, ClassFormViewModel>();
            var subjectMap = subjectResponse.Success && subjectResponse.Data != null
                ? subjectResponse.Data.ToDictionary(s => s.Id, s => s)
                : new Dictionary<int, SubjectFormViewModel>();
            var roomMap = roomResponse.Success && roomResponse.Data != null
                ? roomResponse.Data.ToDictionary(r => r.Id, r => r)
                : new Dictionary<int, ExamRoomViewModel>();
            var examMap = examsResponse.Success && examsResponse.Data != null
                ? examsResponse.Data.ToDictionary(e => e.Id, e => e)
                : new Dictionary<int, ExamViewModel>();

            var assignments = proctorResponse.Success && proctorResponse.Data != null
                ? proctorResponse.Data.Where(p => p.LecturerId == lecturerId.Value).ToList()
                : new List<ProctorAssignmentViewModel>();

            var assignedExamIds = assignments.Select(a => a.ExamId).Distinct().ToHashSet();
            var assignedExams = assignedExamIds
                .Select(id => examMap.TryGetValue(id, out var exam) ? exam : null)
                .Where(exam => exam != null)
                .Cast<ExamViewModel>()
                .ToList();

            model.TotalAssignments = assignedExams.Count;

            var today = DateTime.Today;
            var upcomingEnd = today.AddDays(14);
            var weekEnd = today.AddDays(7);
            var upcomingExams = assignedExams
                .Where(e => e.ExamDate.Date >= today && e.ExamDate.Date <= upcomingEnd)
                .OrderBy(e => e.ExamDate)
                .ThenBy(e => e.StartTime)
                .ToList();

            model.TodayAssignments = assignedExams.Count(e => e.ExamDate.Date == today);
            model.WeekAssignments = assignedExams.Count(e => e.ExamDate.Date >= today && e.ExamDate.Date <= weekEnd);
            model.UpcomingAssignments = upcomingExams.Count;
            model.UpcomingExams = upcomingExams
                .Take(6)
                .Select(e =>
                {
                    subjectMap.TryGetValue(e.SubjectId, out var subject);
                    classMap.TryGetValue(e.ClassId, out var cls);
                    roomMap.TryGetValue(e.RoomId, out var room);
                    return new TeacherDashboardExamItemViewModel
                    {
                        ExamId = e.Id,
                        ExamDate = e.ExamDate,
                        StartTime = e.StartTime,
                        EndTime = e.EndTime,
                        SubjectCode = subject?.SubjectCode ?? string.Empty,
                        SubjectName = subject?.SubjectName ?? string.Empty,
                        ClassCode = cls?.ClassCode ?? string.Empty,
                        RoomCode = room?.RoomCode ?? string.Empty
                    };
                })
                .ToList();

            var reportFrom = today.AddDays(-7);
            var reportTo = today.AddDays(14);
            var reportResponse = await _apiService.GetAsync<List<AttendanceReportItemDto>>(
                "attendances/report",
                new Dictionary<string, string>
                {
                    { "fromDate", reportFrom.ToString("yyyy-MM-dd") },
                    { "toDate", reportTo.ToString("yyyy-MM-dd") }
                });

            if (reportResponse.Success && reportResponse.Data != null)
            {
                var unconfirmed = reportResponse.Data
                    .Where(r => assignedExamIds.Contains(r.ExamId))
                    .Select(r => new
                    {
                        Report = r,
                        Missing = Math.Max(0, r.Total - r.Confirmed)
                    })
                    .Where(x => x.Missing > 0)
                    .OrderBy(x => x.Report.ExamDate)
                    .ThenBy(x => x.Report.StartTime)
                    .ToList();

                model.UnconfirmedAssignments = unconfirmed.Count;
                model.UnconfirmedExams = unconfirmed
                    .Take(6)
                    .Select(x =>
                    {
                        subjectMap.TryGetValue(x.Report.SubjectId, out var subject);
                        classMap.TryGetValue(x.Report.ClassId, out var cls);
                        var roomCode = string.Empty;
                        if (examMap.TryGetValue(x.Report.ExamId, out var exam) &&
                            roomMap.TryGetValue(exam.RoomId, out var room))
                        {
                            roomCode = room.RoomCode;
                        }

                        return new TeacherDashboardUnconfirmedItemViewModel
                        {
                            ExamId = x.Report.ExamId,
                            ExamDate = x.Report.ExamDate,
                            StartTime = x.Report.StartTime,
                            EndTime = x.Report.EndTime,
                            SubjectCode = subject?.SubjectCode ?? x.Report.SubjectCode,
                            SubjectName = subject?.SubjectName ?? x.Report.SubjectName,
                            ClassCode = cls?.ClassCode ?? x.Report.ClassCode,
                            RoomCode = roomCode,
                            UnconfirmedCount = x.Missing,
                            TotalCount = x.Report.Total
                        };
                    })
                    .ToList();

                var assignedReports = reportResponse.Data
                    .Where(r => assignedExamIds.Contains(r.ExamId))
                    .ToList();
                if (assignedReports.Count > 0)
                {
                    var totalSigned = assignedReports.Sum(r => r.Confirmed);
                    var totalStudents = assignedReports.Sum(r => r.Total);
                    model.ConfirmedRate = totalStudents > 0
                        ? (int)Math.Round(totalSigned * 100m / totalStudents, MidpointRounding.AwayFromZero)
                        : 0;
                }
            }

            return View(model);
        }

        // Parent Dashboard - Parent ONLY
        [HttpGet]
        public IActionResult Parent()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Parent")
            {
                return RedirectToAction("AccessDenied", "Auth");
            }
            ViewData["Role"] = role;
            return View();
        }

        // LMS Dashboard - ALL ROLES
        [HttpGet]
        public IActionResult LMS()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role))
            {
                return RedirectToAction("Login", "Auth");
            }
            ViewData["Role"] = role;
            return View();
        }

        // Helper method
        private bool IsAdminOrStaff(string role)
        {
            return role == "Admin" || role == "SuperAdmin" || role == "Staff";
        }

    }
}
