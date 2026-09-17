using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTO.Comunities;
using Services;
using Xunit;
using System;
using CRUDCoursesApp.ViewModels;

namespace CRUDCoursesAppTest
{
    public class AdminControllerTests
    {
        [Fact]
        public void DisplaySpecificComunity_ReturnsView_WithViewModel()
        {
            // Arrange
            var comunitiesService = new ComunitiesService();
            comunitiesService.SeedMockComunities();
            var usersService = new Services.UsersService();
            var controller = new CRUDCoursesApp.Controllers.AdminController(usersService, comunitiesService);
            var comId = comunitiesService.GetAllComunities()[0].Id;

            // Act
            var result = controller.DisplaySpecificComunity(comId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ComunityDetailsViewModel>(result.Model);
        }

        [Fact]
        public void UpdateSpecificComunity_Post_RedirectsOnSuccess()
        {
            var comunitiesService = new ComunitiesService();
            comunitiesService.SeedMockComunities();
            var usersService = new Services.UsersService();
            var controller = new CRUDCoursesApp.Controllers.AdminController(usersService, comunitiesService);

            var com = comunitiesService.GetAllComunities()[0];
            var updateReq = new ServiceContracts.DTO.Comunities.UpdateComunityRequest { Id = com.Id, TeacherId = com.TeacherId, Name = "NewName", Description = "NewDesc" };

            var result = controller.UpdateSpecificComunity(updateReq, com.Id) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("DisplaySpecificComunity", result.ActionName);
        }

        [Fact]
        public void DeleteSpecificComunityConfirm_RemovesAndRedirects()
        {
            var comunitiesService = new ComunitiesService();
            comunitiesService.SeedMockComunities();
            var usersService = new Services.UsersService();
            var controller = new CRUDCoursesApp.Controllers.AdminController(usersService, comunitiesService);

            var com = comunitiesService.GetAllComunities()[0];

            var result = controller.DeleteSpecificComunityConfirm(com.Id) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("DisplayComunities", result.ActionName);
        }
    }
}
