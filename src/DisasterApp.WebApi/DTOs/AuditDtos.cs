namespace DisasterApp.WebApi.DTOs
{
    public class AuditLogFiltersDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public string? Severity { get; set; }
        public string? Action { get; set; }
        public string? UserId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}