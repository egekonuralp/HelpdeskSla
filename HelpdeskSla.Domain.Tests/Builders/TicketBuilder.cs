using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;

namespace HelpdeskSla.Domain.Tests.Builders;

public class TicketBuilder
{
    private string _title = "Sunucu çalışmıyor";
    private string _description = "Sunucuya erişilemiyor";
    private TicketPriority _priority = TicketPriority.Medium;
    private TimeProvider? _timeProvider = null;
    private TicketStatus _targetStatus = TicketStatus.Open;

    public Ticket Build()
    {
        var ticket = new Ticket(_title, _description, _priority, _timeProvider);

        if (_targetStatus == TicketStatus.Open)
            return ticket;

        ticket.Assign(5);

        if (_targetStatus == TicketStatus.Assigned)
            return ticket;

        ticket.StartProgress();

        if (_targetStatus == TicketStatus.InProgress)
            return ticket;

        if (_targetStatus == TicketStatus.Resolved)
        {
            ticket.Resolve();
            return ticket;
        }

        throw new InvalidOperationException($"Desteklenmeyen Ticket Durumu: {_targetStatus}");

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

    public TicketBuilder AsAssigned()
    {
        _targetStatus = TicketStatus.Assigned;
        return this;
    }

    public TicketBuilder AsInProgress()
    {
        _targetStatus = TicketStatus.InProgress;
        return this;
    }

    public TicketBuilder AsResolved()
    {
        _targetStatus = TicketStatus.Resolved;
        return this;
    }
}

