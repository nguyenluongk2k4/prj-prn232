namespace e360_clone_api.Services
{
    public class CronJobSettings
    {
        public bool Enabled { get; set; } = true;
        public int IntervalMinutes { get; set; } = 5;
        public int CheckInGraceMinutes { get; set; } = 15;
    }
}
