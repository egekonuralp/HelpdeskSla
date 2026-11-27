using HelpdeskSla.Domain.Enums;

namespace HelpdeskSla.Application.Interfaces;

public interface ISlaPolicy
{
    TimeSpan GetSlaTarget(TicketPriority priority);
}