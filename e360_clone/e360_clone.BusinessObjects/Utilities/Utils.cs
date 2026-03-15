namespace e360_clone.BusinessObjects.Utilities
{
    public static class GradeUtils
    {
        /// <summary>
        /// Tính điểm chữ từ điểm số
        /// </summary>
        public static string CalculateLetterGrade(decimal? score)
        {
            if (!score.HasValue) return "F";

            return score.Value switch
            {
                >= 8.5m => "A",
                >= 7.0m => "B",
                >= 5.5m => "C",
                >= 4.0m => "D",
                _ => "F"
            };
        }

        /// <summary>
        /// Kiểm tra điểm có đậu không
        /// </summary>
        public static bool IsPass(string letterGrade)
        {
            return letterGrade switch
            {
                "A" or "B" or "C" or "D" => true,
                _ => false
            };
        }

        /// <summary>
        /// Tính GPA từ điểm chữ
        /// </summary>
        public static decimal CalculateGPA(string letterGrade)
        {
            return letterGrade switch
            {
                "A" => 4.0m,
                "B" => 3.0m,
                "C" => 2.0m,
                "D" => 1.0m,
                _ => 0.0m
            };
        }

        /// <summary>
        /// Validate điểm số
        /// </summary>
        public static bool IsValidScore(decimal? score)
        {
            return score.HasValue && score >= 0 && score <= 10;
        }
    }

    public static class DateTimeUtils
    {
        /// <summary>
        /// Kiểm tra ngày có trong giờ làm việc không
        /// </summary>
        public static bool IsWithinWorkingHours(DateTime startTime, DateTime endTime)
        {
            var timeStart = startTime.TimeOfDay;
            var timeEnd = endTime.TimeOfDay;

            // Giờ làm việc: 7:00 - 18:00
            var workStart = new TimeSpan(7, 0, 0);
            var workEnd = new TimeSpan(18, 0, 0);

            return timeStart >= workStart && timeEnd <= workEnd;
        }

        /// <summary>
        /// Kiểm tra 2 khoảng thời gian có trùng nhau không
        /// </summary>
        public static bool IsTimeOverlap(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
        {
            return start1 < end2 && start2 < end1;
        }

        /// <summary>
        /// Tính thời lượng tính bằng phút
        /// </summary>
        public static int CalculateDuration(TimeSpan startTime, TimeSpan endTime)
        {
            return (int)(endTime - startTime).TotalMinutes;
        }

        /// <summary>
        /// Format ngày theo kiểu Việt Nam
        /// </summary>
        public static string FormatVietnamese(DateTime date)
        {
            return date.ToString("dd/MM/yyyy");
        }

        /// <summary>
        /// Format ngày giờ theo kiểu Việt Nam
        /// </summary>
        public static string FormatVietnameseDateTime(DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy HH:mm");
        }
    }

    public static class StringUtils
    {
        /// <summary>
        /// Chuẩn hóa email (trim, lowercase)
        /// </summary>
        public static string NormalizeEmail(string email)
        {
            return email?.Trim().ToLower() ?? string.Empty;
        }

        /// <summary>
        /// Chuẩn hóa mã (trim, uppercase)
        /// </summary>
        public static string NormalizeCode(string code)
        {
            return code?.Trim().ToUpper() ?? string.Empty;
        }

        /// <summary>
        /// Validate email format
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validate số điện thoại Việt Nam
        /// </summary>
        public static bool IsValidVietnamesePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Remove spaces and dashes
            phone = phone.Replace(" ", "").Replace("-", "");

            // Check if starts with 0 and has 9-10 digits after
            return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^(0[3|5|7|8|9]\d{8})$");
        }

        /// <summary>
        /// Tạo mã ngẫu nhiên
        /// </summary>
        public static string GenerateRandomCode(string prefix, int length = 6)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var randomPart = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return $"{prefix}{randomPart}";
        }
    }

    public static class PaginationUtils
    {
        /// <summary>
        /// Tính tổng số trang
        /// </summary>
        public static int CalculateTotalPages(int totalRecords, int pageSize)
        {
            if (pageSize <= 0) return 0;
            return (int)Math.Ceiling((double)totalRecords / pageSize);
        }

        /// <summary>
        /// Tính số bản ghi bỏ qua
        /// </summary>
        public static int CalculateSkip(int pageNumber, int pageSize)
        {
            return (pageNumber - 1) * pageSize;
        }

        /// <summary>
        /// Validate page number
        /// </summary>
        public static int ValidatePageNumber(int pageNumber, int totalPages)
        {
            if (pageNumber < 1) return 1;
            if (totalPages > 0 && pageNumber > totalPages) return totalPages;
            return pageNumber;
        }
    }
}
