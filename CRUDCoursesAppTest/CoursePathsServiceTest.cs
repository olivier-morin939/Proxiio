using ServiceContracts;
using Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;

namespace CRUDCoursesAppTest
{
    /// <summary>
    ///  All the unit tests for our CoursePathsService
    /// </summary>
    public class CoursePathsServiceTest
    {
        private readonly ICoursePathsService _coursePathsService;
        private readonly ITestOutputHelper _outputHelper;
        public CoursePathsServiceTest(ITestOutputHelper outputHelper)
        {
            _coursePathsService = new CoursePathsService();
            _outputHelper = outputHelper;
        }
    }
}
