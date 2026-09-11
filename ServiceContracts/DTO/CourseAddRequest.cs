using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ServiceContracts.DTO
{
    /// <summary>
    /// This is the required DTO parameter object to create a new object of Course Entity
    /// </summary>
    /// <returns>
    /// Returns an DTO object of type CourseReponse
    /// </returns>
    public class CourseAddRequest
    {
        [Required(ErrorMessage = "CourseName can't be blank")]
        [StringLength(80, MinimumLength = 1, ErrorMessage = "CourseDescription should be between 1 and 80 char long")]
        public string? CourseName { get; set; }

        [StringLength(255, MinimumLength = 1, ErrorMessage = "CourseDescription should be between 1 and 255 char long")]
        public string? CourseDescription { get; set; }
    }
}
