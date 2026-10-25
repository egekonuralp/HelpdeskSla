using HelpdeskSla.Domain.Enums;

namespace HelpdeskSla.Application.Common;

public sealed class TicketQueryParameters
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public TicketStatus? Status { get; set; }

    public TicketPriority? Priority { get; set; }
}