namespace e360_clone.BusinessObjects
{
    public class Major
    {
        public int Id { get; set; }
        public string MajorCode { get; set; } = string.Empty;
        public string MajorName { get; set; } = string.Empty;
        public string MajorGroup { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public ICollection<Class> Classes { get; set; } = new List<Class>();
    }
}
