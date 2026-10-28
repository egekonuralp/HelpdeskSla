using Microsoft.AspNetCore.Mvc;
using HelpdeskSla.Application.Services;
using HelpdeskSla.Api.DTOs;
using HelpdeskSla.Application.Common;
using HelpdeskSla.Application.Exceptions;

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
    [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
            CreatedAt = ticket.CreatedAt,
            RowVersion = Convert.ToBase64String(ticket.RowVersion)
        };

        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, response);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new { Message = "Ticket Id must be greater than 0!!" });
        }

        var ticket = await _ticketService.GetTicketByIdAsync(id, cancellationToken);

        if (ticket == null)
        {
            return NotFound(new { Message = "Ticket not found." });
        }

        var response = new TicketResponse
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Priority = ticket.Priority.ToString(),
            Status = ticket.Status.ToString(),
            CreatedAt = ticket.CreatedAt,
            RowVersion = Convert.ToBase64String(ticket.RowVersion)
        };

        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TicketResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<TicketResponse>>> GetAll(
        [FromQuery] TicketQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        if (parameters.PageNumber <= 0 || parameters.PageSize <= 0 || parameters.PageSize > 100)
        {
            return BadRequest(new
            {
                Message = "Invalid pagination parameters."
            });
        }

        if (parameters.Status.HasValue && !Enum.IsDefined(parameters.Status.Value))
        {
            return BadRequest(new
            {
                Message = "Invalid ticket status."
            });
        }

        if (parameters.Priority.HasValue && !Enum.IsDefined(parameters.Priority.Value))
        {
            return BadRequest(new
            {
                Message = "Invalid ticket priority."
            });
        }

        var sortBy = parameters.SortBy?.ToLowerInvariant();
        var sortDirection = parameters.SortDirection?.ToLowerInvariant();

        if (sortBy is not ("id" or "createdat"))
        {
            return BadRequest(new
            {
                Message = "Invalid sort field."
            });
        }

        if (sortDirection is not ("asc" or "desc"))
        {
            return BadRequest(new
            {
                Message = "Invalid sort direction."
            });
        }

        var pagedTickets = await _ticketService.GetPagedTicketsAsync(parameters, cancellationToken);

        var responseItems = pagedTickets.Items
            .Select(ticket => new TicketResponse
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Priority = ticket.Priority.ToString(),
                Status = ticket.Status.ToString(),
                CreatedAt = ticket.CreatedAt,
                RowVersion = Convert.ToBase64String(ticket.RowVersion)
            }).ToList();

        var response = new PagedResult<TicketResponse>(
            responseItems,
            pagedTickets.PageNumber,
            pagedTickets.PageSize,
            pagedTickets.TotalCount);

        return Ok(response);
    }

    [HttpPost("{id:int}/assign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignTicketRequest request, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                Message = "Ticket Id must be greater than 0."
            });
        }

        if (request is null || request.UserId <= 0)
        {
            return BadRequest(new
            {
                Message = "User Id must be greater than 0."
            });
        }

        try
        {
            var assigned = await _ticketService.AssignTicketAsync(id, request.UserId, cancellationToken);

            if (!assigned)
            {
                return NotFound(new
                {
                    Message = "Ticket not found."
                });
            }

            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return Conflict(new
            {
                Message = "Ticket cannot be assigned in its current status."
            });
        }
    }

    [HttpPost("{id:int}/start-progress")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> StartProgress(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                Message = "Ticket Id must be greater than 0."
            });
        }

        try
        {
            var started = await _ticketService.StartProgressAsync(id, cancellationToken);

            if (!started)
            {
                return NotFound(new
                {
                    Message = "Ticket not found."
                });
            }

            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return Conflict(new
            {
                Message = "Ticket cannot start progress in its current status."
            });
        }
    }

    [HttpPost("{id:int}/wait-for-customer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> WaitForCustomer(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                Message = "Ticket Id must be greater than 0."
            });
        }

        try
        {
            var waiting = await _ticketService.WaitForCustomerAsync(id, cancellationToken);

            if (!waiting)
            {
                return NotFound(new
                {
                    Message = "Ticket not found."
                });
            }

            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return Conflict(new
            {
                Message = "Ticket cannot wait for customer in its current status."
            });
        }
    }

    [HttpPost("{id:int}/resume-progress")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ResumeProgress(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                Message = "Ticket Id must be greater than 0."
            });
        }

        try
        {
            var resume = await _ticketService.ResumeProgressAsync(id, cancellationToken);

            if (!resume)
            {
                return NotFound(new
                {
                    Message = "Ticket not found."
                });
            }

            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return Conflict(new
            {
                Message = "Ticket cannot resume progress in its current status."
            });
        }
    }

    [HttpPost("{id:int}/resolve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Resolve(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                Message = "Ticket Id must be greater than 0."
            });
        }

        try
        {
            var resolve = await _ticketService.ResolveTicketAsync(id, cancellationToken);

            if (!resolve)
            {
                return NotFound(new
                {
                    Message = "Ticket not found."
                });
            }

            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return Conflict(new
            {
                Message = "Ticket cannot resolve in its current status."
            });
        }
    }

    [HttpPost("{id:int}/close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Close(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                Message = "Ticket Id must be greater than 0."
            });
        }

        try
        {
            var close = await _ticketService.CloseTicketAsync(id, cancellationToken);

            if (!close)
            {
                return NotFound(new
                {
                    Message = "Ticket not found."
                });
            }

            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return Conflict(new
            {
                Message = "Ticket cannot close in its current status."
            });
        }
    }

    [HttpPut("{id:int}/title")]
    [ProducesResponseType(typeof(TicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TicketResponse>> UpdateTitle(int id, [FromBody] UpdateTicketTitleRequest request, CancellationToken cancellationToken)
    {
        if (id <= 0 || request == null || string.IsNullOrWhiteSpace(request.NewTitle) || string.IsNullOrWhiteSpace(request.RowVersion))
        {
            return BadRequest(new
            {
                Message = "Invalid request parameters. Id, Title or RowVersion cannot be empty."
            });
        }

        byte[] expectedRowVersion;

        try
        {
            expectedRowVersion = Convert.FromBase64String(request.RowVersion);
        }
        catch (FormatException)
        {
            return BadRequest(new
            {
                Message = "Invalid RowVersion format."
            });
        }

        if (expectedRowVersion.Length != 8)
        {
            return BadRequest(new
            {
                Message = "RowVersion must be exactly 8 bytes long."
            });
        }

        try
        {
            var ticket = await _ticketService.UpdateTicketTitleAsync(id, request.NewTitle, expectedRowVersion, cancellationToken);

            var response = new TicketResponse
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Priority = ticket.Priority.ToString(),
                Status = ticket.Status.ToString(),
                CreatedAt = ticket.CreatedAt,
                RowVersion = Convert.ToBase64String(ticket.RowVersion)
            };

            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new
            {
                Message = "Ticket not found."
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                Message = $"Domain rule violation: {ex.Message}"
            });
        }
        catch (TicketConcurrencyException ex)
        {
            return Conflict(new
            {
                Message = ex.Message
            });
        }
    }
}