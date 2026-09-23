using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Entities;

namespace ServiceContracts.DTO.Comunities
{
    public class AddComunityRequest
    {
        [Required]
        public Guid TeacherId { get; set; }
        [Required(ErrorMessage = "Le nom de la communauté est requis")]
        [StringLength(120, MinimumLength = 1)]
        public string? Name { get; set; }

        [StringLength(254, MinimumLength = 1)]
        public string? Description { get; set; }

        // Optional initial posts
        public List<ServiceContracts.DTO.Posts.PostResponse>? Posts { get; set; } = new List<ServiceContracts.DTO.Posts.PostResponse>();

        public Comunity ToComunity()
        {
            var com = new Comunity()
            {
                Id = Guid.NewGuid(),
                TeacherId = this.TeacherId,
                Name = this.Name,
                Description = this.Description,
                PostsList = new List<Post>(),
                Users = new List<User>() { new User() { UserId = this.TeacherId } }
            };

            return com;
        }
    }
}
