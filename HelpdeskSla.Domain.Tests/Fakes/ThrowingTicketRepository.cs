using HelpdeskSla.Application.Interfaces;
using HelpdeskSla.Domain.Entities;

namespace HelpdeskSla.Domain.Tests.Fakes;

public class ThrowingTicketRepository : ITicketRepository
{
    public void Add(Ticket ticket)
    {
        throw new InvalidOperationException(
            "Repository kayıt işlemi başarısız.");
    }
}