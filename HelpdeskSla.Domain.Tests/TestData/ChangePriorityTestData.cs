using HelpdeskSla.Domain.Enums;
using Xunit;

namespace HelpdeskSla.Domain.Tests.TestData;

public class ChangePriorityTestData : TheoryData<TicketPriority, int>
{
    public ChangePriorityTestData()
    {
        Add(TicketPriority.Low, 24);
        Add(TicketPriority.High, 8);
        Add(TicketPriority.Critical, 2);
    }
}