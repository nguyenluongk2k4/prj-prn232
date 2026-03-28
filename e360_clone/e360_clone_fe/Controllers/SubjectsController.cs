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

            var assignments = await _apiService.GetAsync<List<TeachingAssignmentViewModel>>(
                "teaching-assignments",
                new Dictionary<string, string>
                {
                    { "pageNumber", "1" },
                    { "pageSize", "1000" },
                    { "subjectId", id.ToString() }
                });

            var classesResponse = await _apiService.GetAsync<List<ClassViewModel>>("classes",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "1000" } });

            var assignedClassIds = assignments.Success && assignments.Data != null
                ? assignments.Data.Where(x => x.Status == "Active").Select(x => x.ClassId).Distinct().ToHashSet()
                : new HashSet<int>();

            var allClasses = classesResponse.Success && classesResponse.Data != null
                ? classesResponse.Data
                : new List<ClassViewModel>();

            var assignedClasses = allClasses.Where(c => assignedClassIds.Contains(c.Id)).ToList();
            var availableClasses = allClasses.Where(c => !assignedClassIds.Contains(c.Id)).ToList();

            var lecturersResponse = await _apiService.GetAsync<List<LecturerViewModel>>("lecturers",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "1000" } });
            var lecturers = lecturersResponse.Success && lecturersResponse.Data != null
                ? lecturersResponse.Data
                : new List<LecturerViewModel>();

            var majorsResponse = await _apiService.GetAsync<List<MajorViewModel>>("majors",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "1000" } });
            var majors = majorsResponse.Success && majorsResponse.Data != null
                ? majorsResponse.Data
                : new List<MajorViewModel>();

            var viewModel = new SubjectDetailsViewModel
            {
                Subject = response.Data,
                AssignedClasses = assignedClasses,
                AvailableClasses = availableClasses,
                Lecturers = lecturers,
                Majors = majors,
                AddRequest = new AddStudentToSubjectViewModel { SubjectId = id },
                AddAssignment = new AddTeachingAssignmentViewModel { SubjectId = id }
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(SubjectDetailsViewModel model)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            if (model.AddRequest.SubjectId <= 0 || model.AddRequest.ClassId <= 0 || model.AddRequest.StudentIds == null || model.AddRequest.StudentIds.Count == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn lớp và sinh viên.";
                return RedirectToAction(nameof(Details), new { id = model.AddRequest.SubjectId });
            }

            var response = await _apiService.PostAsync<object>("student-subjects", model.AddRequest);
            TempData[response.Success ? "SuccessMessage" : "ErrorMessage"] =
                response.Success ? response.Message : response.Message;

            return RedirectToAction(nameof(Details), new { id = model.AddRequest.SubjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddClass(SubjectDetailsViewModel model)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            if (model.AddAssignment.SubjectId <= 0 || model.AddAssignment.ClassId <= 0 || model.AddAssignment.LecturerId <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn lớp và giảng viên.";
                return RedirectToAction(nameof(Details), new { id = model.AddAssignment.SubjectId });
            }

            var payload = new TeachingAssignmentViewModel
            {
                SubjectId = model.AddAssignment.SubjectId,
                ClassId = model.AddAssignment.ClassId,
                LecturerId = model.AddAssignment.LecturerId,
                Status = "Active",
                AcademicYear = string.Empty,
                Semester = string.Empty
            };

            var response = await _apiService.PostAsync<object>("teaching-assignments", payload);
            TempData[response.Success ? "SuccessMessage" : "ErrorMessage"] =
                response.Success ? "Gán lớp vào môn thành công" : response.Message;

            return RedirectToAction(nameof(Details), new { id = model.AddAssignment.SubjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClassInline(SubjectDetailsViewModel model)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            if (string.IsNullOrWhiteSpace(model.NewClass.ClassCode) ||
                string.IsNullOrWhiteSpace(model.NewClass.ClassName) ||
                model.NewClass.MajorId <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng nhập mã lớp, tên lớp và chọn ngành.";
                return RedirectToAction(nameof(Details), new { id = model.Subject.Id });
            }

            var payload = new ClassFormViewModel
            {
                ClassCode = model.NewClass.ClassCode.Trim(),
                ClassName = model.NewClass.ClassName.Trim(),
                MajorId = model.NewClass.MajorId,
                CourseId = 0,
                AcademicYear = string.Empty,
                Cohort = 0,
                CohortYear = 0,
                Semester = 0,
                StudentCount = 0,
                Status = string.IsNullOrWhiteSpace(model.NewClass.Status) ? "Active" : model.NewClass.Status
            };

            var response = await _apiService.PostAsync<object>("classes", payload);
            TempData[response.Success ? "SuccessMessage" : "ErrorMessage"] =
                response.Success ? "Tạo lớp mới thành công" : response.Message;

            return RedirectToAction(nameof(Details), new { id = model.Subject.Id });
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



