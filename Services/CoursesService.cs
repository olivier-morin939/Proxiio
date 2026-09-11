using ServiceContracts;
using ServiceContracts.DTO;
using Entities;
using System.Data;
using Services.Helpers;
namespace Services
{
    public class CoursesService : ICoursesService
    {

        private readonly List<Course> _courses = new List<Course>();


        public void SeedMockCourses()
        {
            _courses.Clear();
            _courses.Add(
                new Course(){ CourseId = Guid.NewGuid(), CourseName = "C++ professionnal developpement", CourseDescription = "professionnal c++ course."}
                );
            _courses.Add(
                new Course() { CourseId = Guid.NewGuid(), CourseName = "C# professionnal developpement", CourseDescription = "professionnal c# course." }
                );
            _courses.Add(
                new Course() { CourseId = Guid.NewGuid(), CourseName = "Python professionnal developpement", CourseDescription = "professionnal Python course." }
                );
        }

        public CourseResponse AddCourse(CourseAddRequest? courseAddRequest)
        {

            //Validates the informations

            if(courseAddRequest == null)
            {
                throw new ArgumentNullException(nameof(courseAddRequest));
            }

            if(string.IsNullOrEmpty(courseAddRequest.CourseName))
            {
                throw new ArgumentException("Course name cannot be null or empty.", nameof(courseAddRequest.CourseName));
            }

            if(string.IsNullOrEmpty(courseAddRequest.CourseDescription))
            {
                throw new ArgumentException("Course description cannot be null or empty.", nameof(courseAddRequest.CourseDescription));
            }


            if(_courses.Where(c => c.CourseName == courseAddRequest.CourseName).Count() > 0)
            {
                throw new DuplicateNameException("Course name already exists.");
            }


            // Creates a new course object
            Course newCourse = new Course()
            {
                CourseId = Guid.NewGuid(),
                CourseName = courseAddRequest.CourseName,
                CourseDescription = courseAddRequest.CourseDescription,
            };

            // Adds the new course to the list
            _courses.Add(newCourse);


            // Converts the new course object to a CourseAddResponse object and returns it
            return newCourse.ToCourseAddResponse();

        }


        public List<CourseResponse> GetAllCourses()
        {
            if(_courses == null || _courses.Count() == 0)
            {
                return new List<CourseResponse>();
            }

            return _courses.Select(c => c.ToCourseAddResponse()).ToList();
        }

        public CourseResponse GetCourseById(Guid courseId)
        {
            Course? matchingCourse = _courses.Where(c => c.CourseId == courseId).FirstOrDefault();
            
            if (matchingCourse == null)
            {
                throw new ArgumentNullException(nameof(courseId));
            }

            return matchingCourse.ToCourseAddResponse();
        }

        public CourseResponse UpdateCourse(CourseUpdateRequest? courseModifyRequest)
        {

            // Validates the informations
            if (courseModifyRequest == null) { 
            
                throw new ArgumentNullException(nameof(courseModifyRequest));
            }

            if(courseModifyRequest.CourseId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(courseModifyRequest.CourseId));
            }

            Helpers.HelpersValidation.ModelValidation(courseModifyRequest);


            // Find the matching course in the list
            int index = _courses.FindIndex(c => c.CourseId == courseModifyRequest.CourseId);
            
            if(index == -1)
            {
                throw new ArgumentNullException(nameof(courseModifyRequest.CourseId));
            }


            // Find any duplicate name in the list
            foreach (Course courseToCheck in _courses)
            {
                if (courseToCheck.CourseName == courseModifyRequest.CourseName)
                {
                    throw new DuplicateNameException("The course name already exist.");
                }

            }

            // Old object if any properties is set to null
            Course OldCourse = _courses[index];

            // Converting the update request into an Course object
            Course UpdatedCourse = new Course()
            {
                CourseId = courseModifyRequest.CourseId,
                CourseName = courseModifyRequest.CourseName == null ? OldCourse.CourseName : courseModifyRequest.CourseName,
                CourseDescription = courseModifyRequest.CourseDescription == null ? OldCourse.CourseDescription : courseModifyRequest.CourseDescription
            };

            // Updating the location of the old element with the updated element
            _courses[index] = UpdatedCourse;


            // Return the object
            return UpdatedCourse.ToCourseAddResponse();



        }

        public bool DeleteCourseById(Guid courseId)
        {
            // Find the matching course in the list
            int index = _courses.FindIndex(c => c.CourseId == courseId);

            // Course is not found
            if (index == -1)
            {
                return false;
            }

            // Delete the course from the list
            _courses.RemoveAt(index);

           
            return true;
        }
    }
}
