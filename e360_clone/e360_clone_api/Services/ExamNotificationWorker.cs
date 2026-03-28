using e360_clone.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace e360_clone_api.Services
{
    public sealed class ExamNotificationWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExamNotificationWorker> _logger;
        private readonly IExamNotificationQueue _queue;
        private readonly TimeZoneInfo _timeZone;
        private readonly ExamNotificationSettings _settings;

        public ExamNotificationWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<ExamNotificationWorker> logger,
            IExamNotificationQueue queue,
            IOptions<ExamNotificationSettings> settings)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _queue = queue;
            _timeZone = GetVietnamTimeZone();
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Exam notification worker started. Enabled={Enabled}, TimeZone={TimeZoneId}",
                _settings.Enabled,
                _timeZone.Id);

            while (!stoppingToken.IsCancellationRequested)
            {
                ExamNotificationMessage message;
                try
                {
                    message = await _queue.DequeueAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                try
                {
                    if (!_settings.Enabled)
                    {
                        _logger.LogInformation("Exam notification worker skipped (disabled).");
                        continue;
                    }
                    await HandleMessageAsync(message, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exam notification failed. ExamId={ExamId}", message.ExamId);
                }
            }
        }

        private async Task HandleMessageAsync(ExamNotificationMessage message, CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var exam = await db.Exams.FirstOrDefaultAsync(e => e.Id == message.ExamId, stoppingToken);
            if (exam == null)
            {
                _logger.LogWarning("Exam notification skipped. Exam not found. ExamId={ExamId}", message.ExamId);
                return;
            }

            if (string.Equals(exam.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Exam notification skipped. Exam cancelled. ExamId={ExamId}", exam.Id);
                return;
            }

            var subject = await db.Subjects.FirstOrDefaultAsync(s => s.Id == exam.SubjectId, stoppingToken);
            var subjectText = subject != null
                ? $"{subject.SubjectCode} - {subject.SubjectName}"
                : $"SubjectId {exam.SubjectId}";

            var room = await db.ExamRooms.FirstOrDefaultAsync(r => r.Id == exam.RoomId, stoppingToken);
            var roomText = room != null
                ? $"{room.RoomCode} - {room.RoomName}"
                : $"RoomId {exam.RoomId}";

            var students = await LoadStudentsAsync(db, exam.SubjectId, exam.ClassId, exam.AcademicYear, exam.Semester, stoppingToken);
            if (students.Count == 0)
            {
                _logger.LogWarning("Exam notification skipped. No students found. ExamId={ExamId}", exam.Id);
                return;
            }

            var examDateLocal = GetLocalExamDate(exam.ExamDate);
            var start = examDateLocal.Date.Add(exam.StartTime);
            var end = examDateLocal.Date.Add(exam.EndTime);

            var subjectLine = $"[E360] Thong bao lich thi {exam.ExamCode}";

            var sent = 0;
            var studentIds = students.Select(s => s.Id).Distinct().ToList();
            var accountEmailMap = await db.Accounts
                .Where(a => a.StudentId.HasValue && studentIds.Contains(a.StudentId.Value))
                .Where(a => !string.IsNullOrWhiteSpace(a.Email))
                .GroupBy(a => a.StudentId!.Value)
                .ToDictionaryAsync(g => g.Key, g => g.First().Email, stoppingToken);

            foreach (var student in students)
            {
                if (!accountEmailMap.TryGetValue(student.Id, out var email))
                {
                    continue;
                }

                var body = $@"
                    <p>Xin chao <strong>{student.FullName}</strong> ({student.StudentCode}),</p>
                    <p>Ban co lich thi moi:</p>
                    <ul>
                        <li><strong>Mon:</strong> {subjectText}</li>
                        <li><strong>Ky thi:</strong> {exam.ExamName} ({exam.ExamCode})</li>
                        <li><strong>Ngay thi:</strong> {start:dd/MM/yyyy}</li>
                        <li><strong>Gio thi:</strong> {start:HH:mm} - {end:HH:mm}</li>
                        <li><strong>Phong thi:</strong> {roomText}</li>
                    </ul>
                    <p>Vui long den dung gio va theo doi thong bao tu nha truong.</p>";

                if (await emailService.SendAsync(email, subjectLine, body))
                {
                    sent++;
                }
            }

            _logger.LogInformation(
                "Exam notification sent. ExamId={ExamId}, Students={Students}, Sent={Sent}",
                exam.Id,
                students.Count,
                sent);
        }

        private static async Task<List<StudentInfo>> LoadStudentsAsync(
            AppDbContext db,
            int subjectId,
            int classId,
            string academicYear,
            string semester,
            CancellationToken stoppingToken)
        {
            if (int.TryParse(semester, out var semesterValue))
            {
                var enrolled = await db.StudentSubjects
                    .Where(s => s.SubjectId == subjectId
                                && s.ClassId == classId
                                && s.AcademicYear == academicYear
                                && s.Semester == semesterValue)
                    .Select(s => s.StudentId)
                    .Distinct()
                    .ToListAsync(stoppingToken);

                if (enrolled.Count > 0)
                {
                    return await db.Students
                        .Where(s => enrolled.Contains(s.Id))
                        .Select(s => new StudentInfo(s.Id, s.StudentCode, s.FullName, s.Email))
                        .ToListAsync(stoppingToken);
                }
            }

            return await db.Students
                .Where(s => s.ClassId == classId)
                .Select(s => new StudentInfo(s.Id, s.StudentCode, s.FullName, s.Email))
                .ToListAsync(stoppingToken);
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

        private sealed record StudentInfo(int Id, string StudentCode, string FullName, string Email);
    }
}

