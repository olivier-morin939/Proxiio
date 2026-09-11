using System;
using System.Collections.Generic;
using System.Text;
using Entities.Enums;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CoursePathsService : ICoursePathsService
    {
        public CoursePathResponse AddCoursePath(CoursePathAddRequest? coursePathAddRequest)
        {
            throw new NotImplementedException();
        }

        public bool DeleteCoursePathByPathId(Guid pathId)
        {
            throw new NotImplementedException();
        }

        public List<CoursePathResponse> GetAllCoursePaths()
        {
            throw new NotImplementedException();
        }

        public CoursePathResponse GetCoursePathByPathId(Guid pathId)
        {
            throw new NotImplementedException();
        }

        public List<CoursePathResponse> GetFilteredCoursePaths(string? SearchBy, string? SearchString)
        {
            throw new NotImplementedException();
        }

        public List<CoursePathResponse> GetSortedCoursePaths(List<CoursePathResponse> coursePathsList, SortOption sortOption, string? SortBy)
        {
            throw new NotImplementedException();
        }

        public void SeedMockCoursePaths()
        {
            throw new NotImplementedException();
        }

        public CoursePathResponse UpdateCoursePath(CoursePathUpdateRequest? coursePathUpdateRequest)
        {
            throw new NotImplementedException();
        }
    }
}
