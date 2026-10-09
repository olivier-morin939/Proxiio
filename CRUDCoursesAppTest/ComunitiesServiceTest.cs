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
            // Arrange
            Guid userId = Guid.NewGuid();
            Guid com1Id = Guid.NewGuid();
            Guid com2Id = Guid.NewGuid();
            Guid com3Id = Guid.NewGuid();

            Comunity existing_com1 = _fixture.Build<Comunity>()
            .With(c => c.Id, com1Id)
            .With(c => c.TeacherId, Guid.NewGuid())
            .With(c => c.Name, "Programmation Mobile")
            .With(c => c.Description, "Une simple communautee, dedie a la programmation mobile.")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = userId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = userId, CommunityId = com1Id }
                })
                .Create();


            Comunity existing_com2 = _fixture.Build<Comunity>()
            .With(c => c.Id, com2Id)
            .With(c => c.TeacherId, Guid.NewGuid())
            .With(c => c.Name, "Developpement Architecture")
            .With(c => c.Description, "Une simple communautee, dediee au developpement d'architecture durable et scalable.")
            .With(c => c.Users, new List<User>
                {
                                new User { UserId = userId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                                new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = userId, CommunityId = com2Id }
                })
                .Create();



            Comunity existing_com3 = _fixture.Build<Comunity>()
            .With(c => c.Id, com3Id)
            .With(c => c.TeacherId, Guid.NewGuid())
            .With(c => c.Name, "Programmation Web")
            .With(c => c.Description, "Une simple communautee, dediee a la programmation web.")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = userId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = userId, CommunityId = com3Id }
                })
                .Create();



            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com1, existing_com2, existing_com3 });


            _comunitiesRepositoryMock
            .Setup(r => r.GetFilteredComunities(It.IsAny<Expression<Func<Comunity, bool>>>()))
            .ReturnsAsync(new List<Comunity> { existing_com1, existing_com2, existing_com3 });


            List<ComunityResponse> expected_com_responses = new List<ComunityResponse>() { existing_com1.ToComunityResponse(), existing_com2.ToComunityResponse(), existing_com3.ToComunityResponse() };
            _outputHelper.WriteLine("Expected:");
            foreach (ComunityResponse comResponse in expected_com_responses)
            {
                _outputHelper.WriteLine($"{comResponse.ToString()}");
            }


            // Act
            List<ComunityResponse> actual_com_response = await _comunitiesService.GetFilteredComunities(nameof(Comunity.Name), string.Empty);

            _outputHelper.WriteLine("Actual:");
            foreach (ComunityResponse comResponse in actual_com_response)
            {
                _outputHelper.WriteLine($"{comResponse.ToString()}");
            }

            // Assert
            expected_com_responses.Should().BeEquivalentTo(actual_com_response);
        }


        [Fact]
        public async Task GetFilteredComunity_FilteredById_ToBeSuccessful()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            Guid com1Id = Guid.NewGuid();
            Guid com2Id = Guid.NewGuid();
            Guid com3Id = Guid.NewGuid();

            Comunity existing_com1 = _fixture.Build<Comunity>()
            .With(c => c.Id, com1Id)
            .With(c => c.TeacherId, Guid.NewGuid())
            .With(c => c.Name, "Programmation Mobile")
            .With(c => c.Description, "Une simple communautee, dedie a la programmation mobile.")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = userId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = userId, CommunityId = com1Id }
                })
                .Create();


            Comunity existing_com2 = _fixture.Build<Comunity>()
            .With(c => c.Id, com2Id)
            .With(c => c.TeacherId, Guid.NewGuid())
            .With(c => c.Name, "Developpement Architecture")
            .With(c => c.Description, "Une simple communautee, dediee au developpement d'architecture durable et scalable.")
            .With(c => c.Users, new List<User>
                {
                                new User { UserId = userId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                                new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = userId, CommunityId = com2Id }
                })
                .Create();



            Comunity existing_com3 = _fixture.Build<Comunity>()
            .With(c => c.Id, com3Id)
            .With(c => c.TeacherId, Guid.NewGuid())
            .With(c => c.Name, "Programmation Web")
            .With(c => c.Description, "Une simple communautee, dediee a la programmation web.")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = userId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = userId, CommunityId = com3Id }
                })
                .Create();



            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com1, existing_com2, existing_com3 });


            _comunitiesRepositoryMock 
            .Setup(r => r.GetFilteredComunities(It.IsAny<Expression<Func<Comunity, bool>>>()))
            .ReturnsAsync(new List<Comunity> { existing_com1 });


            List<ComunityResponse> expected_com_responses = new List<ComunityResponse>() { existing_com1.ToComunityResponse() };
            _outputHelper.WriteLine("Expected:");
            foreach(ComunityResponse comResponse in expected_com_responses)
            {
                 _outputHelper.WriteLine($"{comResponse.ToString()}");
            }


            // Act
            List<ComunityResponse> actual_com_response = await _comunitiesService.GetFilteredComunities(nameof(Comunity.Id), com1Id.ToString());

            _outputHelper.WriteLine("Actual:");
            foreach (ComunityResponse comResponse in actual_com_response)
            {
               _outputHelper.WriteLine($"{comResponse.ToString()}");
            }

            // Assert
            expected_com_responses.Should().BeEquivalentTo(actual_com_response);
        }


        [Fact]
        public async Task GetFilteredComunity_FilteredByName_ToBeSuccessful()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            Guid com1Id = Guid.NewGuid();
            Guid com2Id = Guid.NewGuid();
            Guid com3Id = Guid.NewGuid();

            Comunity existing_com1 = _fixture.Build<Comunity>()
            .With(c => c.Id, com1Id)
            .With(c => c.TeacherId, Guid.NewGuid())
            .With(c => c.Name, "Programmation Mobile")
            .With(c => c.Description, "Une simple communautee, dedie a la programmation mobile.")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = userId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = userId, CommunityId = com1Id }
                })
                .Create();


            Comunity existing_com2 = _fixture.Build<Comunity>()
            .With(c => c.Id, com2Id)
            .With(c => c.TeacherId, Guid.NewGuid())
            .With(c => c.Name, "Developpement Architecture")
            .With(c => c.Description, "Une simple communautee, dediee au developpement d'architecture durable et scalable.")
            .With(c => c.Users, new List<User>
                {
                                new User { UserId = userId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                                new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = userId, CommunityId = com2Id }
                })
                .Create();



            Comunity existing_com3 = _fixture.Build<Comunity>()
            .With(c => c.Id, com3Id)
            .With(c => c.TeacherId, Guid.NewGuid())
            .With(c => c.Name, "Programmation Web")
            .With(c => c.Description, "Une simple communautee, dediee a la programmation web.")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = userId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = userId, CommunityId = com3Id }
                })
                .Create();



            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com1, existing_com2, existing_com3 });


            _comunitiesRepositoryMock
            .Setup(r => r.GetFilteredComunities(It.IsAny<Expression<Func<Comunity, bool>>>()))
            .ReturnsAsync(new List<Comunity> { existing_com1, existing_com3 });


            List<ComunityResponse> expected_com_responses = new List<ComunityResponse>() { existing_com1.ToComunityResponse(), existing_com3.ToComunityResponse() };
            _outputHelper.WriteLine("Expected:");
            foreach (ComunityResponse comResponse in expected_com_responses)
            {
                _outputHelper.WriteLine($"{comResponse.ToString()}");
            }


            // Act
            List<ComunityResponse> actual_com_response = await _comunitiesService.GetFilteredComunities(nameof(Comunity.Name), "programmation");

            _outputHelper.WriteLine("Actual:");
            foreach (ComunityResponse comResponse in actual_com_response)
            {
                _outputHelper.WriteLine($"{comResponse.ToString()}");
            }

            // Assert
            expected_com_responses.Should().BeEquivalentTo(actual_com_response);
        }


        [Fact]
        public async Task GetFilteredComunity_FilteredByTeacherId_ToBeSuccessful()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            Guid com1Id = Guid.NewGuid();
            Guid creator1Id = Guid.NewGuid();
            Guid creator2Id = Guid.NewGuid();
            Guid com2Id = Guid.NewGuid();
            Guid com3Id = Guid.NewGuid();


            Comunity existing_com1 = _fixture.Build<Comunity>()
            .With(c => c.Id, com1Id)
            .With(c => c.TeacherId, creator1Id)
            .With(c => c.Name, "Programmation Mobile")
            .With(c => c.Description, "Une simple communautee, dedie a la programmation mobile.")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = userId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = creator1Id, Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = userId, CommunityId = com1Id }
                })
                .Create();


            Comunity existing_com2 = _fixture.Build<Comunity>()
            .With(c => c.Id, com2Id)
            .With(c => c.TeacherId, creator1Id)
            .With(c => c.Name, "Developpement Architecture")
            .With(c => c.Description, "Une simple communautee, dediee au developpement d'architecture durable et scalable.")
            .With(c => c.Users, new List<User>
                {
                                new User { UserId = userId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                                new Post { Id = creator1Id, Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = userId, CommunityId = com2Id }
                })
                .Create();



            Comunity existing_com3 = _fixture.Build<Comunity>()
            .With(c => c.Id, com3Id)
            .With(c => c.TeacherId, creator2Id)
            .With(c => c.Name, "Programmation Web")
            .With(c => c.Description, "Une simple communautee, dediee a la programmation web.")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = userId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = creator2Id, Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = userId, CommunityId = com3Id }
                })
                .Create();



            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com1, existing_com2, existing_com3 });


            _comunitiesRepositoryMock
            .Setup(r => r.GetFilteredComunities(It.IsAny<Expression<Func<Comunity, bool>>>()))
            .ReturnsAsync(new List<Comunity> { existing_com1, existing_com2 });


            List<ComunityResponse> expected_com_responses = new List<ComunityResponse>() { existing_com1.ToComunityResponse(), existing_com2.ToComunityResponse() };
            _outputHelper.WriteLine("Expected:");
            foreach (ComunityResponse comResponse in expected_com_responses)
            {
                _outputHelper.WriteLine($"{comResponse.ToString()}");
            }


            // Act
            List<ComunityResponse> actual_com_response = await _comunitiesService.GetFilteredComunities(nameof(Comunity.TeacherId), creator1Id.ToString());

            _outputHelper.WriteLine("Actual:");
            foreach (ComunityResponse comResponse in actual_com_response)
            {
                _outputHelper.WriteLine($"{comResponse.ToString()}");
            }

            // Assert
            expected_com_responses.Should().BeEquivalentTo(actual_com_response);
        }


        [Fact]
        public async Task GetFilteredComunity_FilteredByDesc_ToBeSuccessful()
        {
            // Arrange
            Guid userId = Guid.NewGuid();
            Guid com1Id = Guid.NewGuid();
            Guid com2Id = Guid.NewGuid();
            Guid com3Id = Guid.NewGuid();

            Comunity existing_com1 = _fixture.Build<Comunity>()
            .With(c => c.Id, com1Id)
            .With(c => c.TeacherId, Guid.NewGuid())
            .With(c => c.Name, "Programmation Mobile")
            .With(c => c.Description, "Une simple communautee, dedie a la programmation mobile.")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = userId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = userId, CommunityId = com1Id }
                })
                .Create();


            Comunity existing_com2 = _fixture.Build<Comunity>()
            .With(c => c.Id, com2Id)
            .With(c => c.TeacherId, Guid.NewGuid())
            .With(c => c.Name, "Developpement Architecture")
            .With(c => c.Description, "Une simple communautee, dediee au developpement d'architecture durable et scalable.")
            .With(c => c.Users, new List<User>
                {
                                new User { UserId = userId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                                new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = userId, CommunityId = com2Id }
                })
                .Create();



            Comunity existing_com3 = _fixture.Build<Comunity>()
            .With(c => c.Id, com3Id)
            .With(c => c.TeacherId, Guid.NewGuid())
            .With(c => c.Name, "Programmation Web")
            .With(c => c.Description, "Une simple communautee, dediee a la programmation web.")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = userId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = userId, CommunityId = com3Id }
                })
                .Create();



            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com1, existing_com2, existing_com3 });


            _comunitiesRepositoryMock
            .Setup(r => r.GetFilteredComunities(It.IsAny<Expression<Func<Comunity, bool>>>()))
            .ReturnsAsync(new List<Comunity> { existing_com1, existing_com3 });


            List<ComunityResponse> expected_com_responses = new List<ComunityResponse>() { existing_com1.ToComunityResponse(), existing_com3.ToComunityResponse() };
            _outputHelper.WriteLine("Expected:");
            foreach (ComunityResponse comResponse in expected_com_responses)
            {
                _outputHelper.WriteLine($"{comResponse.ToString()}");
            }


            // Act
            List<ComunityResponse> actual_com_response = await _comunitiesService.GetFilteredComunities(nameof(Comunity.Description), "programmation");

            _outputHelper.WriteLine("Actual:");
            foreach (ComunityResponse comResponse in actual_com_response)
            {
                _outputHelper.WriteLine($"{comResponse.ToString()}");
            }

            // Assert
            expected_com_responses.Should().BeEquivalentTo(actual_com_response);
        }

        [Fact]
        public async Task GetFilteredComunity_FilteredByUsersCount_ToBeSuccessful()
        {
            // Arrange
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid com1Id = Guid.NewGuid();
            Guid com2Id = Guid.NewGuid();
            Guid com3Id = Guid.NewGuid();

            Comunity existing_com1 = _fixture.Build<Comunity>()
            .With(c => c.Id, com1Id)
            .With(c => c.TeacherId, Guid.NewGuid())
            .With(c => c.Name, "Programmation Mobile")
            .With(c => c.Description, "Une simple communautee, dedie a la programmation mobile.")
            .With(c => c.Users, new List<User>
                {
                    new User { 
                        UserId = user1Id,
                        Name = "Alice",
                        Email = "a@b.c",
                        DateOfBirth = DateTime.UtcNow.AddYears(-25),
                        ReceiveNewsLetter = true
                    },
                    new User
                    {
                        UserId = user2Id,
                        Name = "Bob",
                        Email = "b@a.c",
                        DateOfBirth = DateTime.UtcNow.AddYears(-25),
                        ReceiveNewsLetter = true
                    }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = user1Id, CommunityId = com1Id }
                })
                .Create();


            Comunity existing_com2 = _fixture.Build<Comunity>()
            .With(c => c.Id, com2Id)
            .With(c => c.TeacherId, Guid.NewGuid())
            .With(c => c.Name, "Developpement Architecture")
            .With(c => c.Description, "Une simple communautee, dediee au developpement d'architecture durable et scalable.")
            .With(c => c.Users, new List<User>
                {
                                new User { UserId = user1Id, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                                new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = user1Id, CommunityId = com2Id }
                })
                .Create();



            Comunity existing_com3 = _fixture.Build<Comunity>()
            .With(c => c.Id, com3Id)
            .With(c => c.TeacherId, Guid.NewGuid())
            .With(c => c.Name, "Programmation Web")
            .With(c => c.Description, "Une simple communautee, dediee a la programmation web.")
            .With(c => c.Users, new List<User>
                {
                    new User {
                        UserId = user1Id,
                        Name = "Alice",
                        Email = "a@b.c",
                        DateOfBirth = DateTime.UtcNow.AddYears(-25),
                        ReceiveNewsLetter = true
                    },
                    new User
                    {
                        UserId = user2Id,
                        Name = "Bob",
                        Email = "b@a.c",
                        DateOfBirth = DateTime.UtcNow.AddYears(-25),
                        ReceiveNewsLetter = true
                    }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = user1Id, CommunityId = com3Id }
                })
                .Create();



            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com1, existing_com2, existing_com3 });


            _comunitiesRepositoryMock
            .Setup(r => r.GetFilteredComunities(It.IsAny<Expression<Func<Comunity, bool>>>()))
            .ReturnsAsync(new List<Comunity> { existing_com1, existing_com3 });


            List<ComunityResponse> expected_com_responses = new List<ComunityResponse>() { existing_com1.ToComunityResponse(), existing_com3.ToComunityResponse() };
            _outputHelper.WriteLine("Expected:");
            foreach (ComunityResponse comResponse in expected_com_responses)
            {
                _outputHelper.WriteLine($"{comResponse.ToString()}");
            }


            // Act
            List<ComunityResponse> actual_com_response = await _comunitiesService.GetFilteredComunities(nameof(ComunityResponse.UsersCount), expected_com_responses.Count().ToString());

            _outputHelper.WriteLine("Actual:");
            foreach (ComunityResponse comResponse in actual_com_response)
            {
                _outputHelper.WriteLine($"{comResponse.ToString()}");
            }

            // Assert
            expected_com_responses.Should().BeEquivalentTo(actual_com_response);
   
        }

        [Fact]
        public async Task GetFilteredComunity_FilteredByPostsCount_ToBeSuccessful()
        {

            // Arrange
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid post1Id = Guid.NewGuid();
            Guid post2Id = Guid.NewGuid();
            Guid com1Id = Guid.NewGuid();
            Guid com2Id = Guid.NewGuid();
            Guid com3Id = Guid.NewGuid();

            Comunity existing_com1 = _fixture.Build<Comunity>()
            .With(c => c.Id, com1Id)
            .With(c => c.TeacherId, Guid.NewGuid())
            .With(c => c.Name, "Programmation Mobile")
            .With(c => c.Description, "Une simple communautee, dedie a la programmation mobile.")
            .With(c => c.Users, new List<User>
                {
                    new User {
                        UserId = user1Id,
                        Name = "Alice",
                        Email = "a@b.c",
                        DateOfBirth = DateTime.UtcNow.AddYears(-25),
                        ReceiveNewsLetter = true
                    },
                    new User
                    {
                        UserId = user2Id,
                        Name = "Bob",
                        Email = "b@a.c",
                        DateOfBirth = DateTime.UtcNow.AddYears(-25),
                        ReceiveNewsLetter = true
                    }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { 
                        Id = post1Id,
                        Title = "Post1",
                        Body = "Body",
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow,
                        UserId = user1Id,
                        CommunityId = com1Id
                    },
                    new Post {
                        Id = post2Id,
                        Title = "Post2",
                        Body = "Body",
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow,
                        UserId = user2Id,
                        CommunityId = com1Id
                    }
                })
                .Create();


            Comunity existing_com2 = _fixture.Build<Comunity>()
            .With(c => c.Id, com2Id)
            .With(c => c.TeacherId, Guid.NewGuid())
            .With(c => c.Name, "Developpement Architecture")
            .With(c => c.Description, "Une simple communautee, dediee au developpement d'architecture durable et scalable.")
            .With(c => c.Users, new List<User>
                {
                                new User { UserId = user1Id, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                                new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = user1Id, CommunityId = com2Id }
                })
                .Create();



            Comunity existing_com3 = _fixture.Build<Comunity>()
            .With(c => c.Id, com3Id)
            .With(c => c.TeacherId, Guid.NewGuid())
            .With(c => c.Name, "Programmation Web")
            .With(c => c.Description, "Une simple communautee, dediee a la programmation web.")
            .With(c => c.Users, new List<User>
                {
                    new User {
                        UserId = user1Id,
                        Name = "Alice",
                        Email = "a@b.c",
                        DateOfBirth = DateTime.UtcNow.AddYears(-25),
                        ReceiveNewsLetter = true
                    },
                    new User
                    {
                        UserId = user2Id,
                        Name = "Bob",
                        Email = "b@a.c",
                        DateOfBirth = DateTime.UtcNow.AddYears(-25),
                        ReceiveNewsLetter = true
                    }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post {
                        Id = post1Id,
                        Title = "Post1",
                        Body = "Body",
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow,
                        UserId = user1Id,
                        CommunityId = com1Id
                    },
                    new Post {
                        Id = post2Id,
                        Title = "Post2",
                        Body = "Body",
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow,
                        UserId = user2Id,
                        CommunityId = com1Id
                    }
                })
                .Create();



            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com1, existing_com2, existing_com3 });


            _comunitiesRepositoryMock
            .Setup(r => r.GetFilteredComunities(It.IsAny<Expression<Func<Comunity, bool>>>()))
            .ReturnsAsync(new List<Comunity> { existing_com1, existing_com3 });


            List<ComunityResponse> expected_com_responses = new List<ComunityResponse>() { existing_com1.ToComunityResponse(), existing_com3.ToComunityResponse() };
            _outputHelper.WriteLine("Expected:");
            foreach (ComunityResponse comResponse in expected_com_responses)
            {
                _outputHelper.WriteLine($"{comResponse.ToString()}");
            }


            // Act
            List<ComunityResponse> actual_com_response = await _comunitiesService.GetFilteredComunities(nameof(ComunityResponse.PostsCount), expected_com_responses.Count().ToString());

            _outputHelper.WriteLine("Actual:");
            foreach (ComunityResponse comResponse in actual_com_response)
            {
                _outputHelper.WriteLine($"{comResponse.ToString()}");
            }

            // Assert
            expected_com_responses.Should().BeEquivalentTo(actual_com_response);
        }


        #endregion

        #region GetComunityById

        [Fact]
        public async Task GetComunityById_IdDoesNotExist_ToBeNull()
        {
            // Arrange
            _comunitiesRepositoryMock
            .Setup(r => r.GetComunityById(It.IsAny<Guid>()))
            .ReturnsAsync(null as Comunity);


            _outputHelper.WriteLine("InvalidGuid should return null");

            // Act
            ComunityResponse actual_com_response = await _comunitiesService.GetComunityByComId(Guid.NewGuid());

            // Assert
            actual_com_response.Should().BeNull();
        }

        [Fact]
        public async Task GetComunityById_IdDoesxist_ToBeSuccessful()
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
            .Setup(r => r.GetComunityById(It.IsAny<Guid>()))
            .ReturnsAsync(existing_com);


            ComunityResponse expected_com_response = existing_com.ToComunityResponse();
            _outputHelper.WriteLine("Expected:");
            _outputHelper.WriteLine($"{expected_com_response.ToString()}");

            // Act
            ComunityResponse actual_com_response = await _comunitiesService.GetComunityByComId(comId);

            _outputHelper.WriteLine("Expected:");
            _outputHelper.WriteLine($"{actual_com_response.ToString()}");

            // Assert
            actual_com_response.Id.Should().Be(existing_com.Id);
            actual_com_response.UsersCount.Should().Be(1);
            actual_com_response.PostsCount.Should().Be(1);
            actual_com_response.Users.Should().HaveCount(1);
            actual_com_response.Posts.Should().HaveCount(1);
        }

        #endregion

        #region AddComunity

        [Fact]
        public async Task AddComunity_CommunityIsNull_ToBeRejected()
        {

            // Arrange
            Guid Com1Id = Guid.NewGuid();
            Guid Com2Id = Guid.NewGuid();
            Guid CreatorId = Guid.NewGuid();
            Comunity existing_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity1")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();

            Comunity newly_added_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com2Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity2")
            .With(c => c.Description, "desc2")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com2Id }
                })
                .Create();

            ComunityResponse expected_com_response = newly_added_com.ToComunityResponse();

            AddComunityRequest? add_com_request = null; // Simulating a null request
            _outputHelper.WriteLine("AddComunityRequest is null, simulating invalid input.");

            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com });

            _comunitiesRepositoryMock
            .Setup(r => r.AddComunity(It.IsAny<Comunity>(), It.IsAny<Guid>()))
            .ReturnsAsync(newly_added_com);


            // Prepared Act
            var Action = async () =>
            {
                ComunityResponse actual_com_response = await _comunitiesService.AddComunity(add_com_request);
            };

            // Assert
            await Action.Should().ThrowAsync<ArgumentNullException>();
            _outputHelper.WriteLine("AddComunity threw ArgumentNullException as expected for null input.");

        }

        [Fact]
        public async Task AddComunity_NamePropIsEmpty_ToBeRejected()
        {
            // Arrange
            Guid Com1Id = Guid.NewGuid();
            Guid Com2Id = Guid.NewGuid();
            Guid CreatorId = Guid.NewGuid();
            Comunity existing_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity1")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();

            Comunity newly_added_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com2Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity2")
            .With(c => c.Description, "desc2")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com2Id }
                })
                .Create();

            ComunityResponse expected_com_response = newly_added_com.ToComunityResponse();

            AddComunityRequest add_com_request = _fixture.Build<AddComunityRequest>()
                .With(r => r.TeacherId, CreatorId)
                .With(r => r.Name, null as string)
                .With(r => r.Description, "desc2")
                .Create();

            _outputHelper.WriteLine("AddComunityRequest has empty Name property, simulating invalid input.");
            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com });

            _comunitiesRepositoryMock
            .Setup(r => r.AddComunity(It.IsAny<Comunity>(), It.IsAny<Guid>()))
            .ReturnsAsync(newly_added_com);


            // Prepared Act
            var Action = async () =>
            {
                ComunityResponse actual_com_response = await _comunitiesService.AddComunity(add_com_request);
            };


            // Assert
            await Action.Should().ThrowAsync<ArgumentException>();
            _outputHelper.WriteLine("AddComunity threw ArgumentException as expected for empty Name property.");
        }

        [Fact]
        public async Task AddComunity_TeacherIdIsEmpty_ToBeRejected()
        {
            // Arrange
            Guid Com1Id = Guid.NewGuid();
            Guid Com2Id = Guid.NewGuid();
            Guid CreatorId = Guid.NewGuid();
            Comunity existing_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity1")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();

            Comunity newly_added_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com2Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity2")
            .With(c => c.Description, "desc2")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com2Id }
                })
                .Create();

            ComunityResponse expected_com_response = newly_added_com.ToComunityResponse();

            AddComunityRequest add_com_request = _fixture.Build<AddComunityRequest>()
                .With(r => r.TeacherId, Guid.Empty)
                .With(r => r.Name, "TestCommunity2")
                .With(r => r.Description, "desc2")
                .Create();

            _outputHelper.WriteLine("AddComunityRequest has empty TeacherId property, simulating invalid input.");
            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com });

            _comunitiesRepositoryMock
            .Setup(r => r.AddComunity(It.IsAny<Comunity>(), It.IsAny<Guid>()))
            .ReturnsAsync(newly_added_com);


            // Prepared Act
            var Action = async () =>
            {
                ComunityResponse actual_com_response = await _comunitiesService.AddComunity(add_com_request);
            };


            // Assert
            await Action.Should().ThrowAsync<ArgumentException>();
            _outputHelper.WriteLine("AddComunity threw ArgumentException as expected for empty TeacherId property.");
        }

        [Fact]
        public async Task AddComunity_InvalidName_ToBeRejected()
        {
            // Arrange
            Guid Com1Id = Guid.NewGuid();
            Guid Com2Id = Guid.NewGuid();
            Guid CreatorId = Guid.NewGuid();
            Comunity existing_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity1")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();

            Comunity newly_added_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com2Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity2")
            .With(c => c.Description, "desc2")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com2Id }
                })
                .Create();

            ComunityResponse expected_com_response = newly_added_com.ToComunityResponse();

            AddComunityRequest add_com_request = _fixture.Build<AddComunityRequest>()
                .With(r => r.TeacherId, CreatorId)
                .With(r => r.Name, @"LONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONG
                                     LONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONG
                                     LONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONG")
                .With(r => r.Description, "desc2")
                .Create();

            _outputHelper.WriteLine("AddComunityRequest has invalid Name property, simulating invalid input.");
            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com });

            _comunitiesRepositoryMock
            .Setup(r => r.AddComunity(It.IsAny<Comunity>(), It.IsAny<Guid>()))
            .ReturnsAsync(newly_added_com);


            // Prepared Act
            var Action = async () =>
            {
                ComunityResponse actual_com_response = await _comunitiesService.AddComunity(add_com_request);
            };


            // Assert
            await Action.Should().ThrowAsync<ArgumentException>();
            _outputHelper.WriteLine("AddComunity threw ArgumentException as expected for invalid Name property.");
        }


        [Fact]
        public async Task AddComunity_InvalidDesc_ToBeRejected()
        {
            // Arrange
            Guid Com1Id = Guid.NewGuid();
            Guid Com2Id = Guid.NewGuid();
            Guid CreatorId = Guid.NewGuid();
            Comunity existing_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity1")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();

            Comunity newly_added_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com2Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity2")
            .With(c => c.Description, "desc2")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com2Id }
                })
                .Create();

            ComunityResponse expected_com_response = newly_added_com.ToComunityResponse();

            AddComunityRequest add_com_request = _fixture.Build<AddComunityRequest>()
                .With(r => r.TeacherId, CreatorId)
                .With(r => r.Name, "TestCommunity2")
                .With(r => r.Description, @"LONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONG
                                            LONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONG
                                            LONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONG
                                            LONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONG
                                            LONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONG")
                .Create();

            _outputHelper.WriteLine("AddComunityRequest has invalid Desc property, simulating invalid input.");
            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com });

            _comunitiesRepositoryMock
            .Setup(r => r.AddComunity(It.IsAny<Comunity>(), It.IsAny<Guid>()))
            .ReturnsAsync(newly_added_com);


            // Prepared Act
            var Action = async () =>
            {
                ComunityResponse actual_com_response = await _comunitiesService.AddComunity(add_com_request);
            };


            // Assert
            await Action.Should().ThrowAsync<ArgumentException>();
            _outputHelper.WriteLine("AddComunity threw ArgumentException as expected for invalid Desc property.");
        }


        [Fact]
        public async Task AddComunity_ValidObjet_ToBeSuccessful()
        {
            // Arrange
            Guid Com1Id = Guid.NewGuid();
            Guid Com2Id = Guid.NewGuid();
            Guid CreatorId = Guid.NewGuid();
            Comunity existing_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity1")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();

            Comunity newly_added_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com2Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity2")
            .With(c => c.Description, "desc2")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com2Id }
                })
                .Create();

            ComunityResponse expected_com_response = newly_added_com.ToComunityResponse();

            AddComunityRequest add_com_request = _fixture.Build<AddComunityRequest>()
                .With(r => r.TeacherId, CreatorId)
                .With(r => r.Name, "TestCommunity2")
                .With(r => r.Description, "desc2")
                .Create();

            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com });

            _comunitiesRepositoryMock
            .Setup(r => r.AddComunity(It.IsAny<Comunity>(), It.IsAny<Guid>()))
            .ReturnsAsync(newly_added_com);


            _outputHelper.WriteLine("Expected:");
            _outputHelper.WriteLine($"{expected_com_response.ToString()}");



            // Act
            ComunityResponse actual_com_response = await _comunitiesService.AddComunity(add_com_request);
            actual_com_response.Id = newly_added_com.Id;


            _outputHelper.WriteLine("Actual:");
            _outputHelper.WriteLine($"{actual_com_response.ToString()}");

            // Assert
            expected_com_response.Should().BeEquivalentTo(actual_com_response);
        }


        [Fact]
        public async Task AddComunity_DuplicateObject_ToBeRejected()
        {
            // Arrange
            Guid Com1Id = Guid.NewGuid();
            Guid Com2Id = Guid.NewGuid();
            Guid CreatorId = Guid.NewGuid();
            Comunity existing_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity1")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();

            Comunity newly_added_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity1")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();

            ComunityResponse expected_com_response = newly_added_com.ToComunityResponse();

            AddComunityRequest add_com_request = _fixture.Build<AddComunityRequest>()
                .With(r => r.TeacherId, CreatorId)
                .With(r => r.Name, "TestCommunity1")
                .With(r => r.Description, "desc1")
                .Create();
            _outputHelper.WriteLine("AddComunityRequest is a duplicate of an existing community, simulating invalid input.");
            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com });

            _comunitiesRepositoryMock
            .Setup(r => r.AddComunity(It.IsAny<Comunity>(), It.IsAny<Guid>()))
            .ReturnsAsync(newly_added_com);

            // Act
            var Action = async () =>
            {
                ComunityResponse actual_com_response = await _comunitiesService.AddComunity(add_com_request);
            };

            // Assert
            await Action.Should().ThrowAsync<DuplicateNameException>();
            _outputHelper.WriteLine("AddComunity threw DuplicateNameException as expected for duplicate input.");
        }


        #endregion

        #region UpdateComunity

        [Fact]
        public async Task UpdateComunity_CommunityIsNull_ToBeRejected()
        {
            // Arrange
            Guid Com1Id = Guid.NewGuid();
            Guid CreatorId = Guid.NewGuid();
            Comunity existing_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity1")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();

            Comunity newly_updated_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity2")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();

            ComunityResponse expected_com_response = newly_updated_com.ToComunityResponse();

            UpdateComunityRequest? update_com_request = null; // Simulating a null request
            _outputHelper.WriteLine("UpdateComunityRequest is null, simulating invalid input.");

            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com });

            _comunitiesRepositoryMock
            .Setup(r => r.GetComunityById(It.IsAny<Guid>()))
            .ReturnsAsync(existing_com);


            _comunitiesRepositoryMock
            .Setup(r => r.UpdateComunity(It.IsAny<Comunity>()))
            .ReturnsAsync(newly_updated_com);


            // Prepared Act
            var Action = async () =>
            {
                ComunityResponse actual_com_response = await _comunitiesService.UpdateComunity(update_com_request);
            };

            // Assert
            await Action.Should().ThrowAsync<ArgumentNullException>();
            _outputHelper.WriteLine("UpdateComunity threw ArgumentNullException as expected for null input.");
        }

        [Fact]
        public async Task UpdateComunity_NamePropIsEmpty_ToBeRejected()
        {
            // Arrange
            Guid Com1Id = Guid.NewGuid();
            Guid CreatorId = Guid.NewGuid();
            Comunity existing_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity1")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();

            Comunity newly_updated_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity2")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();

            ComunityResponse expected_com_response = newly_updated_com.ToComunityResponse();

            UpdateComunityRequest? update_com_request = _fixture.Build<UpdateComunityRequest>()
            .With(temp => temp.Id, Com1Id)
            .With(temp => temp.TeacherId, CreatorId)
            .With(temp => temp.Name, null as string) // Simulating a invalid property
            .Create();                              
            _outputHelper.WriteLine("UpdateComunityRequest is null, simulating invalid input.");

            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com });

            _comunitiesRepositoryMock
            .Setup(r => r.GetComunityById(It.IsAny<Guid>()))
            .ReturnsAsync(existing_com);

            _comunitiesRepositoryMock
            .Setup(r => r.UpdateComunity(It.IsAny<Comunity>()))
            .ReturnsAsync(newly_updated_com);


            // Prepared Act
            var Action = async () =>
            {
                ComunityResponse actual_com_response = await _comunitiesService.UpdateComunity(update_com_request);
            };

            // Assert
            await Action.Should().ThrowAsync<ArgumentException>();
            _outputHelper.WriteLine("UpdateComunity threw ArgumentNullException as expected for null input.");
        }

        [Fact]
        public async Task UpdateComunity_TeacherIdIsEmpty_ToBeRejected()
        {
            // Arrange
            Guid Com1Id = Guid.NewGuid();
            Guid CreatorId = Guid.NewGuid();
            Comunity existing_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity1")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();

            Comunity newly_updated_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity2")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();

            ComunityResponse expected_com_response = newly_updated_com.ToComunityResponse();

            UpdateComunityRequest? update_com_request = _fixture.Build<UpdateComunityRequest>()
            .With(temp => temp.Id, Com1Id)
            .With(temp => temp.TeacherId, Guid.Empty) // Simulating a invalid property
            .With(temp => temp.Name, "TestCommunity2") 
            .Create();
            _outputHelper.WriteLine("UpdateComunityRequest is null, simulating invalid input.");

            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com });


            _comunitiesRepositoryMock
            .Setup(r => r.GetComunityById(It.IsAny<Guid>()))
            .ReturnsAsync(existing_com);


            _comunitiesRepositoryMock
            .Setup(r => r.UpdateComunity(It.IsAny<Comunity>()))
            .ReturnsAsync(newly_updated_com);


            // Prepared Act
            var Action = async () =>
            {
                ComunityResponse actual_com_response = await _comunitiesService.UpdateComunity(update_com_request);
            };

            // Assert
            await Action.Should().ThrowAsync<ArgumentException>();
            _outputHelper.WriteLine("UpdateComunity threw ArgumentNullException as expected for null input.");
        }

        [Fact]
        public async Task UpdateComunity_InvalidName_ToBeRejected()
        {
            // Arrange
            Guid Com1Id = Guid.NewGuid();
            Guid CreatorId = Guid.NewGuid();
            Comunity existing_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity1")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();

            Comunity newly_updated_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity2")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();

            ComunityResponse expected_com_response = newly_updated_com.ToComunityResponse();

            UpdateComunityRequest? update_com_request = _fixture.Build<UpdateComunityRequest>()
            .With(temp => temp.Id, Com1Id)
            .With(temp => temp.TeacherId, CreatorId) 
            .With(temp => temp.Name, @"LONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONG
                                       LONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONG
                                       LONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONG
                                       LONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONG") 
            .Create(); // Simulating a invalid property
            _outputHelper.WriteLine("UpdateComunityRequest is null, simulating invalid input.");

            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com });

            _comunitiesRepositoryMock
            .Setup(r => r.GetComunityById(It.IsAny<Guid>()))
            .ReturnsAsync(existing_com);

            _comunitiesRepositoryMock
            .Setup(r => r.UpdateComunity(It.IsAny<Comunity>()))
            .ReturnsAsync(newly_updated_com);


            // Prepared Act
            var Action = async () =>
            {
                ComunityResponse actual_com_response = await _comunitiesService.UpdateComunity(update_com_request);
            };

            // Assert
            await Action.Should().ThrowAsync<ArgumentException>();
            _outputHelper.WriteLine("UpdateComunity threw ArgumentNullException as expected for null input.");
        }


        [Fact]
        public async Task UpdateComunity_InvalidDesc_ToBeRejected()
        {
            // Arrange
            Guid Com1Id = Guid.NewGuid();
            Guid CreatorId = Guid.NewGuid();
            Comunity existing_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity1")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();

            Comunity newly_updated_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity2")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();

            ComunityResponse expected_com_response = newly_updated_com.ToComunityResponse();

            UpdateComunityRequest? update_com_request = _fixture.Build<UpdateComunityRequest>()
            .With(temp => temp.Id, Com1Id)
            .With(temp => temp.TeacherId, CreatorId)
            .With(temp => temp.Name, "TestCommunity2")
            .With(temp => temp.Description, @" LONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONG
                                               LONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONG
                                               LONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONG
                                               LONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONG")
            .Create(); // Simulating a invalid property
            _outputHelper.WriteLine("UpdateComunityRequest is null, simulating invalid input.");

            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com });

            _comunitiesRepositoryMock
            .Setup(r => r.GetComunityById(It.IsAny<Guid>()))
            .ReturnsAsync(existing_com);


            _comunitiesRepositoryMock
            .Setup(r => r.UpdateComunity(It.IsAny<Comunity>()))
            .ReturnsAsync(newly_updated_com);


            // Prepared Act
            var Action = async () =>
            {
                ComunityResponse actual_com_response = await _comunitiesService.UpdateComunity(update_com_request);
            };

            // Assert
            await Action.Should().ThrowAsync<ArgumentException>();
            _outputHelper.WriteLine("UpdateComunity threw ArgumentNullException as expected for null input.");
        }


        [Fact]
        public async Task UpdateComunity_ValidObject_ToBeSuccessful()
        {
            // Arrange
            Guid Com1Id = Guid.NewGuid();
            Guid CreatorId = Guid.NewGuid();
            Comunity existing_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity1")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();

            Comunity newly_updated_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity2")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();

            ComunityResponse expected_com_response = newly_updated_com.ToComunityResponse();

            _outputHelper.WriteLine("Expected:");
            _outputHelper.WriteLine($"{expected_com_response.ToString()}");



            UpdateComunityRequest? update_com_request = _fixture.Build<UpdateComunityRequest>()
            .With(temp => temp.Id, Com1Id)
            .With(temp => temp.TeacherId, CreatorId)
            .With(temp => temp.Name, "TestCommunity2")
            .With(temp => temp.Description, "desc1")
            .Create();

            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com });

            _comunitiesRepositoryMock
            .Setup(r => r.GetComunityById(It.IsAny<Guid>()))
            .ReturnsAsync(existing_com);

            _comunitiesRepositoryMock
            .Setup(r => r.UserExistsInComunity(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(true);

            _comunitiesRepositoryMock
            .Setup(r => r.UpdateComunity(It.IsAny<Comunity>()))
            .ReturnsAsync(newly_updated_com);

            // Act
            ComunityResponse actual_com_response = await _comunitiesService.UpdateComunity(update_com_request);

            _outputHelper.WriteLine("Actual:");
            _outputHelper.WriteLine($"{actual_com_response.ToString()}");

            // Assert
            expected_com_response.Should().BeEquivalentTo(actual_com_response);
        }

        [Fact]
        public async Task UpdateComunity_DuplicateObject_ToBeRejected()
        {
            // Arrange
            Guid Com1Id = Guid.NewGuid();
            Guid Com2Id = Guid.NewGuid();
            Guid CreatorId = Guid.NewGuid();

            Comunity existing_com1 = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity1")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();


            Comunity existing_com2 = _fixture.Build<Comunity>()
           .With(c => c.Id, Com1Id)
           .With(c => c.TeacherId, CreatorId)
           .With(c => c.Name, "TestCommunity2")
           .With(c => c.Description, "desc2")
           .With(c => c.Users, new List<User>
               {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
               })
               .With(c => c.PostsList, new List<Post>
               {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com2Id }
               })
               .Create();


            Comunity newly_updated_com = _fixture.Build<Comunity>()
            .With(c => c.Id, Com1Id)
            .With(c => c.TeacherId, CreatorId)
            .With(c => c.Name, "TestCommunity2")
            .With(c => c.Description, "desc1")
            .With(c => c.Users, new List<User>
                {
                    new User { UserId = CreatorId, Name = "Alice", Email = "a@b.c", DateOfBirth = DateTime.UtcNow.AddYears(-25), ReceiveNewsLetter = true }
                })
                .With(c => c.PostsList, new List<Post>
                {
                    new Post { Id = Guid.NewGuid(), Title = "Post1", Body = "Body", CreatedAt = DateTime.UtcNow, ModifiedAt = DateTime.UtcNow, UserId = CreatorId, CommunityId = Com1Id }
                })
                .Create();

            ComunityResponse expected_com_response = newly_updated_com.ToComunityResponse();

            UpdateComunityRequest? update_com_request = _fixture.Build<UpdateComunityRequest>()
            .With(temp => temp.Id, Com2Id)
            .With(temp => temp.TeacherId, CreatorId)
            .With(temp => temp.Name, "TestCommunity1")
            .With(temp => temp.Description, "desc1")
            .Create(); // Simulating a invalid duplicate property
            _outputHelper.WriteLine("DuplicateProperty is invalid, simulating invalid input.");

            _comunitiesRepositoryMock
            .Setup(r => r.GetAllComunities())
            .ReturnsAsync(new List<Comunity> { existing_com1, existing_com2 });


            _comunitiesRepositoryMock
            .Setup(r => r.GetComunityById(It.IsAny<Guid>()))
            .ReturnsAsync(existing_com1);


            _comunitiesRepositoryMock
            .Setup(r => r.UserExistsInComunity(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(true);

            _comunitiesRepositoryMock
            .Setup(r => r.UpdateComunity(It.IsAny<Comunity>()))
            .ReturnsAsync(newly_updated_com);


            // Prepared Act
            var Action = async () =>
            {
                ComunityResponse actual_com_response = await _comunitiesService.UpdateComunity(update_com_request);
            };

            // Assert
            await Action.Should().ThrowAsync<DuplicateNameException>();
            _outputHelper.WriteLine("UpdateComunity threw ArgumentNullException as expected for null input.");
        }


        #endregion

        #region DeleteComunity

        [Fact]
        public async Task DeleteComunity_IdDoesNotExist_ToBeNull()
        {
            // Arrange
            _comunitiesRepositoryMock
            .Setup(r => r.GetComunityById(It.IsAny<Guid>()))
            .ReturnsAsync(null as Comunity);


            _outputHelper.WriteLine("InvalidGuid should return null");

            // Act
            ComunityResponse actual_com_response = await _comunitiesService.GetComunityByComId(Guid.NewGuid());

            // Assert
            actual_com_response.Should().BeNull();
        }

        [Fact]
        public async Task DeleteComunity_IdDoesxist_ToBeSuccessful()
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
            .Setup(r => r.GetComunityById(It.IsAny<Guid>()))
            .ReturnsAsync(existing_com);


            _comunitiesRepositoryMock
            .Setup(r => r.DeleteComunity(It.IsAny<Guid>()))
            .ReturnsAsync(true);


            // Act
            bool isDeleted = await _comunitiesService.DeleteComunityByComId(comId);

            _outputHelper.WriteLine("Deletion should be successful");

            // Assert
            isDeleted.Should().BeTrue();
        }

        #endregion

    }
}
