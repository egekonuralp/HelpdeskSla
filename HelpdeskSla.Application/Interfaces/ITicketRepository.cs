using HelpdeskSla.Domain.Entities;

namespace HelpdeskSla.Application.Interfaces;

public interface ITicketRepository
{
    void Add(Ticket ticket);
}


