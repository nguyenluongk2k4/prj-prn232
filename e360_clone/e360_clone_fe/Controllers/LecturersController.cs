using e360_clone_fe.Models.ViewModels;
using e360_clone_fe.Services;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone_fe.Controllers
{
    public class LecturersController : BaseController
    {
        private const string ApiEndpoint = "lecturers";

        public LecturersController(IApiService apiService, ILogger<LecturersController> logger)
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

            var response = await _apiService.GetAsync<List<LecturerViewModel>>(ApiEndpoint, queryParams);
            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Message;
                return View(new PagedViewModel<LecturerViewModel>());
            }

            var pagedModel = new PagedViewModel<LecturerViewModel>
            {
                Items = response.Data ?? new List<LecturerViewModel>(),
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

            var lecturerResponse = await _apiService.GetAsync<LecturerViewModel>($"{ApiEndpoint}/{id}");
            if (!lecturerResponse.Success || lecturerResponse.Data == null)
            {
                TempData["ErrorMessage"] = lecturerResponse.Message ?? "Không tìm thấy giảng viên";
                return RedirectToAction(nameof(Index));
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
                foreach (var item in assignmentsResponse.Data.Where(x => x.LecturerId == id))
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

        public IActionResult Create()
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            return View(new LecturerViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LecturerViewModel model)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var response = await _apiService.PostAsync<LecturerViewModel>(ApiEndpoint, model);
            if (response.Success)
            {
                TempData["SuccessMessage"] = "Thêm giảng viên thành công";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = response.Message;
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var response = await _apiService.GetAsync<LecturerViewModel>($"{ApiEndpoint}/{id}");
            if (!response.Success || response.Data == null)
            {
                TempData["ErrorMessage"] = response.Message ?? "Không tìm thấy giảng viên";
                return RedirectToAction(nameof(Index));
            }

            return View(response.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LecturerViewModel model)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var response = await _apiService.PutAsync<LecturerViewModel>($"{ApiEndpoint}/{model.Id}", model);
            if (response.Success)
            {
                TempData["SuccessMessage"] = "Cập nhật giảng viên thành công";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = response.Message;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var response = await _apiService.DeleteAsync<object>($"{ApiEndpoint}/{id}");
            TempData[response.Success ? "SuccessMessage" : "ErrorMessage"] =
                response.Success ? "Xóa giảng viên thành công" : response.Message;

            return RedirectToAction(nameof(Index));
        }
    }
}
