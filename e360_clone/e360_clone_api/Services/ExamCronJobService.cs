using e360_clone.DataAccess;
using e360_clone.BusinessObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace e360_clone_api.Services
{
    public class ExamCronJobService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExamCronJobService> _logger;
        private readonly CronJobSettings _settings;
        private readonly TimeZoneInfo _timeZone;

        public ExamCronJobService(
            IServiceScopeFactory scopeFactory,
            ILogger<ExamCronJobService> logger,
            IOptions<CronJobSettings> settings)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _settings = settings.Value;
            _timeZone = GetVietnamTimeZone();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var intervalMinutes = Math.Max(1, _settings.IntervalMinutes);
            var interval = TimeSpan.FromMinutes(intervalMinutes);

            _logger.LogInformation(
                "CronJob started. Interval={IntervalMinutes}m, Enabled={Enabled}, Grace={Grace}m, TimeZone={TimeZoneId}",
                intervalMinutes,
                _settings.Enabled,
                _settings.CheckInGraceMinutes,
                _timeZone.Id);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_settings.Enabled)
                {
                    try
                    {
                        await RunOnceAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "CronJob failed.");
                    }
                }

                await Task.Delay(interval, stoppingToken);
            }
        }

        private async Task RunOnceAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var nowUtc = DateTime.UtcNow;
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, _timeZone);
            _logger.LogInformation(
                "CronJob tick at {NowLocal} (UTC {NowUtc})",
                nowLocal.ToString("yyyy-MM-dd HH:mm:ss"),
                nowUtc.ToString("yyyy-MM-dd HH:mm:ss"));
            var updatedExamIds = new List<int>();

            var exams = await db.Exams
                .Where(e => e.Status != "Cancelled" && e.Status != "Completed")
                .ToListAsync(stoppingToken);
            _logger.LogInformation("Loaded {Count} exams for status evaluation.", exams.Count);

            var plannedToInProgress = 0;
            var inProgressToCompleted = 0;
            foreach (var exam in exams)
            {
                var examDateLocal = GetLocalExamDate(exam.ExamDate);
                var start = examDateLocal.Date.Add(exam.StartTime);
                var end = examDateLocal.Date.Add(exam.EndTime);
                var status = string.IsNullOrWhiteSpace(exam.Status) ? "Planned" : exam.Status.Trim();
                var updated = false;

                if (status == "Planned" && nowLocal >= start)
                {
                    exam.Status = "InProgress";
                    updated = true;
                    plannedToInProgress++;
                    _logger.LogInformation("Exam {ExamId} moved Planned -> InProgress (Start={Start}).", exam.Id, start);
                }

                if (nowLocal >= end && exam.Status != "Completed")
                {
                    exam.Status = "Completed";
                    updated = true;
                    updatedExamIds.Add(exam.Id);
                    inProgressToCompleted++;
                    _logger.LogInformation("Exam {ExamId} moved to Completed (End={End}).", exam.Id, end);
                }

                if (updated)
                {
                    exam.UpdatedAt = nowUtc;
                }
            }

            if (exams.Count > 0)
            {
                await db.SaveChangesAsync(stoppingToken);
            }
            _logger.LogInformation(
                "Exam status updates summary: Planned->InProgress={PlannedToInProgress}, InProgress->Completed={InProgressToCompleted}",
                plannedToInProgress,
                inProgressToCompleted);

            await UpdateAttendanceAndNotifyAsync(db, emailService, nowUtc, nowLocal, stoppingToken);
        }

        private async Task UpdateAttendanceAndNotifyAsync(
            AppDbContext db,
            IEmailService emailService,
            DateTime nowUtc,
            DateTime nowLocal,
            CancellationToken stoppingToken)
        {
            var graceMinutes = Math.Max(0, _settings.CheckInGraceMinutes);
            var grace = TimeSpan.FromMinutes(graceMinutes);
            var threshold = nowLocal - grace;

            var completedExams = await db.Exams
                .Where(e => e.Status == "Completed")
                .ToListAsync(stoppingToken);

            var closedExamIds = completedExams
                .Where(e => GetLocalExamDate(e.ExamDate).Date.Add(e.EndTime) <= threshold)
                .Select(e => e.Id)
                .ToList();

            if (closedExamIds.Count == 0) return;

            var pendingAttendances = await db.Attendances
                .Where(a => closedExamIds.Contains(a.ExamId) && a.Status == "Pending")
                .ToListAsync(stoppingToken);

            if (pendingAttendances.Count == 0) return;

            _logger.LogInformation(
                "Attendance pending count={Count}. Threshold={ThresholdLocal} (grace {Grace}m).",
                pendingAttendances.Count,
                threshold.ToString("yyyy-MM-dd HH:mm:ss"),
                graceMinutes);

            var studentIds = pendingAttendances.Select(a => a.StudentId).Distinct().ToList();
            var studentMap = await db.Students
                .Where(s => studentIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, stoppingToken);

            var examMap = completedExams.ToDictionary(e => e.Id, e => e);

            var notified = 0;
            foreach (var attendance in pendingAttendances)
            {
                attendance.Status = "Absent";
                attendance.RecordedAt = nowUtc;

                if (!studentMap.TryGetValue(attendance.StudentId, out var student))
                {
                    _logger.LogWarning("Attendance {AttendanceId}: student not found (StudentId={StudentId}).", attendance.Id, attendance.StudentId);
                    continue;
                }

                if (!examMap.TryGetValue(attendance.ExamId, out var exam))
                {
                    _logger.LogWarning("Attendance {AttendanceId}: exam not found (ExamId={ExamId}).", attendance.Id, attendance.ExamId);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(student.Email))
                {
                    _logger.LogWarning("Attendance {AttendanceId}: student email missing (StudentId={StudentId}).", attendance.Id, attendance.StudentId);
                    continue;
                }

                var subject = await db.Subjects.FirstOrDefaultAsync(s => s.Id == exam.SubjectId, stoppingToken);
                var subjectName = subject != null ? $"{subject.SubjectCode} - {subject.SubjectName}" : exam.ExamName;
                var examDateLocal = GetLocalExamDate(exam.ExamDate);
                var start = examDateLocal.Date.Add(exam.StartTime);
                var end = examDateLocal.Date.Add(exam.EndTime);

                var subjectLine = $"Nhac diem danh: {subjectName}";
                var htmlBody = $@"
                    <p>Chao {student.FullName},</p>
                    <p>Ban chua check-in diem danh cho ca thi:</p>
                    <ul>
                        <li>Mon/Ky thi: {subjectName}</li>
                        <li>Thoi gian: {start:dd/MM/yyyy HH:mm} - {end:dd/MM/yyyy HH:mm}</li>
                    </ul>
                    <p>He thong da ghi nhan trang thai vang mat do qua thoi gian diem danh.</p>
                ";

                var sent = await emailService.SendAsync(student.Email, subjectLine, htmlBody);
                if (sent)
                {
                    notified++;
                    _logger.LogInformation(
                        "Sent absent notification to {Email} for ExamId={ExamId}.",
                        student.Email,
                        exam.Id);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to send absent notification to {Email} for ExamId={ExamId}.",
                        student.Email,
                        exam.Id);
                }
            }

            await db.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("Attendance updates completed. Marked Absent={Count}, Notifications sent={Notified}.",
                pendingAttendances.Count, notified);
        }

        private static TimeZoneInfo GetVietnamTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
        }

        private DateTime GetLocalExamDate(DateTime examDate)
        {
            if (examDate.Kind == DateTimeKind.Utc)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(examDate, _timeZone);
            }

            return examDate;
        }
    }
}
