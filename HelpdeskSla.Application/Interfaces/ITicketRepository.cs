using HelpdeskSla.Domain.Entities;

namespace HelpdeskSla.Application.Interfaces;

public interface ITicketRepository
{
    void Add(Ticket ticket);

    Ticket? GetById(int id);

    IReadOnlyList<Ticket> GetAll();

    Task<Ticket?> GetByIdAsync(int id);
}


