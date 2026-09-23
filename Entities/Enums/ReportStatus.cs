using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Enums
{
    public enum ReportStatus
    {
        Received,       // La signalisation a ete recu
        InTreatment,    // La signalisation est en cours de traitement
        Terminated      // La signalisation a ete traiter
    }
}
