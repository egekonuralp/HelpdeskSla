using System.ComponentModel.DataAnnotations;

namespace HelpdeskSla.Application.Common.Options;

public sealed class SlaOptions
{
    public const string SectionName = "Sla";

    [Range(1, int.MaxValue)]
    public int LowHours { get; set; } = 24;

    [Range(1, int.MaxValue)]
    public int MediumHours { get; set; } = 12;

    [Range(1, int.MaxValue)]
    public int HighHours { get; set; } = 8;

    [Range(1, int.MaxValue)]
    public int CriticalHours { get; set; } = 2;
}