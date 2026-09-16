using System;
using System.Collections.Generic;
using System.Data;
using ServiceContracts.DTO.Comunities;
using Services;
using Xunit;

namespace CRUDCoursesAppTest
{
    public class CommunitiesAdditionalTests
    {
        private readonly ComunitiesService _service = new ComunitiesService();

        [Fact]
        public void AddComunity_DuplicateName_Throws()
        {
            // Arrange
            _service.SeedMockComunities();
            var existing = _service.GetAllComunities()[0];

            var req = new AddComunityRequest() { TeacherId = Guid.NewGuid(), Name = existing.Name, Description = "dup" };

            // Act & Assert
            Assert.Throws<DuplicateNameException>(() => _service.AddComunity(req));
        }

        [Fact]
        public void UpdateComunity_ChangeNameToExisting_Throws()
        {
            // Arrange
            _service.SeedMockComunities();
            var all = _service.GetAllComunities();
            var first = all[0];
            var second = all[1];

            var updateReq = new UpdateComunityRequest() { Id = first.Id, TeacherId = first.TeacherId, Name = second.Name, Description = "trying duplicate" };

            // Act & Assert - Update should now enforce uniqueness and throw
            Assert.Throws<DuplicateNameException>(() => _service.UpdateComunity(updateReq));
        }

        [Fact]
        public void GetFilteredComunities_ByName_ReturnsMatches()
        {
            // Arrange
            _service.SeedMockComunities();

            // Act
            var results = _service.GetFilteredComunities(nameof(ComunityResponse.Name), "Community 2");

            // Assert
            Assert.NotNull(results);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.Name != null && r.Name.Contains("Community 2"));
        }

        [Fact]
        public void GetFilteredComunities_ByDescription_ReturnsMatches()
        {
            // Arrange
            _service.SeedMockComunities();

            // Act
            var results = _service.GetFilteredComunities(nameof(ComunityResponse.Description), "Description for community 3");

            // Assert
            Assert.NotNull(results);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.Description != null && r.Description.Contains("community 3", StringComparison.OrdinalIgnoreCase));
        }
    }
}
