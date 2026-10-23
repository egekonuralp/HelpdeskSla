using Microsoft.AspNetCore.Mvc;
using HelpdeskSla.Application.Services;
using HelpdeskSla.Api.DTOs;

namespace HelpdeskSla.Api.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly TicketService _ticketService;

    public TicketsController(TicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpPost]
    public async Task<ActionResult<TicketResponse>> Create([FromBody] CreateTicketRequest request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.CreateTicketAsync(
            request.Title,
            request.Description,
            request.Priority,
            cancellationToken);

        var response = new TicketResponse
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Priority = ticket.Priority.ToString(),
            Status = ticket.Status.ToString(),
            CreatedAt = ticket.CreatedAt
        };

        return StatusCode(StatusCodes.Status201Created, response);
    }
}