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
    }
}
