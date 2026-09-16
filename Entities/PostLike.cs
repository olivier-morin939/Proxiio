using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class PostLike
    {
        public Guid PostId { get; set; }
        public Post? Post { get; set; }

        public Guid UserId { get; set; }

        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    }
}
