using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Enums
{
    public enum ReportType
    {
        // Signalements de contenus (Posts)
        HARASSMENT,             // Harcèlement / Intimidation
        HATE_SPEECH,            // Propos haineux / Discrimination
        INAPPROPRIATE_CONTENT,  // Contenu sexuel / Violence / Nudité
        SPAM,                   // Indésirable / Publicité abusive
        MISINFORMATION,         // Fausses informations / Fake news

                                // Signalements de comptes (Users)
        FAKE_ACCOUNT,           // Faux profil / Usurpation d'identité
        UNDERAGE_USER,          // Utilisateur mineur (-13 ans)
        SCAM_FRAUD              // Arnaque / Escroquerie
    }
}
