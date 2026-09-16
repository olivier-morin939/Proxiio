using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class Post
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string? Title { get; set; }
        public string? Body { get; set; } 
        public List<string>? ImagesPath { get; set; } = new List<string>();
        public List<string>? AdditionalsPath { get; set; } = new List<string>();

        public List<Comment> Comments { get; set; } = new List<Comment>();

        // Relation avec la table de jointure
        public List<PostLike> Likes { get; set; } = new List<PostLike>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
