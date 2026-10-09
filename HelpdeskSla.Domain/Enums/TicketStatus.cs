using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpdeskSla.Domain.Enums
{
    public enum TicketStatus
    {
        Open = 0,
        Assigned = 1,
        InProgress = 2,
        PendingCustomer = 3,
        Resolved = 4,
        Closed = 5
    }
}
