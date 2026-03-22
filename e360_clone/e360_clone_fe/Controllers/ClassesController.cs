using Microsoft.AspNetCore.Mvc;
using e360_clone_fe.Services;
using e360_clone_fe.Models.ViewModels;

namespace e360_clone_fe.Controllers
{
    public class ClassesController : BaseController
    {
        private const string ApiEndpoint = "classes";

        public ClassesController(IApiService apiService, ILogger<ClassesController> logger)
            : base(apiService, logger)
        {
        }

        public async Task<IActionResult> Index(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? majorCode = null,
            int? cohort = null,
            string? status = null)
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

            if (!string.IsNullOrEmpty(majorCode))
            {
                queryParams["majorCode"] = majorCode;
            }

            if (cohort.HasValue)
            {
                queryParams["cohort"] = cohort.Value.ToString();
            }

            if (!string.IsNullOrEmpty(status))
            {
                queryParams["status"] = status;
            }

            var response = await _apiService.GetAsync<List<ClassFormViewModel>>(ApiEndpoint, queryParams);
            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Message;
                return View(new PagedViewModel<ClassFormViewModel>());
            }

            var majors = await _apiService.GetAsync<List<MajorViewModel>>("majors",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "1000" } });
            var majorMap = majors.Success && majors.Data != null
                ? majors.Data.ToDictionary(m => m.Id, m => $"{m.MajorCode} - {m.MajorName}")
                : new Dictionary<int, string>();
            ViewBag.MajorMap = majorMap;
            ViewBag.Majors = majors.Success && majors.Data != null ? majors.Data : new List<MajorViewModel>();
            ViewBag.SelectedMajorCode = majorCode;
            ViewBag.SelectedCohort = cohort;
            ViewBag.SelectedStatus = status;

            var pagedModel = new PagedViewModel<ClassFormViewModel>
            {
                Items = response.Data ?? new List<ClassFormViewModel>(),
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

            var response = await _apiService.GetAsync<ClassFormViewModel>($"{ApiEndpoint}/{id}");
            if (!response.Success || response.Data == null)
            {
                TempData["ErrorMessage"] = response.Message ?? "KhÃ´ng tÃ¬m tháº¥y lá»›p";
                return RedirectToAction(nameof(Index));
            }

            var majors = await _apiService.GetAsync<List<MajorViewModel>>("majors",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "1000" } });
            if (majors.Success && majors.Data != null)
            {
                var major = majors.Data.FirstOrDefault(m => m.Id == response.Data.MajorId);
                if (major != null)
                {
                    ViewBag.MajorName = $"{major.MajorCode} - {major.MajorName}";
                }
            }

            return View(response.Data);
        }

        public async Task<IActionResult> Create()
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var form = await BuildClassFormAsync();
            return View(form);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassFormPageViewModel model)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var response = await _apiService.PostAsync<ClassFormViewModel>(ApiEndpoint, model.Class);
            if (response.Success)
            {
                TempData["SuccessMessage"] = "ThÃªm lá»›p thÃ nh cÃ´ng";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = response.Message;
            var reload = await BuildClassFormAsync(model.Class);
            return View(reload);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var response = await _apiService.GetAsync<ClassFormViewModel>($"{ApiEndpoint}/{id}");
            if (!response.Success || response.Data == null)
            {
                TempData["ErrorMessage"] = response.Message ?? "KhÃ´ng tÃ¬m tháº¥y lá»›p";
                return RedirectToAction(nameof(Index));
            }

            var form = await BuildClassFormAsync(response.Data);
            return View(form);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClassFormPageViewModel model)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var response = await _apiService.PutAsync<ClassFormViewModel>($"{ApiEndpoint}/{model.Class.Id}", model.Class);
            if (response.Success)
            {
                TempData["SuccessMessage"] = "Cáº­p nháº­t lá»›p thÃ nh cÃ´ng";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = response.Message;
            var reload = await BuildClassFormAsync(model.Class);
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
                response.Success ? "XÃ³a lá»›p thÃ nh cÃ´ng" : response.Message;

            return RedirectToAction(nameof(Index));
        }

        private async Task<ClassFormPageViewModel> BuildClassFormAsync(ClassFormViewModel? cls = null)
        {
            var form = new ClassFormPageViewModel
            {
                Class = cls ?? new ClassFormViewModel()
            };

            var majors = await _apiService.GetAsync<List<MajorViewModel>>("majors",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "1000" } });
            if (majors.Success && majors.Data != null)
            {
                form.Majors = majors.Data;
            }

            return form;
        }
    }
}


