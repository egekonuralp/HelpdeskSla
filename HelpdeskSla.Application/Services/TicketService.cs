using HelpdeskSla.Application.Interfaces;
using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;

namespace HelpdeskSla.Application.Services;

public class TicketService
{
    private readonly ITicketRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public TicketService(ITicketRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        return _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Ticket> CreateTicketAsync(string title, string description, TicketPriority priority, CancellationToken cancellationToken = default)
    {
        var ticket = new Ticket(title, description, priority);

        await _repository.AddAsync(ticket, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ticket; 
    }

    public async Task<Ticket> UpdateTicketTitleAsync(int id, string newTitle, byte[] expectedRowVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expectedRowVersion);

        if (expectedRowVersion.Length != 8)
        {
            throw new ArgumentException("RowVersion 8 byte uzunluğunda olmalıdır !!!", nameof(expectedRowVersion));
        }

        var ticket = await _repository.GetByIdForUpdateAsync(id, cancellationToken);

        if (ticket == null)
        {
            throw new KeyNotFoundException($"Ticket bulunamadı. Id: {id}");
        }

        _repository.SetOriginalRowVersion(ticket, expectedRowVersion);

        var oldTitle = ticket.Title;

        ticket.UpdateTitle(newTitle);

        var auditLog = new TicketAuditLog(ticket.Id, oldTitle, newTitle);

        await _repository.AddAuditLogAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ticket;
    }
}