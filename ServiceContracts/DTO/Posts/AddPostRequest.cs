using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Entities;

namespace ServiceContracts.DTO.Posts
{
    public class AddPostRequest
    {
        [Required]
        public Guid UserId { get; set; }

        public Guid? ComunityId { get; set; }

        [Required(ErrorMessage = "Le titre ne peut pas être vide")]
        [StringLength(250, MinimumLength = 1)]
        public string? Title { get; set; }

        public string? Body { get; set; }

        public List<string>? ImagesPath { get; set; } = new List<string>();

        public List<string>? AdditionalsPath { get; set; } = new List<string>();

        public Post ToPost()
        {
            return new Post()
            {
                Id = Guid.NewGuid(),
                UserId = this.UserId,
                Title = this.Title,
                Body = this.Body,
                ImagesPath = this.ImagesPath ?? new List<string>(),
                AdditionalsPath = this.AdditionalsPath ?? new List<string>()
            };
        }
    }
}
