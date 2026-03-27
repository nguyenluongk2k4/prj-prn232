namespace e360_clone.BusinessObjects.Utilities
{
    public static class TermMapper
    {
        public static string GetTermName(int semester)
        {
            return semester switch
            {
                1 => "Xuân",
                2 => "Hè",
                3 => "Thu",
                _ => $"Học kỳ {semester}"
            };
        }

        public static string GetTermName(string? semester)
        {
            if (string.IsNullOrWhiteSpace(semester))
            {
                return "Unknown";
            }

            if (int.TryParse(semester, out var value))
            {
                return GetTermName(value);
            }

            return semester.Trim() switch
            {
                "Spring" => "Xuân",
                "Summer" => "Hè",
                "Fall" => "Thu",
                _ => semester.Trim()
            };
        }

        public static string GetTermLabel(string academicYear, int semester)
        {
            var term = GetTermName(semester);
            var year = ResolveTermYear(academicYear);
            return year.HasValue ? $"{term} {year.Value}" : $"{term} {academicYear}".Trim();
        }

        public static string GetTermLabel(string? academicYear, string? semester)
        {
            var term = GetTermName(semester);
            var year = ResolveTermYear(academicYear);
            return year.HasValue ? $"{term} {year.Value}" : $"{term} {academicYear}".Trim();
        }

        private static int? ResolveTermYear(string? academicYear)
        {
            if (string.IsNullOrWhiteSpace(academicYear))
            {
                return null;
            }

            var parts = academicYear.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0)
            {
                return null;
            }

            return int.TryParse(parts[0], out var year) ? year : null;
        }
    }
}
