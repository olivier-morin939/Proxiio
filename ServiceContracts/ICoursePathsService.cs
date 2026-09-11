using Entities;
using System;
using System.Collections.Generic;
using System.Text;
using ServiceContracts.DTO;
using Entities.Enums;

namespace ServiceContracts
{
    /// <summary>
    /// This is the represenation of the data layer of the CoursePathsService.
    /// </summary>
    public interface ICoursePathsService
    {
        /// <summary>
        /// Seed some mock course paths data inside the CoursesPathService
        /// </summary>
        void SeedMockCoursePaths();

        /// <summary>
        /// Add a new course path into the CoursesPathService
        /// </summary>
        /// <param name="coursePathAddRequest">A DTO object of type CoursePathAddRequest which contains the adding informations of the CoursePath object</param>
        /// <returns>Returns an DTO object of type CoursePathResponse</returns>
        CoursePathResponse AddCoursePath(CoursePathAddRequest? coursePathAddRequest);

        /// <summary>
        /// Get all the course paths contained inside the CoursesPathService
        /// </summary>
        /// <returns>Returns a list of DTO object of type CoursePathResponse</returns>
        List<CoursePathResponse> GetAllCoursePaths();

        /// <summary>
        /// Get all the course paths contained inside the CoursesPathService filtered by properties
        /// </summary>
        /// <param name="SearchBy">The name of the properties to filter in</param>
        /// <param name="SearchString">The content of the properties to use as a filter</param>
        /// <returns>Returns a list of DTO object of type CoursePathResponse</returns>
        List<CoursePathResponse> GetFilteredCoursePaths(string? SearchBy, string? SearchString);

        /// <summary>
        /// Get all the course paths contained inside the CoursesPathService sorted by properties and SortOption
        /// </summary>
        /// <param name="coursePathsList">The current state of the CoursePathsService list</param>
        /// <param name="sortOption">The sorting option that determines in which order to sort</param>
        /// <param name="SortBy">The name of the properties to sort for</param>
        /// <returns>Returns a list of DTO object of type CoursePathResponse</returns>
        List<CoursePathResponse> GetSortedCoursePaths(List<CoursePathResponse> coursePathsList, SortOption sortOption, string? SortBy);

        /// <summary>
        /// Get the desired course paths contained inside the CoursesPathService
        /// </summary>
        /// <param name="pathId">The desired Guid of the course path to get</param>
        /// <returns>Returns an DTO object of type CoursePathResponse</returns>
        CoursePathResponse GetCoursePathByPathId(Guid pathId);

        /// <summary>
        /// Update the desired course path contained inside the CoursesPathService
        /// </summary>
        /// <param name="coursePathUpdateRequest">A DTO object of type CoursePathUpdateRequest which contains the updating informations of the CoursePath object</param>
        /// <returns>Returns an DTO object of type CoursePathResponse</returns>
        CoursePathResponse UpdateCoursePath(CoursePathUpdateRequest? coursePathUpdateRequest);

        /// <summary>
        /// Delete the desired course paths contained inside the CoursesPathService
        /// </summary>
        /// <param name="pathId">The desired Guid of the course path to delete</param>
        /// <returns>True if the operation was successful, otherwise False</returns>
        bool DeleteCoursePathByPathId(Guid pathId);





    }
}
