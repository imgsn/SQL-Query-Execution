using System.ComponentModel.DataAnnotations;

namespace DatabaseQueryApp.Models
{
    public class DatabaseQueryModel
    {
        [Required(ErrorMessage = "Connection string is required")]
        [Display(Name = "Database Connection String")]
        public string ConnectionString { get; set; } = string.Empty;

        [Display(Name = "SQL Query")]
        public string SqlQuery { get; set; } = string.Empty;

        public bool IsConnected { get; set; } = false;
        public string ConnectionMessage { get; set; } = string.Empty;
        public List<Dictionary<string, object>>? QueryResults { get; set; }
        public List<string>? ColumnNames { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsSelectQuery { get; set; } = false;
        public int AffectedRows { get; set; } = 0;
        public string? ExecutionMessage { get; set; }
        public TimeSpan ExecutionTime { get; set; }
    }
}