using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpdeskSla.Domain.Enums
{
    public enum SlaStatus
    {
        OnTrack = 0,
        Overdue = 1,
        Met = 2,
        Breached = 3,
    }
}
