using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Entities
{
    public class Post
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid CommunityId { get; set; }
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(120)]
        public string Title { get; set; }

        [Required]
        [StringLength(250)]
        public string Body { get; set; } 

        public List<string> ImagesPath { get; set; } = new List<string>();
        public List<string> AdditionalsPath { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
