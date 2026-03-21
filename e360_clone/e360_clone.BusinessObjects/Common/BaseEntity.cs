namespace e360_clone.BusinessObjects.Common
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public enum RecordStatus
    {
        Active,
        Inactive,
        Deleted
    }

    public enum Gender
    {
        Male,       // Nam
        Female,     // Nữ
        Other       // Khác
    }
}
