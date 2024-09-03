using System.ComponentModel.DataAnnotations;

namespace CP_FleetDataJob
{
    public class Log
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string? FileName { get; set; }
        public string? LogType { get; set; }
        public string? Message { get; set; }
    }
}
