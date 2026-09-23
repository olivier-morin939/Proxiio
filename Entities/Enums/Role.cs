using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Enums
{
    public enum Role
    {
        User = 0, // Utilisateur Regulier : Peuvent rejoindrent des communautees, creer des posts et voir d'autres profils.
        Moderator = 1, // Moderateur : Peuvent rejoindrent n'importe quelles communautees, bannir des utilisateurs et modifer/supprimer des posts.
        Administrator = 2 //Administrateur : Peut tout faire.
    }
}
