using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class Comunity
    {
        public Guid Id { get; set; }

        public Guid TeacherId { get; set; }

        public string? Name { get; set; }
        public string? Description { get; set; }

        public List<User>? Users { get; set; }
        public List<Post>? PostsList { get; set; } = new List<Post>();

    }
}
