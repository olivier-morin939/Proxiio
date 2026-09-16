using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Entities;

namespace ServiceContracts.DTO.Posts
{
    public class UpdatePostRequest
    {
        [Required]
        public Guid Id { get; set; }

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
                Id = this.Id,
                Title = this.Title,
                Body = this.Body,
                ImagesPath = this.ImagesPath ?? new List<string>(),
                AdditionalsPath = this.AdditionalsPath ?? new List<string>()
            };
        }
    }
}
