using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Entities;

namespace ServiceContracts.DTO.Posts
{
    public class UpdatePostRequest
    {
        [Required(ErrorMessage = "L'id du post ne peut pas etre vide.")]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "L'id de l'utilisateur ne peut pas etre vide.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "L'id de la communaute ne peut pas etre vide.")]
        public Guid ComunityId { get; set; }

        [Required(ErrorMessage = "Le titre ne peut pas être vide")]
        [StringLength(120, MinimumLength = 1)]
        public string Title { get; set; }


        [Required(ErrorMessage = "Le coprs ne peut pas être vide")]
        [StringLength(250, MinimumLength = 1)]
        public string Body { get; set; }

        public List<string> ImagesPath { get; set; } = new List<string>();

        public List<string> AdditionalsPath { get; set; } = new List<string>();

        public Post ToPost()
        {
            return new Post()
            {
                Id = this.Id,
                CommunityId = this.ComunityId,
                UserId = this.UserId,
                Title = this.Title,
                Body = this.Body,
                ImagesPath = this.ImagesPath,
                AdditionalsPath = this.AdditionalsPath,
                ModifiedAt = DateTime.Now,
            };
        }
    }
}
