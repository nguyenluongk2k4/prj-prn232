using e360_clone_fe.Models.ViewModels;
using e360_clone_fe.Services;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone_fe.Controllers
{
    public class ProfileController : BaseController
    {
        public ProfileController(IApiService apiService, ILogger<ProfileController> logger)
            : base(apiService, logger)
        {
        }

        public async Task<IActionResult> Index()
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var role = HttpContext.Session.GetString("Role");
            if (string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase))
            {
                return await StudentProfile();
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId");
            var email = HttpContext.Session.GetString("Email");
            var fullName = HttpContext.Session.GetString("FullName") ?? "Giảng viên";

            if (!lecturerId.HasValue && !string.IsNullOrWhiteSpace(email))
            {
                var lecturersResponse = await _apiService.GetAsync<List<LecturerViewModel>>(
                    "lecturers",
                    new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });

                if (lecturersResponse.Success && lecturersResponse.Data != null)
                {
                    var match = lecturersResponse.Data.FirstOrDefault(l =>
                        string.Equals(l.Email, email, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        lecturerId = match.Id;
                        HttpContext.Session.SetInt32("LecturerId", match.Id);
                        fullName = match.FullName;
                    }
                }
            }

            if (!lecturerId.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy giảng viên cho tài khoản này.";
                return View(new LecturerDetailsViewModel
                {
                    Lecturer = new LecturerViewModel { FullName = fullName }
                });
            }

            var lecturerResponse = await _apiService.GetAsync<LecturerViewModel>($"lecturers/{lecturerId.Value}");
            if (!lecturerResponse.Success || lecturerResponse.Data == null)
            {
                TempData["ErrorMessage"] = lecturerResponse.Message ?? "Không tìm thấy giảng viên";
                return View(new LecturerDetailsViewModel
                {
                    Lecturer = new LecturerViewModel { FullName = fullName }
                });
            }

            var assignmentsResponse = await _apiService.GetAsync<List<TeachingAssignmentViewModel>>(
                "teaching-assignments",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var classResponse = await _apiService.GetAsync<List<ClassFormViewModel>>(
                "classes",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var subjectResponse = await _apiService.GetAsync<List<SubjectFormViewModel>>(
                "subjects",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });

            var classMap = classResponse.Success && classResponse.Data != null
                ? classResponse.Data.ToDictionary(c => c.Id, c => c)
                : new Dictionary<int, ClassFormViewModel>();
            var subjectMap = subjectResponse.Success && subjectResponse.Data != null
                ? subjectResponse.Data.ToDictionary(s => s.Id, s => s)
                : new Dictionary<int, SubjectFormViewModel>();

            var teachingAssignments = new List<TeachingAssignmentItemViewModel>();
            if (assignmentsResponse.Success && assignmentsResponse.Data != null)
            {
                foreach (var item in assignmentsResponse.Data.Where(x => x.LecturerId == lecturerId.Value))
                {
                    classMap.TryGetValue(item.ClassId, out var cls);
                    subjectMap.TryGetValue(item.SubjectId, out var subject);

                    teachingAssignments.Add(new TeachingAssignmentItemViewModel
                    {
                        ClassCode = cls?.ClassCode ?? "",
                        ClassName = cls?.ClassName ?? "",
                        SubjectCode = subject?.SubjectCode ?? "",
                        SubjectName = subject?.SubjectName ?? "",
                        AcademicYear = item.AcademicYear,
                        Semester = item.Semester,
                        Status = item.Status
                    });
                }
            }

            var viewModel = new LecturerDetailsViewModel
            {
                Lecturer = lecturerResponse.Data,
                TeachingAssignments = teachingAssignments
            };

            return View(viewModel);
        }

        private async Task<IActionResult> StudentProfile()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            var email = HttpContext.Session.GetString("Email");
            var fullName = HttpContext.Session.GetString("FullName") ?? "Sinh viên";

            StudentViewModel? student = null;

            if (studentId.HasValue)
            {
                var studentResponse = await _apiService.GetAsync<StudentViewModel>($"students/{studentId.Value}");
                if (studentResponse.Success && studentResponse.Data != null)
                {
                    student = studentResponse.Data;
                }
            }

            if (student == null && !string.IsNullOrWhiteSpace(email))
            {
                var studentsResponse = await _apiService.GetAsync<List<StudentViewModel>>(
                    "students",
                    new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });

                if (studentsResponse.Success && studentsResponse.Data != null)
                {
                    student = studentsResponse.Data.FirstOrDefault(s =>
                        string.Equals(s.Email, email, StringComparison.OrdinalIgnoreCase));
                    if (student != null)
                    {
                        HttpContext.Session.SetInt32("StudentId", student.Id);
                    }
                }
            }

            if (student == null)
            {
                TempData["ErrorMessage"] = "KhÃ´ng tÃ¬m tháº¥y sinh viÃªn cho tÃ i khoáº£n nÃ y.";
                return View("Student", new StudentProfileViewModel
                {
                    Student = new StudentViewModel { FullName = fullName }
                });
            }

            ClassFormViewModel? cls = null;
            var classResponse = await _apiService.GetAsync<List<ClassFormViewModel>>(
                "classes",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            if (classResponse.Success && classResponse.Data != null)
            {
                cls = classResponse.Data.FirstOrDefault(c => c.Id == student.ClassId);
            }

            var viewModel = new StudentProfileViewModel
            {
                Student = student,
                Class = cls
            };

            return View("Student", viewModel);
        }
    }
}
