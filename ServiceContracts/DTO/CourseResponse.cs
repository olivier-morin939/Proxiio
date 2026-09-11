using System;
using System.Collections.Generic;
using System.Text;
using Entities;

namespace ServiceContracts.DTO
{
    /// <summary>
    /// This is the DTO return object of a CourseAddRequest OR CourseUpdateRequest
    /// </summary>
    public class CourseResponse
    {
        public Guid CourseId { get; set; }
        public string? CourseName { get; set; }
        public string? CourseDescription { get; set; }

        public override bool Equals(object? obj)
        {

            // Checking if the given object is null
            if(obj == null)
            {
                return false;
            }

            CourseResponse? course_response_to_compare = (CourseResponse?)obj;

            // Checking if the given casted object is the right type of object
            if(course_response_to_compare != null && course_response_to_compare.GetType() != typeof(CourseResponse))
            {
                return false;
            }

            // Comparing this object course id with the given parameter object course id
            return this.CourseId == course_response_to_compare.CourseId && this.CourseName == course_response_to_compare.CourseName && this.CourseDescription == course_response_to_compare.CourseDescription;

        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"Course id: {CourseId}, Course name: {CourseName}, Course description: {CourseDescription}";
        }


    }

    public static class CourseAddResponseExtension
    {
        public static CourseResponse ToCourseAddResponse(this Course course)
        {

            return new CourseResponse()
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                CourseDescription = course.CourseDescription
            };
        }
    }
}
