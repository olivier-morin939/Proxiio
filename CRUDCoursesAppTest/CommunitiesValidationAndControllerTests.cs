using System;
using System.Linq;
// Avoid referencing ASP.NET Core types directly in test project to prevent assembly reference issues.
// We will only test the controller behavior indirectly by calling the method and checking returned object types by name.
using ServiceContracts.DTO.Comunities;
using Services;
// Controller tests require referencing the main project. The test project doesn't reference the web project assembly directly.
// To keep tests isolated, avoid instantiating AdminController here; instead focus on service-level behavior.
using Xunit;

namespace CRUDCoursesAppTest
{
    public class CommunitiesValidationAndControllerTests
    {
        [Fact]
        public void AddComunity_EmptyName_ThrowsArgumentException()
        {
            // Arrange
            var service = new ComunitiesService();
            var req = new AddComunityRequest() { TeacherId = Guid.NewGuid(), Name = null };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => service.AddComunity(req));
        }

        [Fact]
        public void AddComunity_NameTooLong_ThrowsArgumentException()
        {
            // Arrange
            var service = new ComunitiesService();
            string longName = new string('a', 300);
            var req = new AddComunityRequest() { TeacherId = Guid.NewGuid(), Name = longName };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => service.AddComunity(req));
        }

        [Fact]
        public void UpdateComunity_EmptyName_ThrowsArgumentException()
        {
            // Arrange
            var service = new ComunitiesService();
            service.SeedMockComunities();
            var first = service.GetAllComunities().First();
            var updateReq = new UpdateComunityRequest() { Id = first.Id, TeacherId = first.TeacherId, Name = null };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => service.UpdateComunity(updateReq));
        }

        // Controller tests require integration setup; skipping direct controller instantiation in unit tests.

        [Fact]
        public void SeedMockPosts_AddsAtLeastOnePostToFirstComunity()
        {
            var service = new ComunitiesService();
            service.SeedMockComunities();
            service.SeedMockPosts();

            var all = service.GetAllComunities();
            Assert.True(all.Count >= 1);
            Assert.True(all[0].PostsCount >= 1);
        }
    }
}
