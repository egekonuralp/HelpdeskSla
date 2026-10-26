using HelpdeskSla.Application.Interfaces;
using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;
using HelpdeskSla.Application.Common;

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

    public Task<IReadOnlyList<Ticket>> GetAllTicketsAsync(CancellationToken cancellationToken = default)
    {
        return _repository.GetAllAsync(cancellationToken);
    }

    public Task<PagedResult<Ticket>> GetPagedTicketsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        }

        if (pageSize < 0 || pageSize > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        return _repository.GetPagedAsync(pageNumber, pageSize, cancellationToken);
    }

    public Task<PagedResult<Ticket>> GetPagedTicketsAsync(TicketQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(nameof(parameters));

        if (parameters.PageNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parameters.PageNumber));
        }

        if (parameters.PageSize <= 0 || parameters.PageSize > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(parameters.PageSize));
        }

        if (parameters.Status.HasValue && !Enum.IsDefined(parameters.Status.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(parameters.Status));
        }

        if (parameters.Priority.HasValue && !Enum.IsDefined(parameters.Priority.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(parameters.Priority));
        }

        var sortBy = parameters.SortBy?.ToLowerInvariant();
        var sortDirection = parameters.SortDirection?.ToLowerInvariant();

        if (sortBy is not ("id" or "createdat"))
        {
            throw new ArgumentException("Invalid sort field.", nameof(parameters.SortBy));
        }

        if (sortDirection is not ("asc" or "desc"))
        {
            throw new ArgumentException("Invalid sort direction.", nameof(parameters.SortDirection));
        }

        return _repository.GetPagedAsync(parameters, cancellationToken);
    }

    public async Task<bool> AssignTicketAsync(int ticketId, int userId, CancellationToken cancellationToken = default)
    {
        var ticket = await _repository.GetByIdAsync(ticketId, cancellationToken);

        if (ticket is null)
        {
            return false;
        }

        ticket.Assign(userId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> StartProgressAsync(int  ticketId, CancellationToken cancellationToken = default)
    {
        var ticket = await _repository.GetByIdAsync(ticketId, cancellationToken);

        if (ticket is null)
        {
            return false;
        }

        ticket.StartProgress();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> WaitForCustomerAsync(int ticketId, CancellationToken cancellationToken = default)
    {
        var ticket = await _repository.GetByIdAsync(ticketId, cancellationToken);

        if (ticket is null)
        {
            return false;
        }

        ticket.WaitForCustomer();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}