using Microsoft.AspNetCore.Mvc;
using e360_clone_fe.Services;
using e360_clone_fe.Models.ViewModels;

namespace e360_clone_fe.Controllers
{
    public class ExamsController : BaseController
    {
        private const string ApiEndpoint = "exams";

        public ExamsController(IApiService apiService, ILogger<ExamsController> logger)
            : base(apiService, logger)
        {
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var queryParams = new Dictionary<string, string>
            {
                { "pageNumber", pageNumber.ToString() },
                { "pageSize", pageSize.ToString() }
            };

            if (!string.IsNullOrEmpty(searchTerm))
            {
                queryParams["searchTerm"] = searchTerm;
            }

            var response = await _apiService.GetAsync<List<ExamViewModel>>(ApiEndpoint, queryParams);
            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Message;
                return View(new PagedViewModel<ExamViewModel>());
            }

            var subjectMap = new Dictionary<int, string>();
            var subjects = await _apiService.GetAsync<List<SubjectViewModel>>("subjects",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "1000" } });
            if (subjects.Success && subjects.Data != null)
            {
                subjectMap = subjects.Data.ToDictionary(s => s.Id, s => $"{s.SubjectCode} - {s.SubjectName}");
            }

            var classMap = new Dictionary<int, string>();
            var classes = await _apiService.GetAsync<List<ClassViewModel>>("classes",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "1000" } });
            if (classes.Success && classes.Data != null)
            {
                classMap = classes.Data.ToDictionary(c => c.Id, c => $"{c.ClassCode} - {c.ClassName}");
            }

            var roomMap = new Dictionary<int, string>();
            var rooms = await _apiService.GetAsync<List<ExamRoomViewModel>>("rooms",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "1000" } });
            if (rooms.Success && rooms.Data != null)
            {
                roomMap = rooms.Data.ToDictionary(r => r.Id, r => $"{r.RoomCode} - {r.RoomName}");
            }

            ViewBag.SubjectMap = subjectMap;
            ViewBag.ClassMap = classMap;
            ViewBag.RoomMap = roomMap;

            var pagedModel = new PagedViewModel<ExamViewModel>
            {
                Items = response.Data ?? new List<ExamViewModel>(),
                PageNumber = response.PageNumber,
                PageSize = response.PageSize,
                TotalRecords = response.TotalRecords,
                SearchTerm = searchTerm
            };

            return View(pagedModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var response = await _apiService.GetAsync<ExamViewModel>($"{ApiEndpoint}/{id}");
            if (!response.Success || response.Data == null)
            {
                TempData["ErrorMessage"] = response.Message ?? "Không tìm thấy kỳ thi";
                return RedirectToAction(nameof(Index));
            }

            return View(response.Data);
        }

        public async Task<IActionResult> Create()
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var form = await BuildExamFormAsync();
            return View(form);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamFormViewModel model)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var response = await _apiService.PostAsync<ExamViewModel>(ApiEndpoint, model.Exam);
            if (response.Success)
            {
                TempData["SuccessMessage"] = "Thêm kỳ thi thành công";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = response.Message;
            var reload = await BuildExamFormAsync(model.Exam);
            return View(reload);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var response = await _apiService.GetAsync<ExamViewModel>($"{ApiEndpoint}/{id}");
            if (!response.Success || response.Data == null)
            {
                TempData["ErrorMessage"] = response.Message ?? "Không tìm thấy kỳ thi";
                return RedirectToAction(nameof(Index));
            }

            var form = await BuildExamFormAsync(response.Data);
            return View(form);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ExamFormViewModel model)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var response = await _apiService.PutAsync<ExamViewModel>($"{ApiEndpoint}/{model.Exam.Id}", model.Exam);
            if (response.Success)
            {
                TempData["SuccessMessage"] = "Cập nhật kỳ thi thành công";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = response.Message;
            var reload = await BuildExamFormAsync(model.Exam);
            return View(reload);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var response = await _apiService.DeleteAsync<object>($"{ApiEndpoint}/{id}");
            TempData[response.Success ? "SuccessMessage" : "ErrorMessage"] =
                response.Success ? "Xóa kỳ thi thành công" : response.Message;

            return RedirectToAction(nameof(Index));
        }

        private async Task<ExamFormViewModel> BuildExamFormAsync(ExamViewModel? exam = null)
        {
            var form = new ExamFormViewModel
            {
                Exam = exam ?? new ExamViewModel()
            };

            var subjects = await _apiService.GetAsync<List<SubjectViewModel>>("subjects",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "1000" } });
            if (subjects.Success && subjects.Data != null)
            {
                form.Subjects = subjects.Data;
            }

            var classes = await _apiService.GetAsync<List<ClassViewModel>>("classes",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "1000" } });
            if (classes.Success && classes.Data != null)
            {
                form.Classes = classes.Data;
            }

            var rooms = await _apiService.GetAsync<List<ExamRoomViewModel>>("rooms",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "1000" } });
            if (rooms.Success && rooms.Data != null)
            {
                form.Rooms = rooms.Data;
            }

            return form;
        }
    }
}
