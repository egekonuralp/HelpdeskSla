using HelpdeskSla.Domain.Enums;
using Xunit;

namespace HelpdeskSla.Domain.Tests.TestData;

public class SlaBreachTestData : TheoryData<TimeSpan, bool>
{
    public SlaBreachTestData()
    {
        Add(TimeSpan.FromHours(1), false);
        Add(TimeSpan.FromHours(2), false);
        Add(TimeSpan.FromHours(3), true);
    }
}