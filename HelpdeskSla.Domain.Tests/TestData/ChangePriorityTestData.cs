using HelpdeskSla.Domain.Enums;
using System.Collections;
using System.Collections.Generic;

namespace HelpdeskSla.Domain.Tests.TestData;

public class ChangePriorityTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            TicketPriority.Low, 24
        };

        yield return new object[]
        {
            TicketPriority.High, 8
        };

        yield return new object[]
        {
            TicketPriority.Critical, 2
        };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}