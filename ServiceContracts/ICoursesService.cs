using ServiceContracts.DTO;

namespace ServiceContracts
{
    /// <summary>
    /// This is the represenation of the data layer of the CoursesService.
    /// </summary>
    public interface ICoursesService
    {
        /// <summary>
        /// Seed some mock data into our services
        /// </summary>
        void SeedMockCourses();

        /// <summary>
        /// Add a course to the CoursesService
        /// </summary>
        /// <param name="courseAddRequest">A DTO object of type CourseAddRequest which contains the adding informations</param>
        /// <returns>Returns an DTO object of type CourseResponse</returns>
        CourseResponse AddCourse(CourseAddRequest? courseAddRequest);

        /// <summary>
        /// Get all the courses contained inside the CoursesService
        /// </summary>
        /// <returns>Returns a list of DTO object of type CourseResponse</returns>
        List<CourseResponse> GetAllCourses();

        /// <summary>
        /// Get the desired course contained inside the CoursesService
        /// </summary>
        /// <param name="courseId">The Guid of the desired course to get</param>
        /// <returns>Returns an DTO object of type CourseResponse</returns>
        CourseResponse GetCourseById(Guid courseId);

        /// <summary>
        /// Update the desired course contained inside the CoursesService
        /// </summary>
        /// <param name="courseModifyRequest">A DTO object of type CourseUpdateRequest which contains the updating informations of the Course object</param>
        /// <returns>Returns an DTO object of type CourseResponse</returns>
        CourseResponse UpdateCourse(CourseUpdateRequest? courseModifyRequest);

        /// <summary>
        /// Delete the desired course contained inside the CoursesService
        /// </summary>
        /// <param name="courseId">The desired Guid of the course to delete</param>
        /// <returns>True in case of success, otherwise false</returns>
        bool DeleteCourseById(Guid courseId);
    }
}
