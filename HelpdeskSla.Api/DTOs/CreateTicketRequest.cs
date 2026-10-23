using HelpdeskSla.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace HelpdeskSla.Api.DTOs;

public sealed class CreateTicketRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public TicketPriority Priority { get; set; }
}