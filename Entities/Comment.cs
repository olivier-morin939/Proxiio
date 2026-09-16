using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class Comment
    {
        public Guid Id { get; set; }

        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Clé étrangère et propriété de navigation vers le Post
        public Guid PostId { get; set; }
        public Post? Post { get; set; }

        // Clé étrangère vers l'auteur du commentaire
        public Guid UserId { get; set; }
       
    }
}
