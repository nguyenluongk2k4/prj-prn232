using e360_clone_fe.Models.ViewModels;
using e360_clone_fe.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone_fe.Controllers
{
    public class AttendanceController : BaseController
    {
        private const string AttendanceEndpoint = "attendances";

        public AttendanceController(IApiService apiService, ILogger<AttendanceController> logger)
            : base(apiService, logger)
        {
        }

        public async Task<IActionResult> Index(
            DateTime? date,
            int? subjectId,
            string? startTime,
            string? endTime,
            int? examId,
            string? searchTerm)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var selectedDate = (date ?? DateTime.Today).Date;
            var selectedStart = ParseTime(startTime);
            var selectedEnd = ParseTime(endTime);

            var exams = await LoadExamsAsync();
            var subjects = await LoadSubjectsAsync();
            var classes = await LoadClassesAsync();

            var slots = BuildSlots(exams, selectedDate, subjectId);
            var examOptions = BuildExamOptions(exams, subjects, classes, selectedDate, subjectId, selectedStart, selectedEnd);

            var roster = new List<AttendanceRosterItemViewModel>();
            if (examId.HasValue && examId.Value > 0)
            {
                var rosterResponse = await _apiService.GetAsync<List<AttendanceRosterItemViewModel>>(
                    $"{AttendanceEndpoint}/roster",
                    new Dictionary<string, string> { { "examId", examId.Value.ToString() } });

                if (rosterResponse.Success && rosterResponse.Data != null)
                {
                    roster = rosterResponse.Data;
                }
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                roster = roster.Where(r =>
                        r.StudentCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        r.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        r.ClassCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var model = new AttendancePageViewModel
            {
                Filter = new AttendanceFilterViewModel
                {
                    SelectedDate = selectedDate,
                    SubjectId = subjectId,
                    StartTime = selectedStart,
                    EndTime = selectedEnd,
                    ExamId = examId,
                    SearchTerm = searchTerm
                },
                Subjects = subjects,
                Slots = slots,
                Exams = examOptions,
                Roster = roster
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ExamOptions(
            DateTime? date,
            int? subjectId,
            string? startTime,
            string? endTime)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var selectedDate = (date ?? DateTime.Today).Date;
            var selectedStart = ParseTime(startTime);
            var selectedEnd = ParseTime(endTime);

            var exams = await LoadExamsAsync();
            var subjects = await LoadSubjectsAsync();
            var classes = await LoadClassesAsync();

            var examOptions = BuildExamOptions(exams, subjects, classes, selectedDate, subjectId, selectedStart, selectedEnd);
            return Ok(examOptions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            int attendanceId,
            int examId,
            int studentId,
            string status,
            bool studentConfirmed,
            string? notes,
            DateTime? checkInTime,
            DateTime? checkOutTime,
            string? violation,
            string? returnUrl)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var payload = new
            {
                Id = attendanceId,
                ExamId = examId,
                StudentId = studentId,
                Status = status,
                CheckInTime = checkInTime,
                CheckOutTime = checkOutTime,
                Notes = notes ?? string.Empty,
                Violation = violation ?? string.Empty,
                StudentConfirmed = studentConfirmed,
                StudentConfirmedAt = studentConfirmed ? DateTime.UtcNow : (DateTime?)null
            };

            var response = await _apiService.PutAsync<object>($"{AttendanceEndpoint}/{attendanceId}", payload);
            TempData[response.Success ? "SuccessMessage" : "ErrorMessage"] = response.Success
                ? "Cập nhật điểm danh thành công"
                : response.Message;

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Export(
            DateTime? date,
            int? subjectId,
            string? startTime,
            string? endTime,
            int? examId,
            string? searchTerm)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            if (!examId.HasValue || examId.Value <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn môn/phòng trước khi xuất Excel.";
                return RedirectToAction(nameof(Index), new
                {
                    date = date?.ToString("yyyy-MM-dd"),
                    subjectId,
                    startTime,
                    endTime,
                    examId,
                    searchTerm
                });
            }

            var rosterResponse = await _apiService.GetAsync<List<AttendanceRosterItemViewModel>>(
                $"{AttendanceEndpoint}/roster",
                new Dictionary<string, string> { { "examId", examId.Value.ToString() } });

            if (!rosterResponse.Success || rosterResponse.Data == null)
            {
                TempData["ErrorMessage"] = rosterResponse.Message;
                return RedirectToAction(nameof(Index), new
                {
                    date = date?.ToString("yyyy-MM-dd"),
                    subjectId,
                    startTime,
                    endTime,
                    examId,
                    searchTerm
                });
            }

            var roster = rosterResponse.Data;
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                roster = roster.Where(r =>
                        r.StudentCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        r.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        r.ClassCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.AddWorksheet("Attendance");

            worksheet.Cell(1, 1).Value = "StudentCode";
            worksheet.Cell(1, 2).Value = "FullName";
            worksheet.Cell(1, 3).Value = "ClassCode";
            worksheet.Cell(1, 4).Value = "Status";
            worksheet.Cell(1, 5).Value = "StudentConfirmed";
            worksheet.Cell(1, 6).Value = "CheckInTime";
            worksheet.Cell(1, 7).Value = "CheckOutTime";

            var row = 2;
            foreach (var item in roster)
            {
                worksheet.Cell(row, 1).Value = item.StudentCode;
                worksheet.Cell(row, 2).Value = item.FullName;
                worksheet.Cell(row, 3).Value = item.ClassCode;
                worksheet.Cell(row, 4).Value = item.Status;
                worksheet.Cell(row, 5).Value = item.StudentConfirmed ? "Yes" : "No";
                worksheet.Cell(row, 6).Value = item.CheckInTime?.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cell(row, 7).Value = item.CheckOutTime?.ToString("dd/MM/yyyy HH:mm");
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new System.IO.MemoryStream();
            workbook.SaveAs(stream);
            var bytes = stream.ToArray();
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"attendance_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> Reports(
            DateTime? fromDate,
            DateTime? toDate,
            int? subjectId,
            int? classId)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var from = (fromDate ?? DateTime.Today.AddDays(-7)).Date;
            var to = (toDate ?? DateTime.Today).Date;

            var subjects = await LoadSubjectsAsync();
            var classes = await LoadClassesAsync();

            var queryParams = new Dictionary<string, string>
            {
                { "fromDate", from.ToString("yyyy-MM-dd") },
                { "toDate", to.ToString("yyyy-MM-dd") }
            };
            if (subjectId.HasValue) queryParams["subjectId"] = subjectId.Value.ToString();
            if (classId.HasValue) queryParams["classId"] = classId.Value.ToString();

            var reportResponse = await _apiService.GetAsync<List<AttendanceReportItemViewModel>>(
                $"{AttendanceEndpoint}/report",
                queryParams);

            var items = reportResponse.Success && reportResponse.Data != null
                ? reportResponse.Data
                : new List<AttendanceReportItemViewModel>();

            var model = new AttendanceReportPageViewModel
            {
                Filter = new AttendanceReportFilterViewModel
                {
                    FromDate = from,
                    ToDate = to,
                    SubjectId = subjectId,
                    ClassId = classId
                },
                Subjects = subjects,
                Classes = classes,
                Items = items,
                Total = items.Sum(x => x.Total),
                Present = items.Sum(x => x.Present),
                Absent = items.Sum(x => x.Absent),
                Late = items.Sum(x => x.Late),
                Excused = items.Sum(x => x.Excused),
                Confirmed = items.Sum(x => x.Confirmed)
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ExportReport(
            DateTime? fromDate,
            DateTime? toDate,
            int? subjectId,
            int? classId)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var from = (fromDate ?? DateTime.Today.AddDays(-7)).Date;
            var to = (toDate ?? DateTime.Today).Date;

            var queryParams = new Dictionary<string, string>
            {
                { "fromDate", from.ToString("yyyy-MM-dd") },
                { "toDate", to.ToString("yyyy-MM-dd") }
            };
            if (subjectId.HasValue) queryParams["subjectId"] = subjectId.Value.ToString();
            if (classId.HasValue) queryParams["classId"] = classId.Value.ToString();

            var reportResponse = await _apiService.GetAsync<List<AttendanceReportItemViewModel>>(
                $"{AttendanceEndpoint}/report",
                queryParams);

            if (!reportResponse.Success || reportResponse.Data == null)
            {
                TempData["ErrorMessage"] = reportResponse.Message;
                return RedirectToAction(nameof(Reports), new
                {
                    fromDate = from.ToString("yyyy-MM-dd"),
                    toDate = to.ToString("yyyy-MM-dd"),
                    subjectId,
                    classId
                });
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.AddWorksheet("AttendanceReport");

            worksheet.Cell(1, 1).Value = "Date";
            worksheet.Cell(1, 2).Value = "Slot";
            worksheet.Cell(1, 3).Value = "Subject";
            worksheet.Cell(1, 4).Value = "Class";
            worksheet.Cell(1, 5).Value = "Total";
            worksheet.Cell(1, 6).Value = "Present";
            worksheet.Cell(1, 7).Value = "Absent";
            worksheet.Cell(1, 8).Value = "Late";
            worksheet.Cell(1, 9).Value = "Excused";
            worksheet.Cell(1, 10).Value = "Confirmed";

            var row = 2;
            foreach (var item in reportResponse.Data)
            {
                worksheet.Cell(row, 1).Value = item.ExamDate.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 2).Value = $"{item.StartTime:hh\\:mm}-{item.EndTime:hh\\:mm}";
                worksheet.Cell(row, 3).Value = $"{item.SubjectCode} - {item.SubjectName}";
                worksheet.Cell(row, 4).Value = item.ClassCode;
                worksheet.Cell(row, 5).Value = item.Total;
                worksheet.Cell(row, 6).Value = item.Present;
                worksheet.Cell(row, 7).Value = item.Absent;
                worksheet.Cell(row, 8).Value = item.Late;
                worksheet.Cell(row, 9).Value = item.Excused;
                worksheet.Cell(row, 10).Value = item.Confirmed;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new System.IO.MemoryStream();
            workbook.SaveAs(stream);
            var bytes = stream.ToArray();

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"attendance_report_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        private static List<ExamScheduleSlotViewModel> BuildSlots(
            List<ExamViewModel> exams,
            DateTime date,
            int? subjectId)
        {
            var filtered = exams
                .Where(e => e.ExamDate.Date == date)
                .Where(e => !subjectId.HasValue || e.SubjectId == subjectId.Value)
                .OrderBy(e => e.StartTime)
                .ThenBy(e => e.EndTime)
                .ToList();

            return filtered
                .GroupBy(e => new { e.StartTime, e.EndTime })
                .Select(g => new ExamScheduleSlotViewModel
                {
                    StartTime = g.Key.StartTime,
                    EndTime = g.Key.EndTime,
                    Items = g.Select(e => new ExamScheduleItemViewModel { ExamId = e.Id, SubjectId = e.SubjectId, RoomId = e.RoomId }).ToList()
                })
                .ToList();
        }

        private static List<ExamOptionViewModel> BuildExamOptions(
            List<ExamViewModel> exams,
            List<SubjectFormViewModel> subjects,
            List<ClassFormViewModel> classes,
            DateTime date,
            int? subjectId,
            TimeSpan? startTime,
            TimeSpan? endTime)
        {
            if (!startTime.HasValue || !endTime.HasValue)
            {
                return new List<ExamOptionViewModel>();
            }

            var subjectMap = subjects.ToDictionary(s => s.Id, s => s);
            var classMap = classes.ToDictionary(c => c.Id, c => c);

            return exams
                .Where(e => e.ExamDate.Date == date)
                .Where(e => !subjectId.HasValue || e.SubjectId == subjectId.Value)
                .Where(e => !startTime.HasValue || e.StartTime == startTime.Value)
                .Where(e => !endTime.HasValue || e.EndTime == endTime.Value)
                .OrderBy(e => e.StartTime)
                .ThenBy(e => e.SubjectId)
                .Select(e =>
                {
                    subjectMap.TryGetValue(e.SubjectId, out var subject);
                    classMap.TryGetValue(e.ClassId, out var cls);
                    var subjectText = subject != null ? $"{subject.SubjectCode} - {subject.SubjectName}" : "N/A";
                    var classText = cls != null ? cls.ClassCode : "N/A";
                    return new ExamOptionViewModel
                    {
                        Id = e.Id,
                        ExamDate = e.ExamDate,
                        StartTime = e.StartTime,
                        EndTime = e.EndTime,
                        DisplayName = $"{e.ExamDate:dd/MM/yyyy} {e.StartTime:hh\\:mm}-{e.EndTime:hh\\:mm} | {subjectText} | {classText}"
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

        private static TimeSpan? ParseTime(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return TimeSpan.TryParse(value, out var time) ? time : null;
        }

    }
}
