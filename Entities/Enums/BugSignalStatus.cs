using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Enums
{
    public enum BugSignalStatus
    {
        Received,       // Le bug a ete recu
        Correcting,     // Le bug est en cours de correction
        Patched         // Le bug a ete patcher
    }
}
