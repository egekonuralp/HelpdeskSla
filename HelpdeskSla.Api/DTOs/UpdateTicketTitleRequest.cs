namespace HelpdeskSla.Api.DTOs
{
    public class UpdateTicketTitleRequest
    {
        public string NewTitle { get; set; } = string.Empty;

        public string RowVersion { get; set; } = string.Empty;
    }
}
