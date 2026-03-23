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
    }
}
