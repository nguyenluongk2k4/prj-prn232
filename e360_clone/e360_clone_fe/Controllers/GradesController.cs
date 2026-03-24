using e360_clone_fe.Models.ViewModels;
using e360_clone_fe.Services;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone_fe.Controllers
{
    public class GradesController : BaseController
    {
        public GradesController(IApiService apiService, ILogger<GradesController> logger)
            : base(apiService, logger)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Enter()
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var model = await BuildImportModelAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enter(int examId, string scoreType, IFormFile? file)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            if (examId <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ca thi.";
                var model = await BuildImportModelAsync();
                return View(model);
            }

            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file điểm.";
                var model = await BuildImportModelAsync();
                return View(model);
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId");
            var endpoint = $"grades/import?examId={examId}&scoreType={Uri.EscapeDataString(scoreType ?? "Final")}";
            if (lecturerId.HasValue)
            {
                endpoint += $"&enteredBy={lecturerId.Value}";
            }

            var response = await _apiService.UploadAsync<GradeImportResultViewModel>(endpoint, file);
            if (response.Success)
            {
                TempData["SuccessMessage"] = response.Message;
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
            }

            var viewModel = await BuildImportModelAsync();
            viewModel.SelectedExamId = examId;
            viewModel.ScoreType = scoreType;
            viewModel.Result = response.Data;
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Approve()
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var model = await BuildImportModelAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> My(int? subjectId = null)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (!studentId.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sinh viên cho tài khoản này.";
                return View(new MyGradesPageViewModel());
            }

            var response = await _apiService.GetAsync<List<StudentGradeItemViewModel>>(
                "grades/student",
                new Dictionary<string, string> { { "studentId", studentId.Value.ToString() } });

            if (!response.Success || response.Data == null)
            {
                TempData["ErrorMessage"] = response.Message;
                return View(new MyGradesPageViewModel());
            }

            var subjects = response.Data
                .GroupBy(x => x.SubjectId)
                .Select(g =>
                {
                    var first = g.First();
                    return new StudentGradeSubjectViewModel
                    {
                        SubjectId = g.Key,
                        SubjectCode = first.SubjectCode,
                        SubjectName = first.SubjectName,
                        Items = g.OrderByDescending(x => x.ExamDate).ToList()
                    };
                })
                .OrderBy(x => x.SubjectCode)
                .ToList();

            var selectedSubjectId = subjectId ?? subjects.FirstOrDefault()?.SubjectId;
            var selectedSubject = selectedSubjectId.HasValue
                ? subjects.FirstOrDefault(x => x.SubjectId == selectedSubjectId.Value)
                : null;

            if (selectedSubject == null && subjects.Count > 0)
            {
                selectedSubject = subjects[0];
                selectedSubjectId = selectedSubject.SubjectId;
            }

            var model = new MyGradesPageViewModel
            {
                Subjects = subjects,
                SelectedSubjectId = selectedSubjectId,
                SelectedSubject = selectedSubject
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int examId)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            if (examId <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ca thi để công bố.";
                return RedirectToAction(nameof(Approve));
            }

            var response = await _apiService.PostAsync<string>($"grades/publish?examId={examId}", new { });
            if (response.Success)
            {
                TempData["SuccessMessage"] = response.Message;
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
            }

            return RedirectToAction(nameof(Approve));
        }

        private async Task<GradeImportPageViewModel> BuildImportModelAsync()
        {
            var examsResponse = await _apiService.GetAsync<List<ExamViewModel>>(
                "exams",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
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

            var exams = examsResponse.Success && examsResponse.Data != null
                ? examsResponse.Data
                : new List<ExamViewModel>();

            var options = exams
                .OrderByDescending(e => e.ExamDate)
                .Select(e =>
                {
                    subjectMap.TryGetValue(e.SubjectId, out var subject);
                    classMap.TryGetValue(e.ClassId, out var cls);
                    var subjectText = subject != null ? $"{subject.SubjectCode} - {subject.SubjectName}" : $"Subject #{e.SubjectId}";
                    var classText = cls != null ? cls.ClassCode : $"Class #{e.ClassId}";
                    var dateText = e.ExamDate.ToString("dd/MM/yyyy");
                    var timeText = $"{e.StartTime:hh\\:mm}-{e.EndTime:hh\\:mm}";
                    return new GradeExamOptionViewModel
                    {
                        Id = e.Id,
                        Display = $"{dateText} {timeText} | {subjectText} | {classText}"
                    };
                })
                .ToList();

            return new GradeImportPageViewModel
            {
                Exams = options
            };
        }
    }
}
