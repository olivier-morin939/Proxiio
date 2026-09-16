using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Entities;

namespace ServiceContracts.DTO.Comunities
{
    public class UpdateComunityRequest
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid TeacherId { get; set; }

        [Required(ErrorMessage = "Le nom de la communauté est requis")]
        [StringLength(200, MinimumLength = 1)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public Comunity ToComunity()
        {
            return new Comunity()
            {
                Id = this.Id,
                TeacherId = this.TeacherId,
                PostsList = new List<Post>()
            };
        }
    }
}
