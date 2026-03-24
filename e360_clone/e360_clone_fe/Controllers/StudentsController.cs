using Microsoft.AspNetCore.Mvc;
using e360_clone_fe.Services;
using e360_clone_fe.Models.ViewModels;
using e360_clone.BusinessObjects;

namespace e360_clone_fe.Controllers
{
    /// <summary>
    /// Controller for Student Management
    /// Server-side calls to backend API
    /// </summary>
    public class StudentsController : BaseController
    {
        private const string ApiEndpoint = "students";

        public StudentsController(IApiService apiService, ILogger<StudentsController> logger)
            : base(apiService, logger)
        {
        }

        /// <summary>
        /// Get all students with pagination and search
        /// </summary>
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, int? classId = null, int? subjectId = null)
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

            if (classId.HasValue)
            {
                queryParams["classId"] = classId.Value.ToString();
            }

            if (subjectId.HasValue)
            {
                queryParams["subjectId"] = subjectId.Value.ToString();
            }

            var endpoint = subjectId.HasValue ? "students/by-subject" : ApiEndpoint;
            var response = await _apiService.GetAsync<List<StudentViewModel>>(endpoint, queryParams);

            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Message;
                return View(new PagedViewModel<StudentViewModel>());
            }

            var pagedModel = new PagedViewModel<StudentViewModel>
            {
                Items = response.Data ?? new List<StudentViewModel>(),
                PageNumber = response.PageNumber,
                PageSize = response.PageSize,
                TotalRecords = response.TotalRecords,
                SearchTerm = searchTerm
            };

            ViewBag.ClassId = classId;
            ViewBag.SubjectId = subjectId;
            return View(pagedModel);
        }

        /// <summary>
        /// Get student details by ID
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var response = await _apiService.GetAsync<StudentViewModel>($"{ApiEndpoint}/{id}");

            if (!response.Success || response.Data == null)
            {
                TempData["ErrorMessage"] = response.Message ?? "Không tìm thấy sinh viên";
                return RedirectToAction(nameof(Index));
            }

            return View(response.Data);
        }

        /// <summary>
        /// Show create student form
        /// </summary>
        public IActionResult Create()
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            return View(new CreateStudentViewModel());
        }

        /// <summary>
        /// Create new student
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStudentViewModel model)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var studentData = new
            {
                model.StudentCode,
                model.FullName,
                model.DateOfBirth,
                model.Gender,
                model.Email,
                model.PhoneNumber,
                model.Address,
                model.ClassId,
                model.Status
            };

            var response = await _apiService.PostAsync<StudentViewModel>(ApiEndpoint, studentData);

            if (response.Success)
            {
                TempData["SuccessMessage"] = "Thêm sinh viên thành công";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = response.Message;
            return View(model);
        }

        /// <summary>
        /// Show edit student form
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var response = await _apiService.GetAsync<StudentViewModel>($"{ApiEndpoint}/{id}");

            if (!response.Success || response.Data == null)
            {
                TempData["ErrorMessage"] = response.Message ?? "Không tìm thấy sinh viên";
                return RedirectToAction(nameof(Index));
            }

            var model = new UpdateStudentViewModel
            {
                Id = response.Data.Id,
                StudentCode = response.Data.StudentCode,
                FullName = response.Data.FullName,
                DateOfBirth = response.Data.DateOfBirth,
                Gender = response.Data.Gender,
                Email = response.Data.Email,
                PhoneNumber = response.Data.PhoneNumber,
                Address = response.Data.Address,
                ClassId = response.Data.ClassId,
                Status = response.Data.Status
            };

            return View(model);
        }

        /// <summary>
        /// Update student
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateStudentViewModel model)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var studentData = new
            {
                model.Id,
                model.StudentCode,
                model.FullName,
                model.DateOfBirth,
                model.Gender,
                model.Email,
                model.PhoneNumber,
                model.Address,
                model.ClassId,
                model.Status
            };

            var response = await _apiService.PutAsync<StudentViewModel>($"{ApiEndpoint}/{model.Id}", studentData);

            if (response.Success)
            {
                TempData["SuccessMessage"] = "Cập nhật sinh viên thành công";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = response.Message;
            return View(model);
        }

        /// <summary>
        /// Delete student
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var response = await _apiService.DeleteAsync<object>($"{ApiEndpoint}/{id}");

            if (response.Success)
            {
                TempData["SuccessMessage"] = "Xóa sinh viên thành công";
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Get student by ID for AJAX calls (optional, for partial updates)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStudent(int id)
        {
            var response = await _apiService.GetAsync<StudentViewModel>($"{ApiEndpoint}/{id}");
            return response.Success && response.Data != null
                ? Ok(response.Data)
                : NotFound(response.Message);
        }
    }

}
