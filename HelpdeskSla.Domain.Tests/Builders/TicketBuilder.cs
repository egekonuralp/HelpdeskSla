using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;

namespace HelpdeskSla.Domain.Tests.Builders;

public class TicketBuilder
{
    private string _title = "Sunucu çalışmıyor";
    private string _description = "Sunucuya erişilemiyor";
    private TicketPriority _priority = TicketPriority.Medium;
    private TimeProvider? _timeProvider = null;

    public Ticket Build()
    {
        return new Ticket(_title, _description, _priority, _timeProvider);
    }

    public TicketBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public TicketBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public TicketBuilder WithPriority(TicketPriority priority)
    {
        _priority = priority; 
        return this;
    }

    public TicketBuilder WithTimeProvider(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        return this;
    }
}

