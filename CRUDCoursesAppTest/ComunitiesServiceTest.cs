using AutoFixture;
using Entities;
using Entities.Enums;
using FluentAssertions;
using Moq;
using Repository;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO.Comunities;
using Services;
using System.Data;
using System.Linq.Expressions;
using Xunit.Abstractions;

namespace CRUDCoursesAppTest
{
    /// <summary>
    ///  All the unit tests for our ComunitiesService
    /// </summary>
    public class ComunitiesServiceTest
    {

        private readonly IComunitiesService _comunitiesService;
        private readonly Mock<IComunitiesRepository> _comunitiesRepositoryMock;
        private readonly IComunitiesRepository _comunitiesRepository;
        private readonly ITestOutputHelper _outputHelper;
        private readonly IFixture _fixture;

        public ComunitiesServiceTest(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _comunitiesRepositoryMock = new Mock<IComunitiesRepository>();
            _comunitiesRepository = _comunitiesRepositoryMock.Object;
            _comunitiesService = new ComunitiesService(_comunitiesRepository);
            _fixture = new Fixture();

        }


        #region GetAllComunities
        [Fact]
        public async Task GetAllComunities_EmptyList_ToBeSuccessful()
        {
            // Arrange
            _comunitiesRepositoryMock
                .Setup(r => r.GetAllComunities())
                .ReturnsAsync(new List<Comunity>());

            _outputHelper.WriteLine("Mock repository configured to return empty list.");

            // Act
            var result = await _comunitiesService.GetAllComunities();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _outputHelper.WriteLine("GetAllComunities returned empty list as expected.");

        }


        [Fact]
        public async Task GetAllComunities_FullList_ToBeSuccessful()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            Guid comId = Guid.NewGuid();

            Comunity existing_com = _fixture.Build<Comunity>()
            .With(c => c.Id, comId)
            .With(c => c.TeacherId, Guid.NewGuid())
            .With(c => c.Name, "TestCommunity")
            .With(c => c.Description, "desc")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = userId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = userId, CommunityId = comId }
                })
                .Create();


            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com });


            ComunityResponse expected_com_response = existing_com.ToComunityResponse();
            _outputHelper.WriteLine("Expected:");
            _outputHelper.WriteLine($"{expected_com_response.ToString()}");

            // Act
            List<ComunityResponse> actual_communities_response = await _comunitiesService.GetAllComunities();
            ComunityResponse actual_com_response = actual_communities_response.First();

            _outputHelper.WriteLine("Expected:");
            _outputHelper.WriteLine($"{actual_com_response.ToString()}");

            // Assert
            actual_communities_response.Should().HaveCount(1);
            actual_com_response.Id.Should().Be(existing_com.Id);
            actual_com_response.UsersCount.Should().Be(1);
            actual_com_response.PostsCount.Should().Be(1);
            actual_com_response.Users.Should().HaveCount(1);
            actual_com_response.Posts.Should().HaveCount(1);

        }

        #endregion


        #region GetFilteredComunity


        [Fact]
        public async Task GetFilteredComunity_EmptyList_ToBeSuccessful()
        {

        }


        [Fact]
        public async Task GetFilteredComunity_FilteredById_ToBeSuccessful()
        {

        }


        [Fact]
        public async Task GetFilteredComunity_FilteredByName_ToBeSuccessful()
        {

        }



        [Fact]
        public async Task GetFilteredComunity_FilteredByTeacherId_ToBeSuccessful()
        {

        }


        [Fact]
        public async Task GetFilteredComunity_FilteredByDesc_ToBeSuccessful()
        {

        }

        [Fact]
        public async Task GetFilteredComunity_FilteredByUsersCount_ToBeSuccessful()
        {

        }

        [Fact]
        public async Task GetFilteredComunity_FilteredByPostsCount_ToBeSuccessful()
        {


        }


        #endregion




        #region GetComunityById

        [Fact]
        public async Task GetComunityById_IdDoesNotExist_ToBeNull()
        {

        }

        [Fact]
        public async Task GetComunityById_IdDoesxist_ToBeSuccessful()
        {

        }

        #endregion



        #region AddComunity

        [Fact]
        public async Task AddComunity_CommunityIsNull_ToBeRejected()
        {

        }

        [Fact]
        public async Task AddComunity_NamePropIsEmpty_ToBeRejected()
        {

        }

        [Fact]
        public async Task AddComunity_TeacherIdIsEmpty_ToBeRejected()
        {

        }

        [Fact]
        public async Task AddComunity_InvalidName_ToBeRejected()
        {

        }


        [Fact]
        public async Task AddComunity_InvalidDesc_ToBeRejected()
        {

        }


        [Fact]
        public async Task AddComunity_ValidObjet_ToBeSuccessful()
        {

        }


        [Fact]
        public async Task AddComunity_DuplicateObject_ToBeRejected()
        {

        }


        #endregion


        #region UpdateComunity

        [Fact]
        public async Task UpdateComunity_CommunityIsNull_ToBeRejected()
        {

        }

        [Fact]
        public async Task UpdateComunity_NamePropIsEmpty_ToBeRejected()
        {

        }

        [Fact]
        public async Task UpdateComunity_TeacherIdIsEmpty_ToBeRejected()
        {

        }

        [Fact]
        public async Task UpdateComunity_InvalidName_ToBeRejected()
        {

        }


        [Fact]
        public async Task UpdateComunity_InvalidDesc_ToBeRejected()
        {

        }


        [Fact]
        public async Task UpdateComunity_ValidObject_ToBeSuccessful()
        {

        }

        [Fact]
        public async Task UpdateComunity_DuplicateObject_ToBeRejected()
        {

        }


        #endregion



        #region DeleteComunity

        [Fact]
        public async Task DeleteComunity_IdDoesNotExist_ToBeNull()
        {

        }

        [Fact]
        public async Task DeleteComunity_IdDoesxist_ToBeSuccessful()
        {

        }

        #endregion

    }
}
