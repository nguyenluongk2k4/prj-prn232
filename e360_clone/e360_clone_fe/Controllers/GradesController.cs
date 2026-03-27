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
            var authResult = RequireRole("Teacher", "Admin", "SuperAdmin", "Staff");
            if (authResult != null) return authResult;

            var role = HttpContext.Session.GetString("Role") ?? string.Empty;
            var model = await BuildImportModelAsync(role);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enter(int subjectId, string academicYear, int semester, string scoreType, IFormFile? file)
        {
            var authResult = RequireRole("Teacher", "Admin", "SuperAdmin", "Staff");
            if (authResult != null) return authResult;

            var role = HttpContext.Session.GetString("Role") ?? string.Empty;
            var model = await BuildImportModelAsync(role);
            model.SelectedSubjectId = subjectId;
            model.AcademicYear = academicYear;
            model.Semester = semester;
            model.ScoreType = scoreType;

            if (subjectId <= 0 || string.IsNullOrWhiteSpace(academicYear) || semester <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn môn và kỳ học.";
                return View(model);
            }

            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file điểm.";
                return View(model);
            }

            var allowedTypes = model.ScoreTypes.Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (!allowedTypes.Contains(scoreType))
            {
                TempData["ErrorMessage"] = "Loại điểm không hợp lệ cho quyền hiện tại.";
                return View(model);
            }

            var enteredBy = HttpContext.Session.GetInt32("UserId");
            var additionalData = new Dictionary<string, string>
            {
                { "subjectId", subjectId.ToString() },
                { "academicYear", academicYear },
                { "semester", semester.ToString() },
                { "scoreType", scoreType },
                { "role", role }
            };
            if (enteredBy.HasValue)
            {
                additionalData["enteredBy"] = enteredBy.Value.ToString();
            }

            var response = await _apiService.UploadAsync<GradeImportResultViewModel>("grades/import", file, additionalData);
            if (response.Success)
            {
                TempData["SuccessMessage"] = response.Message;
                model.Result = response.Data;
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnterManual(int subjectId, string academicYear, int semester, string scoreType, string studentCode, decimal? score, string? letterGrade, string? notes)
        {
            var authResult = RequireRole("Teacher", "Admin", "SuperAdmin", "Staff");
            if (authResult != null) return authResult;

            var role = HttpContext.Session.GetString("Role") ?? string.Empty;
            var model = await BuildImportModelAsync(role);
            model.SelectedSubjectId = subjectId;
            model.AcademicYear = academicYear;
            model.Semester = semester;
            model.ScoreType = scoreType;

            if (subjectId <= 0 || string.IsNullOrWhiteSpace(academicYear) || semester <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn môn và kỳ học.";
                return View("Enter", model);
            }

            if (string.IsNullOrWhiteSpace(studentCode))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập mã sinh viên.";
                return View("Enter", model);
            }

            var allowedTypes = model.ScoreTypes.Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (!allowedTypes.Contains(scoreType))
            {
                TempData["ErrorMessage"] = "Loại điểm không hợp lệ cho quyền hiện tại.";
                return View("Enter", model);
            }

            var enteredBy = HttpContext.Session.GetInt32("UserId");
            var payload = new
            {
                SubjectId = subjectId,
                AcademicYear = academicYear,
                Semester = semester,
                ScoreType = scoreType,
                StudentCode = studentCode.Trim(),
                Score = score,
                LetterGrade = letterGrade ?? string.Empty,
                Notes = notes ?? string.Empty,
                EnteredBy = enteredBy,
                Role = role
            };

            var response = await _apiService.PostAsync<string>("grades/manual", payload);
            if (response.Success)
            {
                TempData["SuccessMessage"] = response.Message;
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
            }

            return View("Enter", model);
        }

        [HttpGet]
        public IActionResult Template()
        {
            var authResult = RequireRole("Teacher", "Admin", "SuperAdmin", "Staff");
            if (authResult != null) return authResult;

            var csv = "StudentCode,Score,LetterGrade,Notes\nHE180001,8.5,A,Good\n";
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", "grade_template.csv");
        }

        [HttpGet]
        public async Task<IActionResult> Approve()
        {
            var authResult = RequireRole("Admin", "SuperAdmin", "Staff");
            if (authResult != null) return authResult;

            var model = await BuildImportModelAsync("Admin");
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
                        Items = g.OrderByDescending(x => x.AcademicYear)
                            .ThenByDescending(x => x.Semester)
                            .ToList()
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

        [HttpGet]
        public async Task<IActionResult> Transcript(string? academicYear = null, int? semester = null)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (!studentId.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sinh viên cho tài khoản này.";
                return View(new TranscriptPageViewModel());
            }

            var response = await _apiService.GetAsync<List<TranscriptTermViewModel>>(
                "grades/transcript",
                new Dictionary<string, string> { { "studentId", studentId.Value.ToString() } });

            if (!response.Success || response.Data == null)
            {
                TempData["ErrorMessage"] = response.Message;
                return View(new TranscriptPageViewModel());
            }

            var terms = response.Data;
            TranscriptTermViewModel? selected = null;

            if (!string.IsNullOrWhiteSpace(academicYear) && semester.HasValue)
            {
                selected = terms.FirstOrDefault(t =>
                    t.AcademicYear == academicYear && t.Semester == semester.Value);
            }

            if (selected == null && terms.Count > 0)
            {
                selected = terms[0];
                academicYear = selected.AcademicYear;
                semester = selected.Semester;
            }

            var model = new TranscriptPageViewModel
            {
                Terms = terms,
                SelectedAcademicYear = academicYear,
                SelectedSemester = semester,
                SelectedTerm = selected
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int subjectId, string academicYear, int semester)
        {
            var authResult = RequireRole("Admin", "SuperAdmin", "Staff");
            if (authResult != null) return authResult;

            if (subjectId <= 0 || string.IsNullOrWhiteSpace(academicYear) || semester <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn môn và kỳ học để công bố.";
                return RedirectToAction(nameof(Approve));
            }

            var response = await _apiService.PostAsync<string>(
                $"grades/publish?subjectId={subjectId}&academicYear={Uri.EscapeDataString(academicYear)}&semester={semester}",
                new { });
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

        private async Task<GradeImportPageViewModel> BuildImportModelAsync(string role)
        {
            var subjectsResponse = await _apiService.GetAsync<List<SubjectFormViewModel>>(
                "subjects",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var scoreTypes = ResolveScoreTypes(role);
            var subjects = subjectsResponse.Success && subjectsResponse.Data != null
                ? subjectsResponse.Data.OrderBy(s => s.SubjectCode).ToList()
                : new List<SubjectFormViewModel>();

            return new GradeImportPageViewModel
            {
                Subjects = subjects,
                ScoreTypes = scoreTypes,
                ScoreType = scoreTypes.FirstOrDefault()?.Value ?? "Final",
                AcademicYear = DateTime.Today.Year.ToString(),
                Semester = 1
            };
        }

        private static List<GradeScoreTypeOptionViewModel> ResolveScoreTypes(string role)
        {
            if (string.Equals(role, "Teacher", StringComparison.OrdinalIgnoreCase))
            {
                return new List<GradeScoreTypeOptionViewModel>
                {
                    new() { Value = "ProgressTest", Label = "Kiểm tra quá trình" },
                    new() { Value = "Assignment", Label = "Assignment" },
                    new() { Value = "Lab", Label = "Lab" }
                };
            }

            return new List<GradeScoreTypeOptionViewModel>
            {
                new() { Value = "Practical", Label = "Thực hành" },
                new() { Value = "Final", Label = "Cuối kỳ" },
                new() { Value = "PracticalRetake", Label = "Thi lại thực hành" },
                new() { Value = "FinalRetake", Label = "Thi lại cuối kỳ" }
            };
        }
    }
}
