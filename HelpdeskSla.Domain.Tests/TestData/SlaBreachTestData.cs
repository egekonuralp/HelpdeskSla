using System.Collections;
using System.Collections.Generic;

namespace HelpdeskSla.Domain.Tests.TestData;

public class SlaBreachTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            TimeSpan.FromHours(1),
            false
        };

        yield return new object[]
        {
            TimeSpan.FromHours(2),
            false
        };

        yield return new object[]
        {
            TimeSpan.FromHours(3),
            true
        };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}