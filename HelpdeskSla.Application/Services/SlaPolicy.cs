using HelpdeskSla.Application.Common.Options;
using HelpdeskSla.Application.Interfaces;
using HelpdeskSla.Domain.Enums;
using Microsoft.Extensions.Options;

namespace HelpdeskSla.Application.Services;

public sealed class SlaPolicy : ISlaPolicy
{
    private readonly SlaOptions _options;

    public SlaPolicy(IOptions<SlaOptions> options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _options = options.Value;
    }

    public TimeSpan GetSlaTarget(TicketPriority priority)
    {
        return priority switch
        {
            TicketPriority.Low => TimeSpan.FromHours(_options.LowHours),
            TicketPriority.Medium => TimeSpan.FromHours(_options.MediumHours),
            TicketPriority.High => TimeSpan.FromHours(_options.HighHours),
            TicketPriority.Critical => TimeSpan.FromHours(_options.CriticalHours),
            _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, "Invalid ticket priority")
        };
    }
}