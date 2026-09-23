using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Entities
{
    public class Comunity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid TeacherId { get; set; }

        [StringLength(120)]
        public string? Name { get; set; }

        [StringLength(254)]
        public string? Description { get; set; }

        public List<User>? Users { get; set; }
        public List<Post>? PostsList { get; set; } = new List<Post>();

    }
}
