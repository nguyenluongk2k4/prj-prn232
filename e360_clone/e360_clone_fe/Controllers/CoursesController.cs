using e360_clone_fe.Models.ViewModels;
using e360_clone_fe.Services;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone_fe.Controllers
{
    public class CoursesController : BaseController
    {
        public CoursesController(IApiService apiService, ILogger<CoursesController> logger)
            : base(apiService, logger)
        {
        }

        public async Task<IActionResult> My()
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId.HasValue)
            {
                return await RenderStudentCoursesAsync();
            }

            var email = HttpContext.Session.GetString("Email");
            if (!string.IsNullOrWhiteSpace(email))
            {
                var studentsResponse = await _apiService.GetAsync<List<StudentViewModel>>(
                    "students",
                    new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });

                if (studentsResponse.Success && studentsResponse.Data != null)
                {
                    var match = studentsResponse.Data.FirstOrDefault(s =>
                        string.Equals(s.Email, email, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        HttpContext.Session.SetInt32("StudentId", match.Id);
                        return await RenderStudentCoursesAsync();
                    }
                }
            }

            var role = GetCurrentUserRole() ?? string.Empty;
            if (role.Contains("student", StringComparison.OrdinalIgnoreCase))
            {
                return await RenderStudentCoursesAsync();
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId");
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
                return View(new MyCoursesPageViewModel { LecturerName = fullName });
            }

            var assignmentsResponse = await _apiService.GetAsync<List<TeachingAssignmentViewModel>>(
                "teaching-assignments",
                new Dictionary<string, string>
                {
                    { "pageNumber", "1" },
                    { "pageSize", "2000" },
                    { "lecturerId", lecturerId.Value.ToString() }
                });

            var assignments = assignmentsResponse.Success && assignmentsResponse.Data != null
                ? assignmentsResponse.Data
                : new List<TeachingAssignmentViewModel>();

            var subjectsResponse = await _apiService.GetAsync<List<SubjectFormViewModel>>(
                "subjects",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var classesResponse = await _apiService.GetAsync<List<ClassFormViewModel>>(
                "classes",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });

            var subjectMap = subjectsResponse.Success && subjectsResponse.Data != null
                ? subjectsResponse.Data.ToDictionary(s => s.Id, s => s)
                : new Dictionary<int, SubjectFormViewModel>();
            var classMap = classesResponse.Success && classesResponse.Data != null
                ? classesResponse.Data.ToDictionary(c => c.Id, c => c)
                : new Dictionary<int, ClassFormViewModel>();

            var courses = assignments
                .Select(a =>
                {
                    subjectMap.TryGetValue(a.SubjectId, out var subject);
                    classMap.TryGetValue(a.ClassId, out var cls);

                    return new MyCourseItemViewModel
                    {
                        SubjectId = a.SubjectId,
                        SubjectCode = subject?.SubjectCode ?? string.Empty,
                        SubjectName = subject?.SubjectName ?? string.Empty,
                        AcademicYear = a.AcademicYear,
                        Semester = a.Semester,
                        ClassId = a.ClassId,
                        ClassCode = cls?.ClassCode ?? $"Class #{a.ClassId}"
                    };
                })
                .OrderBy(x => x.SubjectCode)
                .ThenBy(x => x.ClassCode)
                .ToList();

            var model = new MyCoursesPageViewModel
            {
                LecturerName = fullName,
                Courses = courses
            };

            return View(model);
        }

        private async Task<IActionResult> RenderStudentCoursesAsync()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            var email = HttpContext.Session.GetString("Email");
            var fullName = HttpContext.Session.GetString("FullName") ?? "Sinh viên";

            if (!studentId.HasValue && !string.IsNullOrWhiteSpace(email))
            {
                var studentsResponse = await _apiService.GetAsync<List<StudentViewModel>>(
                    "students",
                    new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });

                if (studentsResponse.Success && studentsResponse.Data != null)
                {
                    var match = studentsResponse.Data.FirstOrDefault(s =>
                        string.Equals(s.Email, email, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        studentId = match.Id;
                        HttpContext.Session.SetInt32("StudentId", match.Id);
                        fullName = match.FullName;
                    }
                }
            }

            var model = new MyCoursesPageViewModel
            {
                StudentName = fullName
            };

            if (!studentId.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sinh viên cho tài khoản này.";
                return View("MyStudent", model);
            }

            var studentResponse = await _apiService.GetAsync<StudentViewModel>($"students/{studentId.Value}");
            if (studentResponse.Success && studentResponse.Data != null)
            {
                model.StudentName = studentResponse.Data.FullName;
                model.StudentClassId = studentResponse.Data.ClassId;
            }

            if (model.StudentClassId > 0)
            {
                var classResponse = await _apiService.GetAsync<ClassFormViewModel>($"classes/{model.StudentClassId}");
                if (classResponse.Success && classResponse.Data != null)
                {
                    model.StudentClassCode = classResponse.Data.ClassCode;
                }
            }

            var enrollmentsResponse = await _apiService.GetAsync<List<StudentSubjectViewModel>>(
                "student-subjects",
                new Dictionary<string, string> { { "studentId", studentId.Value.ToString() } });

            var enrollments = enrollmentsResponse.Success && enrollmentsResponse.Data != null
                ? enrollmentsResponse.Data
                : new List<StudentSubjectViewModel>();

            var subjectsResponse = await _apiService.GetAsync<List<SubjectFormViewModel>>(
                "subjects",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var classesResponse = await _apiService.GetAsync<List<ClassFormViewModel>>(
                "classes",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });

            var subjectMap = subjectsResponse.Success && subjectsResponse.Data != null
                ? subjectsResponse.Data.ToDictionary(s => s.Id, s => s)
                : new Dictionary<int, SubjectFormViewModel>();
            var classMap = classesResponse.Success && classesResponse.Data != null
                ? classesResponse.Data.ToDictionary(c => c.Id, c => c)
                : new Dictionary<int, ClassFormViewModel>();

            model.Courses = enrollments
                .Select(e =>
                {
                    subjectMap.TryGetValue(e.SubjectId, out var subject);
                    var classId = e.ClassId ?? 0;
                    classMap.TryGetValue(classId, out var cls);

                    return new MyCourseItemViewModel
                    {
                        SubjectId = e.SubjectId,
                        SubjectCode = subject?.SubjectCode ?? string.Empty,
                        SubjectName = subject?.SubjectName ?? string.Empty,
                        AcademicYear = e.AcademicYear,
                        Semester = e.Semester.ToString(),
                        ClassId = classId > 0 ? classId : model.StudentClassId,
                        ClassCode = cls?.ClassCode ?? model.StudentClassCode
                    };
                })
                .OrderBy(x => x.SubjectCode)
                .ThenBy(x => x.ClassCode)
                .ToList();

            return View("MyStudent", model);
        }
    }
}
