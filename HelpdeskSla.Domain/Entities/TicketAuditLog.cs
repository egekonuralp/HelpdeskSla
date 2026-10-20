namespace HelpdeskSla.Domain.Entities;

public class TicketAuditLog
{
    public int Id { get; private set; }

    public int TicketId { get; private set; }

    public string OldTitle { get; private set; }

    public string NewTitle { get; private set; }

    public DateTimeOffset ChangedAt { get; private set; }

    private TicketAuditLog()
    {
        OldTitle = null!;
        NewTitle = null!;
    }

    public TicketAuditLog(int ticketId, string oldTitle, string newTitle, TimeProvider? timeProvider = null)
    {
        if (ticketId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ticketId));
        }

        if (string.IsNullOrWhiteSpace(oldTitle))
        {
            throw new ArgumentException("Eski başlık boş olamaz !", nameof(oldTitle));
        }

        if (string.IsNullOrWhiteSpace(newTitle))
        {
            throw new ArgumentException("Yeni başlık boş olamaz !", nameof(newTitle));
        }

        if (oldTitle == newTitle)
        {
            throw new ArgumentException("Eski başlık Yeni başlık ile aynı olamaz !", nameof(newTitle));
        }

        TicketId = ticketId;
        OldTitle = oldTitle;
        NewTitle = newTitle;

        ChangedAt = (timeProvider ?? TimeProvider.System).GetUtcNow();
    }
}
