using e360_clone_fe.Models.ViewModels;
using e360_clone_fe.Services;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone_fe.Controllers
{
    public class ProctorAssignmentsController : BaseController
    {
        private const string ApiEndpoint = "proctors";

        public ProctorAssignmentsController(IApiService apiService, ILogger<ProctorAssignmentsController> logger)
            : base(apiService, logger)
        {
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var queryParams = new Dictionary<string, string>
            {
                { "pageNumber", pageNumber.ToString() },
                { "pageSize", pageSize.ToString() }
            };

            var response = await _apiService.GetAsync<List<ProctorAssignmentViewModel>>(ApiEndpoint, queryParams);
            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Message;
                return View(new PagedViewModel<ProctorAssignmentListItemViewModel>());
            }

            var exams = await LoadExamsAsync();
            var lecturers = await LoadLecturersAsync();
            var subjects = await LoadSubjectsAsync();
            var classes = await LoadClassesAsync();
            var rooms = await LoadRoomsAsync();

            var examMap = exams.ToDictionary(e => e.Id, e => e);
            var lecturerMap = lecturers.ToDictionary(l => l.Id, l => l);
            var subjectMap = subjects.ToDictionary(s => s.Id, s => s);
            var classMap = classes.ToDictionary(c => c.Id, c => c);
            var roomMap = rooms.ToDictionary(r => r.Id, r => r);

            var items = (response.Data ?? new List<ProctorAssignmentViewModel>())
                .Select(a =>
                {
                    examMap.TryGetValue(a.ExamId, out var exam);
                    lecturerMap.TryGetValue(a.LecturerId, out var lecturer);
                    subjectMap.TryGetValue(exam?.SubjectId ?? 0, out var subject);
                    classMap.TryGetValue(exam?.ClassId ?? 0, out var cls);
                    roomMap.TryGetValue(exam?.RoomId ?? 0, out var room);

                    return new ProctorAssignmentListItemViewModel
                    {
                        Id = a.Id,
                        ExamId = a.ExamId,
                        LecturerId = a.LecturerId,
                        LecturerName = lecturer?.FullName ?? string.Empty,
                        LecturerCode = lecturer?.EmployeeCode ?? string.Empty,
                        Role = a.Role,
                        Status = a.Status,
                        AssignedAt = a.AssignedAt,
                        ExamDate = exam?.ExamDate ?? DateTime.MinValue,
                        StartTime = exam?.StartTime ?? TimeSpan.Zero,
                        EndTime = exam?.EndTime ?? TimeSpan.Zero,
                        SubjectCode = subject?.SubjectCode ?? string.Empty,
                        SubjectName = subject?.SubjectName ?? string.Empty,
                        ClassCode = cls?.ClassCode ?? string.Empty,
                        RoomCode = room?.RoomCode ?? string.Empty
                    };
                })
                .ToList();

            var model = new PagedViewModel<ProctorAssignmentListItemViewModel>
            {
                Items = items,
                PageNumber = response.PageNumber,
                PageSize = response.PageSize,
                TotalRecords = response.TotalRecords
            };

            return View(model);
        }

        public async Task<IActionResult> Create(int? examId)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var model = new ProctorAssignmentFormViewModel
            {
                Assignment = new ProctorAssignmentViewModel
                {
                    ExamId = examId ?? 0
                }
            };

            await PopulateFormListsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProctorAssignmentFormViewModel model, string? returnUrl)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = Request.Headers.Referer.ToString();
            }

            if (model.Assignment.ExamId <= 0 || model.Assignment.LecturerId <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ca thi và giảng viên.";
                if (!string.IsNullOrWhiteSpace(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                await PopulateFormListsAsync(model);
                return View(model);
            }

            var payload = new
            {
                model.Assignment.ExamId,
                model.Assignment.LecturerId,
                Role = string.IsNullOrWhiteSpace(model.Assignment.Role) ? "Proctor" : model.Assignment.Role,
                Status = string.IsNullOrWhiteSpace(model.Assignment.Status) ? "Assigned" : model.Assignment.Status,
                Notes = model.Assignment.Notes ?? string.Empty
            };

            var response = await _apiService.PostAsync<ProctorAssignmentViewModel>(ApiEndpoint, payload);
            if (response.Success)
            {
                TempData["SuccessMessage"] = "Phân công coi thi thành công";
                if (!string.IsNullOrWhiteSpace(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = response.Message;
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                return Redirect(returnUrl);
            }
            await PopulateFormListsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var response = await _apiService.DeleteAsync<object>($"{ApiEndpoint}/{id}");
            if (response.Success)
            {
                TempData["SuccessMessage"] = "Xóa phân công coi thi thành công";
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateFormListsAsync(ProctorAssignmentFormViewModel model)
        {
            var exams = await LoadExamsAsync();
            var subjects = await LoadSubjectsAsync();
            var classes = await LoadClassesAsync();
            var rooms = await LoadRoomsAsync();

            model.Lecturers = await LoadLecturersAsync();
            model.Exams = BuildExamOptions(exams, subjects, classes, rooms);
        }

        private static List<ExamOptionViewModel> BuildExamOptions(
            List<ExamViewModel> exams,
            List<SubjectFormViewModel> subjects,
            List<ClassFormViewModel> classes,
            List<ExamRoomViewModel> rooms)
        {
            var subjectMap = subjects.ToDictionary(s => s.Id, s => s);
            var classMap = classes.ToDictionary(c => c.Id, c => c);
            var roomMap = rooms.ToDictionary(r => r.Id, r => r);

            return exams
                .OrderBy(e => e.ExamDate)
                .ThenBy(e => e.StartTime)
                .Select(e =>
                {
                    subjectMap.TryGetValue(e.SubjectId, out var subject);
                    classMap.TryGetValue(e.ClassId, out var cls);
                    roomMap.TryGetValue(e.RoomId, out var room);

                    var subjectText = subject != null ? $"{subject.SubjectCode} - {subject.SubjectName}" : "N/A";
                    var classText = cls != null ? cls.ClassCode : "N/A";
                    var roomText = room != null ? room.RoomCode : "N/A";

                    return new ExamOptionViewModel
                    {
                        Id = e.Id,
                        ExamDate = e.ExamDate,
                        StartTime = e.StartTime,
                        EndTime = e.EndTime,
                        DisplayName = $"{e.ExamDate:dd/MM/yyyy} {e.StartTime:hh\\:mm}-{e.EndTime:hh\\:mm} | {subjectText} | {classText} | {roomText}"
                    };
                })
                .ToList();
        }

        private async Task<List<ExamViewModel>> LoadExamsAsync()
        {
            var response = await _apiService.GetAsync<List<ExamViewModel>>("exams",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            return response.Success && response.Data != null ? response.Data : new List<ExamViewModel>();
        }

        private async Task<List<LecturerViewModel>> LoadLecturersAsync()
        {
            var response = await _apiService.GetAsync<List<LecturerViewModel>>("lecturers",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            return response.Success && response.Data != null ? response.Data : new List<LecturerViewModel>();
        }

        private async Task<List<SubjectFormViewModel>> LoadSubjectsAsync()
        {
            var response = await _apiService.GetAsync<List<SubjectFormViewModel>>("subjects",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            return response.Success && response.Data != null ? response.Data : new List<SubjectFormViewModel>();
        }

        private async Task<List<ClassFormViewModel>> LoadClassesAsync()
        {
            var response = await _apiService.GetAsync<List<ClassFormViewModel>>("classes",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            return response.Success && response.Data != null ? response.Data : new List<ClassFormViewModel>();
        }

        private async Task<List<ExamRoomViewModel>> LoadRoomsAsync()
        {
            var response = await _apiService.GetAsync<List<ExamRoomViewModel>>("rooms",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            return response.Success && response.Data != null ? response.Data : new List<ExamRoomViewModel>();
        }
    }
}
