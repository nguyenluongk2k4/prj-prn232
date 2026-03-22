using e360_clone_fe.Models.ViewModels;
using e360_clone_fe.Services;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone_fe.Controllers
{
    public class SubjectsController : BaseController
    {
        private const string ApiEndpoint = "subjects";

        public SubjectsController(IApiService apiService, ILogger<SubjectsController> logger)
            : base(apiService, logger)
        {
        }

        public async Task<IActionResult> Index(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? status = null,
            string? subjectType = null,
            string? department = null)
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

            if (!string.IsNullOrEmpty(status))
            {
                queryParams["status"] = status;
            }

            if (!string.IsNullOrEmpty(subjectType))
            {
                queryParams["subjectType"] = subjectType;
            }

            if (!string.IsNullOrEmpty(department))
            {
                queryParams["department"] = department;
            }

            var response = await _apiService.GetAsync<List<SubjectFormViewModel>>(ApiEndpoint, queryParams);
            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Message;
                return View(new PagedViewModel<SubjectFormViewModel>());
            }

            var pagedModel = new PagedViewModel<SubjectFormViewModel>
            {
                Items = response.Data ?? new List<SubjectFormViewModel>(),
                PageNumber = response.PageNumber,
                PageSize = response.PageSize,
                TotalRecords = response.TotalRecords,
                SearchTerm = searchTerm
            };

            ViewBag.SelectedStatus = status;
            ViewBag.SelectedSubjectType = subjectType;
            ViewBag.SelectedDepartment = department;

            return View(pagedModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var response = await _apiService.GetAsync<SubjectFormViewModel>($"{ApiEndpoint}/{id}");
            if (!response.Success || response.Data == null)
            {
                TempData["ErrorMessage"] = response.Message ?? "Không tìm th?y môn h?c";
                return RedirectToAction(nameof(Index));
            }

            return View(response.Data);
        }

        public IActionResult Create()
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            return View(new SubjectFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubjectFormViewModel model)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var response = await _apiService.PostAsync<SubjectFormViewModel>(ApiEndpoint, model);
            if (response.Success)
            {
                TempData["SuccessMessage"] = "Thêm môn h?c thành công";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = response.Message;
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var response = await _apiService.GetAsync<SubjectFormViewModel>($"{ApiEndpoint}/{id}");
            if (!response.Success || response.Data == null)
            {
                TempData["ErrorMessage"] = response.Message ?? "Không tìm th?y môn h?c";
                return RedirectToAction(nameof(Index));
            }

            return View(response.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubjectFormViewModel model)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var response = await _apiService.PutAsync<SubjectFormViewModel>($"{ApiEndpoint}/{model.Id}", model);
            if (response.Success)
            {
                TempData["SuccessMessage"] = "C?p nh?t môn h?c thành công";
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
                response.Success ? "Xóa môn h?c thành công" : response.Message;

            return RedirectToAction(nameof(Index));
        }
    }
}



