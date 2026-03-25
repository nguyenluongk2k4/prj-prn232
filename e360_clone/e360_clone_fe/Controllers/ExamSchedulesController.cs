using e360_clone_fe.Models.ViewModels;
using e360_clone_fe.Services;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone_fe.Controllers
{
    public class ExamSchedulesController : BaseController
    {
        private const string ExamsEndpoint = "exams";

        public ExamSchedulesController(IApiService apiService, ILogger<ExamSchedulesController> logger)
            : base(apiService, logger)
        {
        }

        public async Task<IActionResult> Index(
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
            var weekStart = GetWeekStart(selectedDate);
            var weekEnd = weekStart.AddDays(6);
            var monthStart = new DateTime(selectedDate.Year, selectedDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            //var weekStart = GetWeekStart(selectedDate);
            //var weekEnd = weekStart.AddDays(6);
            //var monthStart = new DateTime(selectedDate.Year, selectedDate.Month, 1);
            //var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var examsResponse = await _apiService.GetAsync<List<ExamViewModel>>(
                ExamsEndpoint,
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var classResponse = await _apiService.GetAsync<List<ClassFormViewModel>>(
                "classes",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var subjectResponse = await _apiService.GetAsync<List<SubjectFormViewModel>>(
                "subjects",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var roomResponse = await _apiService.GetAsync<List<ExamRoomViewModel>>(
                "rooms",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var lecturerResponse = await _apiService.GetAsync<List<LecturerViewModel>>(
                "lecturers",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var proctorResponse = await _apiService.GetAsync<List<ProctorAssignmentViewModel>>(
                "proctors",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });

            var classMap = classResponse.Success && classResponse.Data != null
                ? classResponse.Data.ToDictionary(c => c.Id, c => c)
                : new Dictionary<int, ClassFormViewModel>();
            var subjectMap = subjectResponse.Success && subjectResponse.Data != null
                ? subjectResponse.Data.ToDictionary(s => s.Id, s => s)
                : new Dictionary<int, SubjectFormViewModel>();
            var roomMap = roomResponse.Success && roomResponse.Data != null
                ? roomResponse.Data.ToDictionary(r => r.Id, r => r)
                : new Dictionary<int, ExamRoomViewModel>();
            var lecturerMap = lecturerResponse.Success && lecturerResponse.Data != null
                ? lecturerResponse.Data.ToDictionary(l => l.Id, l => l)
                : new Dictionary<int, LecturerViewModel>();

            var assignmentMap = new Dictionary<int, List<string>>();
            var blockedLecturerMap = new Dictionary<int, HashSet<int>>();
            if (proctorResponse.Success && proctorResponse.Data != null)
            {
                var assignmentList = proctorResponse.Data;
                var examById = examsResponse.Success && examsResponse.Data != null
                    ? examsResponse.Data.ToDictionary(e => e.Id, e => e)
                    : new Dictionary<int, ExamViewModel>();

                foreach (var assignment in assignmentList)
                {
                    if (!assignmentMap.TryGetValue(assignment.ExamId, out var list))
                    {
                        list = new List<string>();
                        assignmentMap[assignment.ExamId] = list;
                    }

                    if (lecturerMap.TryGetValue(assignment.LecturerId, out var lecturer))
                    {
                        list.Add($"{lecturer.EmployeeCode} - {lecturer.FullName}");
                    }
                    else
                    {
                        list.Add($"Lecturer #{assignment.LecturerId}");
                    }

                    if (examById.TryGetValue(assignment.ExamId, out var assignedExam))
                    {
                        foreach (var exam in examById.Values)
                        {
                            if (exam.ExamDate.Date != assignedExam.ExamDate.Date)
                                continue;

                            if (!IsTimeOverlap(assignedExam.StartTime, assignedExam.EndTime, exam.StartTime, exam.EndTime))
                                continue;

                            if (!blockedLecturerMap.TryGetValue(exam.Id, out var blockedSet))
                            {
                                blockedSet = new HashSet<int>();
                                blockedLecturerMap[exam.Id] = blockedSet;
                            }

                            blockedSet.Add(assignment.LecturerId);
                        }
                    }
                }
            }

            var slots = new List<ExamScheduleSlotViewModel>();
            if (examsResponse.Success && examsResponse.Data != null)
            {
                var exams = examsResponse.Data
                    .Where(e => e.ExamDate.Date == selectedDate)
                    .OrderBy(e => e.StartTime)
                    .ThenBy(e => e.EndTime)
                    .ToList();

                var grouped = exams.GroupBy(e => new { e.StartTime, e.EndTime });
                foreach (var group in grouped)
                {
                    var slot = new ExamScheduleSlotViewModel
                    {
                        StartTime = group.Key.StartTime,
                        EndTime = group.Key.EndTime
                    };

                    foreach (var exam in group)
                    {
                        classMap.TryGetValue(exam.ClassId, out var cls);
                        subjectMap.TryGetValue(exam.SubjectId, out var subject);
                        roomMap.TryGetValue(exam.RoomId, out var room);

                        slot.Items.Add(new ExamScheduleItemViewModel
                        {
                            ExamId = exam.Id,
                            SubjectId = exam.SubjectId,
                            RoomId = exam.RoomId,
                            SubjectCode = subject?.SubjectCode ?? string.Empty,
                            SubjectName = subject?.SubjectName ?? string.Empty,
                            ClassCode = cls?.ClassCode ?? string.Empty,
                            ClassName = cls?.ClassName ?? string.Empty,
                            RoomCode = room?.RoomCode ?? string.Empty,
                            RoomName = room?.RoomName ?? string.Empty,
                            AcademicYear = exam.AcademicYear,
                            Semester = exam.Semester,
                            Status = exam.Status,
                            HasAssignment = assignmentMap.ContainsKey(exam.Id),
                            AssignedLecturers = assignmentMap.TryGetValue(exam.Id, out var assignedList)
                                ? string.Join(", ", assignedList.Distinct())
                                : string.Empty
                        });
                    }

                    if (slot.Items.Count > 0 && (!subjectId.HasValue || slot.Items.Any(i => i.SubjectId == subjectId.Value)))
                    {
                        slots.Add(slot);
                    }
                }
            }

            var model = new ExamSchedulePageViewModel
            {
                SelectedDate = selectedDate,
                Slots = slots,
                Subjects = subjectResponse.Success && subjectResponse.Data != null ? subjectResponse.Data : new List<SubjectFormViewModel>(),
                Lecturers = lecturerResponse.Success && lecturerResponse.Data != null ? lecturerResponse.Data : new List<LecturerViewModel>(),
                BlockedLecturerIds = blockedLecturerMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToList()),
                SelectedSubjectId = subjectId,
                SelectedStartTime = selectedStart,
                SelectedEndTime = selectedEnd
            };

            return View(model);
        }

        public async Task<IActionResult> My(
            DateTime? date,
            int? subjectId,
            string? startTime,
            string? endTime,
            string? view)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var lecturerId = HttpContext.Session.GetInt32("LecturerId");
            var email = HttpContext.Session.GetString("Email");
            var calendarView = string.IsNullOrWhiteSpace(view) ? "agendaWeek" : view;

            if (!lecturerId.HasValue && !string.IsNullOrWhiteSpace(email))
            {
                var lecturersResponse = await _apiService.GetAsync<List<LecturerViewModel>>(
                    "lecturers",
                    new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });

                if (lecturersResponse.Success && lecturersResponse.Data != null)
                {
                    var match = lecturersResponse.Data.FirstOrDefault(l =>
                        string.Equals(l.Email, email, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        lecturerId = match.Id;
                        HttpContext.Session.SetInt32("LecturerId", match.Id);
                    }
                }
            }

            if (!lecturerId.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy giảng viên cho tài khoản này.";
                ViewData["UseCalendar"] = true;
                ViewData["ListAction"] = "My";
                ViewData["Title"] = "Lịch coi thi của tôi";
                ViewData["CalendarView"] = calendarView;
                return View("Index", new ExamSchedulePageViewModel
                {
                    SelectedDate = (date ?? DateTime.Today).Date,
                    Slots = new List<ExamScheduleSlotViewModel>(),
                    Subjects = new List<SubjectFormViewModel>(),
                    Lecturers = new List<LecturerViewModel>(),
                    BlockedLecturerIds = new Dictionary<int, List<int>>(),
                    SelectedSubjectId = subjectId,
                    SelectedStartTime = ParseTime(startTime),
                    SelectedEndTime = ParseTime(endTime)
                });
            }

            var selectedDate = (date ?? DateTime.Today).Date;
            var selectedStart = ParseTime(startTime);
            var selectedEnd = ParseTime(endTime);
            var weekStart = GetWeekStart(selectedDate);
            var weekEnd = weekStart.AddDays(6);
            var monthStart = new DateTime(selectedDate.Year, selectedDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var proctorResponse = await _apiService.GetAsync<List<ProctorAssignmentViewModel>>(
                "proctors",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var examsResponse = await _apiService.GetAsync<List<ExamViewModel>>(
                ExamsEndpoint,
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var classResponse = await _apiService.GetAsync<List<ClassFormViewModel>>(
                "classes",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var subjectResponse = await _apiService.GetAsync<List<SubjectFormViewModel>>(
                "subjects",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var roomResponse = await _apiService.GetAsync<List<ExamRoomViewModel>>(
                "rooms",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var lecturersResponseForMatch = await _apiService.GetAsync<List<LecturerViewModel>>(
                "lecturers",
                new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });

            _logger.LogInformation(
                "ExamSchedules/My fetch: proctors={ProctorsOk} exams={ExamsOk} classes={ClassesOk} subjects={SubjectsOk} rooms={RoomsOk} lecturers={LecturersOk}",
                proctorResponse.Success, examsResponse.Success, classResponse.Success, subjectResponse.Success, roomResponse.Success, lecturersResponseForMatch.Success);

            var classMap = classResponse.Success && classResponse.Data != null
                ? classResponse.Data.ToDictionary(c => c.Id, c => c)
                : new Dictionary<int, ClassFormViewModel>();
            var subjectMap = subjectResponse.Success && subjectResponse.Data != null
                ? subjectResponse.Data.ToDictionary(s => s.Id, s => s)
                : new Dictionary<int, SubjectFormViewModel>();
            var roomMap = roomResponse.Success && roomResponse.Data != null
                ? roomResponse.Data.ToDictionary(r => r.Id, r => r)
                : new Dictionary<int, ExamRoomViewModel>();

            var candidateLecturerIds = new HashSet<int>();
            if (lecturerId.HasValue)
            {
                candidateLecturerIds.Add(lecturerId.Value);
            }
            if (!string.IsNullOrWhiteSpace(email) && lecturersResponseForMatch.Success && lecturersResponseForMatch.Data != null)
            {
                foreach (var lec in lecturersResponseForMatch.Data)
                {
                    if (string.Equals(lec.Email, email, StringComparison.OrdinalIgnoreCase))
                    {
                        candidateLecturerIds.Add(lec.Id);
                    }
                }
            }

            _logger.LogInformation("ExamSchedules/My lecturerId={LecturerId} email={Email} candidateIds=[{CandidateIds}]",
                lecturerId, email, string.Join(",", candidateLecturerIds));

            var assignedExamIds = proctorResponse.Success && proctorResponse.Data != null && candidateLecturerIds.Count > 0
                ? proctorResponse.Data
                    .Where(p => candidateLecturerIds.Contains(p.LecturerId))
                    .Select(p => p.ExamId)
                    .Distinct()
                    .ToHashSet()
                : new HashSet<int>();

            _logger.LogInformation("ExamSchedules/My assignedExamIds count={Count}", assignedExamIds.Count);

            var slots = new List<ExamScheduleSlotViewModel>();
            var calendarItems = new List<ExamScheduleCalendarItemViewModel>();
            if (examsResponse.Success && examsResponse.Data != null && assignedExamIds.Count > 0)
            {
                var exams = examsResponse.Data
                    .Where(e => assignedExamIds.Contains(e.Id))
                    .Where(e =>
                    {
                        var examDate = e.ExamDate.Kind == DateTimeKind.Utc
                            ? e.ExamDate.ToLocalTime().Date
                            : e.ExamDate.Date;
                        return calendarView switch
                        {
                            "agendaDay" => examDate == selectedDate,
                            "month" => examDate >= monthStart && examDate <= monthEnd,
                            _ => examDate >= weekStart && examDate <= weekEnd
                        };
                    })
                    .OrderBy(e => e.StartTime)
                    .ThenBy(e => e.EndTime)
                    .ToList();

                _logger.LogInformation("ExamSchedules/My selectedDate={SelectedDate:yyyy-MM-dd} view={View} weekStart={WeekStart:yyyy-MM-dd} weekEnd={WeekEnd:yyyy-MM-dd} monthStart={MonthStart:yyyy-MM-dd} monthEnd={MonthEnd:yyyy-MM-dd} matchedExams={Count}",
                    selectedDate, calendarView, weekStart, weekEnd, monthStart, monthEnd, exams.Count);

                foreach (var exam in exams)
                {
                    classMap.TryGetValue(exam.ClassId, out var cls);
                    subjectMap.TryGetValue(exam.SubjectId, out var subject);
                    roomMap.TryGetValue(exam.RoomId, out var room);

                    var examDate = exam.ExamDate.Kind == DateTimeKind.Utc
                        ? exam.ExamDate.ToLocalTime().Date
                        : exam.ExamDate.Date;

                    calendarItems.Add(new ExamScheduleCalendarItemViewModel
                    {
                        ExamId = exam.Id,
                        ExamDate = examDate,
                        StartTime = exam.StartTime,
                        EndTime = exam.EndTime,
                        SubjectId = exam.SubjectId,
                        SubjectCode = subject?.SubjectCode ?? string.Empty,
                        SubjectName = subject?.SubjectName ?? string.Empty,
                        ClassCode = cls?.ClassCode ?? string.Empty,
                        RoomCode = room?.RoomCode ?? string.Empty
                    });
                }

                var grouped = exams
                    .Where(e =>
                    {
                        var examDate = e.ExamDate.Kind == DateTimeKind.Utc
                            ? e.ExamDate.ToLocalTime().Date
                            : e.ExamDate.Date;
                        return examDate == selectedDate;
                    })
                    .GroupBy(e => new { e.StartTime, e.EndTime });
                foreach (var group in grouped)
                {
                    var slot = new ExamScheduleSlotViewModel
                    {
                        StartTime = group.Key.StartTime,
                        EndTime = group.Key.EndTime
                    };

                    foreach (var exam in group)
                    {
                        classMap.TryGetValue(exam.ClassId, out var cls);
                        subjectMap.TryGetValue(exam.SubjectId, out var subject);
                        roomMap.TryGetValue(exam.RoomId, out var room);

                        slot.Items.Add(new ExamScheduleItemViewModel
                        {
                            ExamId = exam.Id,
                            SubjectId = exam.SubjectId,
                            RoomId = exam.RoomId,
                            SubjectCode = subject?.SubjectCode ?? string.Empty,
                            SubjectName = subject?.SubjectName ?? string.Empty,
                            ClassCode = cls?.ClassCode ?? string.Empty,
                            ClassName = cls?.ClassName ?? string.Empty,
                            RoomCode = room?.RoomCode ?? string.Empty,
                            RoomName = room?.RoomName ?? string.Empty,
                            AcademicYear = exam.AcademicYear,
                            Semester = exam.Semester,
                            Status = exam.Status,
                            HasAssignment = true,
                            AssignedLecturers = string.Empty
                        });
                    }

                    if (slot.Items.Count > 0 && (!subjectId.HasValue || slot.Items.Any(i => i.SubjectId == subjectId.Value)))
                    {
                        slots.Add(slot);
                    }
                }
            }

            var model = new ExamSchedulePageViewModel
            {
                SelectedDate = selectedDate,
                Slots = slots,
                CalendarItems = calendarItems,
                Subjects = subjectResponse.Success && subjectResponse.Data != null ? subjectResponse.Data : new List<SubjectFormViewModel>(),
                Lecturers = new List<LecturerViewModel>(),
                BlockedLecturerIds = new Dictionary<int, List<int>>(),
                SelectedSubjectId = subjectId,
                SelectedStartTime = selectedStart,
                SelectedEndTime = selectedEnd
            };

            var totalItems = slots.Sum(s => s.Items.Count);
            var sampleItems = slots.SelectMany(s => s.Items)
                .Take(5)
                .Select(i => $"{i.SubjectCode}-{i.ClassCode}@{i.RoomCode}")
                .ToList();
            _logger.LogInformation("ExamSchedules/My slots={SlotCount} items={ItemCount} sample={Sample}",
                slots.Count, totalItems, string.Join(", ", sampleItems));

            ViewData["Title"] = "Lịch coi thi của tôi";
            ViewData["UseCalendar"] = true;
            ViewData["ListAction"] = "My";
            ViewData["CalendarView"] = calendarView;
            ViewData["CalendarTarget"] = "Attendance";
            ViewData["EventsUrl"] = Url.Action("MyEvents", "ExamSchedules");
            return View("Index", model);
        }

        public async Task<IActionResult> MyStudent(DateTime? date, string? view)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var studentId = HttpContext.Session.GetInt32("StudentId");
            var email = HttpContext.Session.GetString("Email");

            if (!studentId.HasValue && !string.IsNullOrWhiteSpace(email))
            {
                var studentsResponse = await _apiService.GetAsync<List<StudentViewModel>>(
                    "students",
                    new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });

                if (studentsResponse.Success && studentsResponse.Data != null)
                {
                    var match = studentsResponse.Data.FirstOrDefault(s =>
                        string.Equals(s.Email, email, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        studentId = match.Id;
                        HttpContext.Session.SetInt32("StudentId", match.Id);
                    }
                }
            }

            _logger.LogInformation("MyStudent resolved studentId={StudentId} email={Email}", studentId, email);

            if (!studentId.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sinh viên cho tài khoản này.";
                ViewData["UseCalendar"] = true;
                ViewData["ListAction"] = "MyStudent";
                ViewData["CalendarView"] = view ?? "agendaWeek";
                ViewData["CalendarTarget"] = "Student";
                ViewData["EventsUrl"] = Url.Action("MyStudentEvents", "ExamSchedules");
                return View("Index", new ExamSchedulePageViewModel
                {
                    SelectedDate = (date ?? DateTime.Today).Date,
                    Slots = new List<ExamScheduleSlotViewModel>(),
                    CalendarItems = new List<ExamScheduleCalendarItemViewModel>()
                });
            }

            var selectedDate = (date ?? DateTime.Today).Date;
            var calendarView = string.IsNullOrWhiteSpace(view) ? "agendaWeek" : view;
            var weekStart = GetWeekStart(selectedDate);
            var weekEnd = weekStart.AddDays(6);
            var monthStart = new DateTime(selectedDate.Year, selectedDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var dateRange = calendarView switch
            {
                "agendaDay" => (From: selectedDate, To: selectedDate),
                "month" => (From: monthStart, To: monthEnd),
                _ => (From: weekStart, To: weekEnd)
            };

            var examQuery = new Dictionary<string, string>
            {
                { "studentId", studentId.Value.ToString() },
                { "fromDate", dateRange.From.ToString("yyyy-MM-dd") },
                { "toDate", dateRange.To.ToString("yyyy-MM-dd") }
            };

            var examsResponse = await _apiService.GetAsync<List<ExamViewModel>>("exams/student", examQuery);
            _logger.LogInformation("MyStudent exams/student query={Query} success={Success} count={Count} message={Message}",
                string.Join("&", examQuery.Select(kv => $"{kv.Key}={kv.Value}")),
                examsResponse.Success,
                examsResponse.Data?.Count ?? 0,
                examsResponse.Message);
            var classResponse = await _apiService.GetAsync<List<ClassFormViewModel>>(
                "classes", new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var subjectResponse = await _apiService.GetAsync<List<SubjectFormViewModel>>(
                "subjects", new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var roomResponse = await _apiService.GetAsync<List<ExamRoomViewModel>>(
                "rooms", new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });

            var classMap = classResponse.Success && classResponse.Data != null
                ? classResponse.Data.ToDictionary(c => c.Id, c => c)
                : new Dictionary<int, ClassFormViewModel>();
            var subjectMap = subjectResponse.Success && subjectResponse.Data != null
                ? subjectResponse.Data.ToDictionary(s => s.Id, s => s)
                : new Dictionary<int, SubjectFormViewModel>();
            var roomMap = roomResponse.Success && roomResponse.Data != null
                ? roomResponse.Data.ToDictionary(r => r.Id, r => r)
                : new Dictionary<int, ExamRoomViewModel>();

            var calendarItems = new List<ExamScheduleCalendarItemViewModel>();
            if (examsResponse.Success && examsResponse.Data != null)
            {
                foreach (var exam in examsResponse.Data)
                {
                    classMap.TryGetValue(exam.ClassId, out var cls);
                    subjectMap.TryGetValue(exam.SubjectId, out var subject);
                    roomMap.TryGetValue(exam.RoomId, out var room);

                    var examDate = exam.ExamDate.Kind == DateTimeKind.Utc
                        ? exam.ExamDate.ToLocalTime().Date
                        : exam.ExamDate.Date;

                    calendarItems.Add(new ExamScheduleCalendarItemViewModel
                    {
                        ExamId = exam.Id,
                        ExamDate = examDate,
                        StartTime = exam.StartTime,
                        EndTime = exam.EndTime,
                        SubjectId = exam.SubjectId,
                        SubjectCode = subject?.SubjectCode ?? string.Empty,
                        SubjectName = subject?.SubjectName ?? string.Empty,
                        ClassCode = cls?.ClassCode ?? string.Empty,
                        RoomCode = room?.RoomCode ?? string.Empty
                    });
                }
            }

            var model = new ExamSchedulePageViewModel
            {
                SelectedDate = selectedDate,
                CalendarItems = calendarItems
            };

            if (calendarItems.Count > 0)
            {
                var sample = calendarItems.Take(3).Select(i => $"{i.SubjectCode}-{i.ClassCode}@{i.ExamDate:yyyy-MM-dd}");
                _logger.LogInformation("MyStudent calendar items count={Count} sample={Sample}",
                    calendarItems.Count, string.Join(", ", sample));
            }
            else
            {
                _logger.LogWarning("MyStudent calendar items empty");
            }

            ViewData["Title"] = "Lịch thi của tôi";
            ViewData["UseCalendar"] = true;
            ViewData["ListAction"] = "MyStudent";
            ViewData["CalendarView"] = calendarView;
            ViewData["CalendarTarget"] = "Student";
            ViewData["EventsUrl"] = Url.Action("MyStudentEvents", "ExamSchedules");
            return View("Index", model);
        }

        [HttpGet]
        public async Task<IActionResult> MyStudentEvents(DateTime? date, string? view)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var studentId = HttpContext.Session.GetInt32("StudentId");
            var email = HttpContext.Session.GetString("Email");
            var selectedDate = (date ?? DateTime.Today).Date;
            var calendarView = string.IsNullOrWhiteSpace(view) ? "agendaWeek" : view;

            if (!studentId.HasValue && !string.IsNullOrWhiteSpace(email))
            {
                var studentsResponse = await _apiService.GetAsync<List<StudentViewModel>>(
                    "students",
                    new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });

                if (studentsResponse.Success && studentsResponse.Data != null)
                {
                    var match = studentsResponse.Data.FirstOrDefault(s =>
                        string.Equals(s.Email, email, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        studentId = match.Id;
                        HttpContext.Session.SetInt32("StudentId", match.Id);
                    }
                }
            }

            if (!studentId.HasValue)
            {
                return Json(new { success = false, data = Array.Empty<object>() });
            }

            var weekStart = GetWeekStart(selectedDate);
            var weekEnd = weekStart.AddDays(6);
            var monthStart = new DateTime(selectedDate.Year, selectedDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var dateRange = calendarView switch
            {
                "agendaDay" => (From: selectedDate, To: selectedDate),
                "month" => (From: monthStart, To: monthEnd),
                _ => (From: weekStart, To: weekEnd)
            };

            var examQuery = new Dictionary<string, string>
            {
                { "studentId", studentId.Value.ToString() },
                { "fromDate", dateRange.From.ToString("yyyy-MM-dd") },
                { "toDate", dateRange.To.ToString("yyyy-MM-dd") }
            };

            var examsResponse = await _apiService.GetAsync<List<ExamViewModel>>("exams/student", examQuery);
            _logger.LogInformation("MyStudentEvents exams/student query={Query} success={Success} count={Count} message={Message}",
                string.Join("&", examQuery.Select(kv => $"{kv.Key}={kv.Value}")),
                examsResponse.Success,
                examsResponse.Data?.Count ?? 0,
                examsResponse.Message);
            var classResponse = await _apiService.GetAsync<List<ClassFormViewModel>>(
                "classes", new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var subjectResponse = await _apiService.GetAsync<List<SubjectFormViewModel>>(
                "subjects", new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var roomResponse = await _apiService.GetAsync<List<ExamRoomViewModel>>(
                "rooms", new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });

            var classMap = classResponse.Success && classResponse.Data != null
                ? classResponse.Data.ToDictionary(c => c.Id, c => c)
                : new Dictionary<int, ClassFormViewModel>();
            var subjectMap = subjectResponse.Success && subjectResponse.Data != null
                ? subjectResponse.Data.ToDictionary(s => s.Id, s => s)
                : new Dictionary<int, SubjectFormViewModel>();
            var roomMap = roomResponse.Success && roomResponse.Data != null
                ? roomResponse.Data.ToDictionary(r => r.Id, r => r)
                : new Dictionary<int, ExamRoomViewModel>();

            var items = new List<ExamScheduleCalendarItemViewModel>();
            if (examsResponse.Success && examsResponse.Data != null)
            {
                foreach (var exam in examsResponse.Data)
                {
                    classMap.TryGetValue(exam.ClassId, out var cls);
                    subjectMap.TryGetValue(exam.SubjectId, out var subject);
                    roomMap.TryGetValue(exam.RoomId, out var room);

                    var examDate = exam.ExamDate.Kind == DateTimeKind.Utc
                        ? exam.ExamDate.ToLocalTime().Date
                        : exam.ExamDate.Date;

                    items.Add(new ExamScheduleCalendarItemViewModel
                    {
                        ExamId = exam.Id,
                        ExamDate = examDate,
                        StartTime = exam.StartTime,
                        EndTime = exam.EndTime,
                        SubjectId = exam.SubjectId,
                        SubjectCode = subject?.SubjectCode ?? string.Empty,
                        SubjectName = subject?.SubjectName ?? string.Empty,
                        ClassCode = cls?.ClassCode ?? string.Empty,
                        RoomCode = room?.RoomCode ?? string.Empty
                    });
                }
            }

            return Json(new { success = true, data = items });
        }

        [HttpGet]
        public async Task<IActionResult> MyEvents(DateTime? date, string? view)
        {
            var authResult = RequireAuth();
            if (authResult != null) return authResult;

            var lecturerId = HttpContext.Session.GetInt32("LecturerId");
            var email = HttpContext.Session.GetString("Email");
            var calendarView = string.IsNullOrWhiteSpace(view) ? "agendaWeek" : view;
            var selectedDate = (date ?? DateTime.Today).Date;

            if (!lecturerId.HasValue && !string.IsNullOrWhiteSpace(email))
            {
                var lecturersResponse = await _apiService.GetAsync<List<LecturerViewModel>>(
                    "lecturers",
                    new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });

                if (lecturersResponse.Success && lecturersResponse.Data != null)
                {
                    var match = lecturersResponse.Data.FirstOrDefault(l =>
                        string.Equals(l.Email, email, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        lecturerId = match.Id;
                        HttpContext.Session.SetInt32("LecturerId", match.Id);
                    }
                }
            }

            if (!lecturerId.HasValue)
            {
                return Json(new { success = false, data = Array.Empty<object>() });
            }

            var weekStart = GetWeekStart(selectedDate);
            var weekEnd = weekStart.AddDays(6);
            var monthStart = new DateTime(selectedDate.Year, selectedDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var proctorQuery = new Dictionary<string, string>
            {
                { "pageNumber", "1" },
                { "pageSize", "2000" },
                { "lecturerId", lecturerId.Value.ToString() }
            };

            var dateRange = calendarView switch
            {
                "agendaDay" => (From: selectedDate, To: selectedDate),
                "month" => (From: monthStart, To: monthEnd),
                _ => (From: weekStart, To: weekEnd)
            };

            var examQuery = new Dictionary<string, string>
            {
                { "pageNumber", "1" },
                { "pageSize", "2000" },
                { "fromDate", dateRange.From.ToString("yyyy-MM-dd") },
                { "toDate", dateRange.To.ToString("yyyy-MM-dd") }
            };

            var proctorResponse = await _apiService.GetAsync<List<ProctorAssignmentViewModel>>("proctors", proctorQuery);
            var examsResponse = await _apiService.GetAsync<List<ExamViewModel>>(ExamsEndpoint, examQuery);
            var classResponse = await _apiService.GetAsync<List<ClassFormViewModel>>("classes", new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var subjectResponse = await _apiService.GetAsync<List<SubjectFormViewModel>>("subjects", new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });
            var roomResponse = await _apiService.GetAsync<List<ExamRoomViewModel>>("rooms", new Dictionary<string, string> { { "pageNumber", "1" }, { "pageSize", "2000" } });

            var classMap = classResponse.Success && classResponse.Data != null
                ? classResponse.Data.ToDictionary(c => c.Id, c => c)
                : new Dictionary<int, ClassFormViewModel>();
            var subjectMap = subjectResponse.Success && subjectResponse.Data != null
                ? subjectResponse.Data.ToDictionary(s => s.Id, s => s)
                : new Dictionary<int, SubjectFormViewModel>();
            var roomMap = roomResponse.Success && roomResponse.Data != null
                ? roomResponse.Data.ToDictionary(r => r.Id, r => r)
                : new Dictionary<int, ExamRoomViewModel>();

            var assignedExamIds = proctorResponse.Success && proctorResponse.Data != null
                ? proctorResponse.Data.Select(p => p.ExamId).Distinct().ToHashSet()
                : new HashSet<int>();

            var items = new List<ExamScheduleCalendarItemViewModel>();
            if (examsResponse.Success && examsResponse.Data != null && assignedExamIds.Count > 0)
            {
                foreach (var exam in examsResponse.Data.Where(e => assignedExamIds.Contains(e.Id)))
                {
                    classMap.TryGetValue(exam.ClassId, out var cls);
                    subjectMap.TryGetValue(exam.SubjectId, out var subject);
                    roomMap.TryGetValue(exam.RoomId, out var room);

                    var examDate = exam.ExamDate.Kind == DateTimeKind.Utc
                        ? exam.ExamDate.ToLocalTime().Date
                        : exam.ExamDate.Date;

                    items.Add(new ExamScheduleCalendarItemViewModel
                    {
                        ExamId = exam.Id,
                        ExamDate = examDate,
                        StartTime = exam.StartTime,
                        EndTime = exam.EndTime,
                        SubjectId = exam.SubjectId,
                        SubjectCode = subject?.SubjectCode ?? string.Empty,
                        SubjectName = subject?.SubjectName ?? string.Empty,
                        ClassCode = cls?.ClassCode ?? string.Empty,
                        RoomCode = room?.RoomCode ?? string.Empty
                    });
                }
            }

            return Json(new { success = true, data = items });
        }

        public IActionResult Create()
        {
            return RedirectToAction("Create", "Exams");
        }

        private static TimeSpan? ParseTime(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return TimeSpan.TryParse(value, out var time) ? time : null;
        }

        private static bool IsTimeOverlap(TimeSpan start1, TimeSpan end1, TimeSpan start2, TimeSpan end2)
        {
            return start1 < end2 && start2 < end1;
        }

        private static DateTime GetWeekStart(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }
    }
}
