using e360_clone.BusinessObjects;
using e360_clone.BusinessObjects.DTOs;
using e360_clone.Repositories;
using e360_clone_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace e360_clone.Controllers
{
    public class ExamsController : BaseApiController
    {
        private readonly IExamRepository _repository;
        private readonly IExamRoomAllocationRepository _allocationRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IEmailService _emailService;
        private readonly IAccountRepository _accountRepository;
        private readonly IExamNotificationQueue _notificationQueue;
        private readonly ILogger<ExamsController> _logger;

        public ExamsController(
            IExamRepository repository,
            IExamRoomAllocationRepository allocationRepository,
            ISubjectRepository subjectRepository,
            IEmailService emailService,
            IAccountRepository accountRepository,
            IExamNotificationQueue notificationQueue,
            ILogger<ExamsController> logger)
        {
            _repository = repository;
            _allocationRepository = allocationRepository;
            _subjectRepository = subjectRepository;
            _emailService = emailService;
            _accountRepository = accountRepository;
            _notificationQueue = notificationQueue;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] PagedRequest request,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var result = await _repository.GetPagedFilteredWithMetaAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                fromDate,
                toDate);

            return Ok(new PagedResponse<Exam>
            {
                Success = true,
                Message = "Lấy danh sách kỳ thi thành công",
                Data = result.Items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = result.TotalRecords
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"Không tìm thấy kỳ thi có ID = {id}");

            return HandleResult(item, "Lấy thông tin kỳ thi thành công");
        }

        [HttpGet("student")]
        public async Task<IActionResult> GetByStudent(
            [FromQuery] int studentId,
            [FromQuery] string? email,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                if (studentId <= 0 && string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest(new ApiResponse<List<StudentExamScheduleDto>>
                    {
                        Success = false,
                        Message = "StudentId hoặc email không hợp lệ"
                    });
                }

                var exams = await _repository.GetStudentScheduleAsync(studentId, email, fromDate, toDate);

                return Ok(new ApiResponse<List<StudentExamScheduleDto>>
                {
                    Success = true,
                    Message = "Lấy lịch thi sinh viên thành công",
                    Data = exams
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByStudent failed. studentId={StudentId} email={Email} from={From} to={To}", studentId, email, fromDate, toDate);
                return HandleError($"GetByStudent failed: {ex.Message}");
            }
        }

        [HttpGet("subject-student-count")]
        public async Task<IActionResult> GetStudentCount([FromQuery] int subjectId, [FromQuery] int? classId)
        {
            if (subjectId <= 0)
            {
                return BadRequest(new ApiResponse<int>
                {
                    Success = false,
                    Message = "SubjectId không hợp lệ",
                    Data = 0
                });
            }

            var count = await _repository.GetStudentCountForSubjectAsync(subjectId, classId);
            return Ok(new ApiResponse<int>
            {
                Success = true,
                Message = "Lấy số lượng sinh viên thành công",
                Data = count
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ExamCreateRequestDto request)
        {
            try
            {
                if (request == null || request.Exam == null)
                {
                    return BadRequest(new ApiResponse<Exam>
                    {
                        Success = false,
                        Message = "Dữ liệu không hợp lệ"
                    });
                }

                var exam = request.Exam;
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<Exam> { Success = false, Message = "Dữ liệu không hợp lệ" });

                _logger.LogInformation("Create exam request: subjectId={SubjectId} classId={ClassId} date={Date} start={Start} end={End} applyAll={ApplyAll} rooms={Rooms}",
                    exam.SubjectId, exam.ClassId, exam.ExamDate, exam.StartTime, exam.EndTime, request.ApplyAllClasses, request.RoomIds?.Count ?? 0);

                var roomIds = request.RoomIds?
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList() ?? new List<int>();

                if (roomIds.Count == 0 && exam.RoomId > 0)
                {
                    roomIds.Add(exam.RoomId);
                }

                exam.ExamDate = NormalizeExamDate(exam.ExamDate);

                var applyAllClasses = request.ApplyAllClasses || exam.ClassId <= 0;
                var createdAt = DateTime.UtcNow;
                var subjectCode = await _repository.GetSubjectCodeAsync(exam.SubjectId);
                var createdExams = new List<Exam>();
                exam.Notes ??= string.Empty;

                if (applyAllClasses)
                {
                    var classInfos = await _repository.GetClassesForSubjectAsync(exam.SubjectId);
                    if (classInfos.Count == 0)
                    {
                        return BadRequest(new ApiResponse<Exam>
                        {
                            Success = false,
                            Message = "Không tìm thấy lớp đang học môn này."
                        });
                    }

                    if (roomIds.Count == 0)
                    {
                        return BadRequest(new ApiResponse<Exam>
                        {
                            Success = false,
                            Message = "Vui lòng chọn ít nhất một phòng thi."
                        });
                    }

                    for (var i = 0; i < classInfos.Count; i++)
                    {
                        var classInfo = classInfos[i];
                        var roomId = roomIds[i % roomIds.Count];
                        var generatedCode = BuildExamCode(exam.ExamCode, subjectCode, classInfo.Code, i);
                        createdExams.Add(new Exam
                        {
                            ExamCode = generatedCode,
                            ExamName = exam.ExamName,
                            ExamType = exam.ExamType,
                            SubjectId = exam.SubjectId,
                            ClassId = classInfo.Id,
                            ExamDate = exam.ExamDate,
                            StartTime = exam.StartTime,
                            EndTime = exam.EndTime,
                            Duration = exam.Duration,
                            RoomId = roomId,
                            AcademicYear = exam.AcademicYear,
                            Semester = exam.Semester,
                            Status = exam.Status,
                            Notes = exam.Notes ?? string.Empty,
                            CreatedBy = exam.CreatedBy,
                            CreatedAt = createdAt
                        });
                    }

                    await _repository.AddRangeAsync(createdExams);
                }
                else
                {
                    exam.CreatedAt = createdAt;
                    if (roomIds.Count > 0)
                    {
                        exam.RoomId = roomIds[0];
                    }
                    if (string.IsNullOrWhiteSpace(exam.ExamCode))
                    {
                        exam.ExamCode = BuildExamCode(exam.ExamCode, subjectCode, exam.ClassId.ToString(), 0);
                    }
                    exam.Notes ??= string.Empty;
                    await _repository.AddAsync(exam);
                    createdExams.Add(exam);
                }

                var firstExam = createdExams.FirstOrDefault();
                foreach (var created in createdExams)
                {
                    if (created.Id > 0)
                    {
                        _notificationQueue.Enqueue(new ExamNotificationMessage(created.Id, DateTime.UtcNow));
                        _logger.LogInformation("Queued exam notification. ExamId={ExamId}", created.Id);
                    }
                }
                return CreatedAtAction(nameof(GetById), new { id = firstExam?.Id ?? 0 }, new ApiResponse<Exam>
                {
                    Success = true,
                    Message = createdExams.Count > 1
                        ? $"Tạo {createdExams.Count} kỳ thi thành công"
                        : "Thêm kỳ thi thành công",
                    Data = firstExam
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create exam failed.");
                return HandleError($"Không tạo được kỳ thi: {ex.Message}");
            }
        }

        [HttpGet("{examId}/allocations")]
        public async Task<IActionResult> GetAllocations(int examId)
        {
            var summary = await _allocationRepository.GetSummaryAsync(examId);
            if (summary == null)
            {
                return HandleNotFound("Không tìm thấy kỳ thi.");
            }

            return Ok(new ApiResponse<ExamAllocationSummaryDto>
            {
                Success = true,
                Message = "Lấy danh sách xếp phòng thành công",
                Data = summary
            });
        }

        [HttpPost("{examId}/allocations/auto")]
        public async Task<IActionResult> AutoAllocate(int examId, [FromBody] ExamAllocationAutoRequestDto request)
        {
            try
            {
                if (request == null || request.RoomIds == null || request.RoomIds.Count == 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Vui lòng chọn ít nhất một phòng thi."
                    });
                }

                var summary = await _allocationRepository.AutoAllocateAsync(examId, request.RoomIds);
                if (summary == null)
                {
                    return HandleNotFound("Không tìm thấy kỳ thi.");
                }

                await SendAllocationEmailsAsync(summary);

                return Ok(new ApiResponse<ExamAllocationSummaryDto>
                {
                    Success = true,
                    Message = "Xếp sinh viên tự động thành công",
                    Data = summary
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AutoAllocate failed. examId={ExamId}", examId);
                return HandleError($"Không thể xếp sinh viên: {ex.Message}");
            }
        }

        [HttpPost("{examId}/allocations/manual")]
        public async Task<IActionResult> SaveAllocations(int examId, [FromBody] ExamAllocationSaveRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Dữ liệu không hợp lệ"
                    });
                }

                var summary = await _allocationRepository.SaveAllocationsAsync(examId, request.Allocations ?? new List<ExamAllocationItemDto>());
                if (summary == null)
                {
                    return HandleNotFound("Không tìm thấy kỳ thi.");
                }

                await SendAllocationEmailsAsync(summary);

                return Ok(new ApiResponse<ExamAllocationSummaryDto>
                {
                    Success = true,
                    Message = "Lưu danh sách sinh viên thành công",
                    Data = summary
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveAllocations failed. examId={ExamId}", examId);
                return HandleError($"Không thể lưu danh sách sinh viên: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Exam exam)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return HandleNotFound($"Không tìm thấy kỳ thi có ID = {id}");

            existing.ExamCode = exam.ExamCode;
            existing.ExamName = exam.ExamName;
            existing.ExamType = exam.ExamType;
            existing.SubjectId = exam.SubjectId;
            existing.ClassId = exam.ClassId;
            existing.ExamDate = NormalizeExamDate(exam.ExamDate);
            existing.StartTime = exam.StartTime;
            existing.EndTime = exam.EndTime;
            existing.Duration = exam.Duration;
            existing.RoomId = exam.RoomId;
            existing.AcademicYear = exam.AcademicYear;
            existing.Semester = exam.Semester;
            existing.Status = exam.Status;
            existing.Notes = exam.Notes ?? string.Empty;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);
            return HandleResult(existing, "Cập nhật kỳ thi thành công");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return HandleNotFound($"Không tìm thấy kỳ thi có ID = {id}");

            await _repository.DeleteAsync(item);
            return HandleResult(true, "Xóa kỳ thi thành công");
        }

        private static string BuildExamCode(string? baseCode, string? subjectCode, string classCode, int index)
        {
            var seed = string.IsNullOrWhiteSpace(baseCode)
                ? $"{subjectCode ?? "EX"}-{classCode}"
                : $"{baseCode}-{classCode}";

            if (seed.Length > 20)
            {
                seed = seed.Substring(0, 20);
            }

            if (index == 0)
            {
                return seed;
            }

            var suffix = $"-{index + 1}";
            var maxBaseLength = Math.Max(1, 20 - suffix.Length);
            var trimmed = seed.Length > maxBaseLength ? seed.Substring(0, maxBaseLength) : seed;
            return $"{trimmed}{suffix}";
        }

        private static DateTime NormalizeExamDate(DateTime value)
        {
            var dateOnly = value.Date;
            return DateTime.SpecifyKind(dateOnly, DateTimeKind.Utc);
        }

        private async Task SendAllocationEmailsAsync(ExamAllocationSummaryDto summary)
        {
            try
            {
                if (summary.Allocations.Count == 0)
                {
                    return;
                }

                var exam = await _repository.GetByIdAsync(summary.ExamId);
                if (exam == null)
                {
                    return;
                }

                var subject = await _subjectRepository.GetByIdAsync(exam.SubjectId);
                var subjectText = subject != null
                    ? $"{subject.SubjectCode} - {subject.SubjectName}"
                    : $"Môn học ID {exam.SubjectId}";

                var roomMap = summary.Rooms.ToDictionary(r => r.RoomId);
                var allocMap = summary.Allocations.ToDictionary(a => a.StudentId);

                var studentIds = summary.Students.Select(s => s.StudentId).Distinct().ToList();
                var accounts = await _accountRepository.GetByStudentIdsAsync(studentIds);
                var accountEmailMap = accounts
                    .Where(a => a.StudentId.HasValue && !string.IsNullOrWhiteSpace(a.Email))
                    .GroupBy(a => a.StudentId!.Value)
                    .ToDictionary(g => g.Key, g => g.First().Email);

                foreach (var student in summary.Students)
                {
                    if (!accountEmailMap.TryGetValue(student.StudentId, out var email))
                    {
                        continue;
                    }

                    if (!allocMap.TryGetValue(student.StudentId, out var alloc))
                    {
                        continue;
                    }

                    roomMap.TryGetValue(alloc.RoomId, out var room);
                    var roomText = room != null
                        ? $"{room.RoomCode} - {room.RoomName}"
                        : $"Phòng ID {alloc.RoomId}";

                    var subjectLine = $"[E360] Thông báo lịch thi {exam.ExamCode}";
                    var body = $@"
                        <p>Xin chào <strong>{student.FullName}</strong> ({student.StudentCode}),</p>
                        <p>Bạn đã được xếp phòng thi cho kỳ thi:</p>
                        <ul>
                            <li><strong>Môn:</strong> {subjectText}</li>
                            <li><strong>Kỳ thi:</strong> {exam.ExamName} ({exam.ExamCode})</li>
                            <li><strong>Ngày thi:</strong> {exam.ExamDate:dd/MM/yyyy}</li>
                            <li><strong>Giờ thi:</strong> {exam.StartTime.ToString(@"hh\:mm")} - {exam.EndTime.ToString(@"hh\:mm")}</li>
                            <li><strong>Phòng:</strong> {roomText}</li>
                            <li><strong>Số ghế:</strong> {alloc.SeatNumber}</li>
                        </ul>
                        <p>Vui lòng đến đúng giờ và mang theo giấy tờ cần thiết.</p>";

                    await _emailService.SendAsync(email, subjectLine, body);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendAllocationEmailsAsync failed. examId={ExamId}", summary.ExamId);
            }
        }
    }
}
