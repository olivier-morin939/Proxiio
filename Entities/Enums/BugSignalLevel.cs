using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Enums
{
    public enum BugSignalLevel
    {
        low,            // Petit bug presque sans consequences
        high,           // Gros bug avec consequences visibles
        critical,       // Bug permettant potentiellement d'alterer le fonctionnement du systeme
        criticalLow,    // Bug permettant d'alterer le fonctionnement du systeme
        criticalHigh    // Bug permettant de penetrer dans notre systeme
    }
}
