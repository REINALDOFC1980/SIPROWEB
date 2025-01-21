namespace SIPROSHARED.Models
{
    public class ErrorResponse
    {
        public DateTime Timestamp { get; set; }
        public string? Message { get; set; }
        public string? Details { get; set; }
    }
}
