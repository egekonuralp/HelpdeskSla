using HelpdeskSla.Application.Interfaces;
using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;

namespace HelpdeskSla.Application.Services;

public class TicketService
{
    private readonly ITicketRepository _repository;

    public TicketService(ITicketRepository repository)
    {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
    }

    public Ticket CreateTicket(string title, string description, TicketPriority priority)
    {
        var ticket = new Ticket(title, description, priority);

        _repository.Add(ticket);

        return ticket;
    }

    public void AssignTicket(Ticket ticket, int userId)
    {
        ticket.Assign(userId);
    }

    public Ticket? GetTicketById(int id)
    {        
        return _repository.GetById(id);
    }

    public IReadOnlyList<Ticket> GetAllTickets()
    {
        return _repository.GetAll();
    }

    public Task<Ticket?> GetTicketByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _repository.GetByIdAsync(id, cancellationToken);
    }
}