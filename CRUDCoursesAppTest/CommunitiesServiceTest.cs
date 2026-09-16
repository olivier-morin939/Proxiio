using Entities;
using Entities.Enums;
using ServiceContracts.DTO.Comunities;
using ServiceContracts.DTO.Posts;
using Services;
using System;
using System.Collections.Generic;
using System.Data;
using Xunit;
using Xunit.Abstractions;

namespace CRUDCoursesAppTest
{
    public class CommunitiesServiceTest
    {
        private readonly ComunitiesService _comunitiesService;
        private readonly ITestOutputHelper _outputHelper;

        public CommunitiesServiceTest(ITestOutputHelper outputHelper)
        {
            _comunitiesService = new ComunitiesService();
            _outputHelper = outputHelper;
        }

        public AddComunityRequest AddBasicComunityRequest()
        {
            return new AddComunityRequest()
            {
                TeacherId = Guid.NewGuid(),
                Name = "Test Comunity",
                Description = "Description for test comunity"
            };
        }

        public AddPostRequest AddBasicPostRequest(Guid comunityId)
        {
            return new AddPostRequest()
            {
                UserId = Guid.NewGuid(),
                Title = "Testing Title",
                Body = "Testing Body",
                ComunityId = comunityId
            };
        }

        public AddComunityMemberRequest AddBasicMemberRequest(Guid comunityId)
        {
            return new AddComunityMemberRequest()
            {
                ComunityId = comunityId,
                UserId = Guid.NewGuid(),
                Role = ComunityRole.Member
            };
        }

        public AddPostLikeRequest AddBasicPostLikeRequest(Guid postId)
        {
            return new AddPostLikeRequest()
            {
                PostId = postId,
                UserId = Guid.NewGuid()
            };
        }

        [Fact]
        public void SeedMockComunities_ReturnsComunities()
        {
            // Arrange
            _comunitiesService.SeedMockComunities();

            // Act
            List<ComunityResponse> all = _comunitiesService.GetAllComunities();

            // Assert
            Assert.NotNull(all);
            Assert.NotEmpty(all);
            // Expect 10 seeded communities
            Assert.Equal(10, all.Count);
        }

        [Fact]
        public void AddComunity_ObjectIsNull()
        {
            AddComunityRequest? req = null;
            Assert.Throws<ArgumentNullException>(() => _comunitiesService.AddComunity(req));
        }

        [Fact]
        public void AddComunity_ValidObject()
        {
            // Arrange
            var req = AddBasicComunityRequest();

            // Act
            var resp = _comunitiesService.AddComunity(req);

            // Assert
            Assert.Equal(req.TeacherId, resp.TeacherId);
            Assert.NotEqual(Guid.Empty, resp.Id);
        }

        [Fact]
        public void GetComunityByComId_ReturnsValid()
        {
            // Arrange
            _comunitiesService.SeedMockComunities();
            var all = _comunitiesService.GetAllComunities();
            var id = all[0].Id;

            // Act
            var resp = _comunitiesService.GetComunityByComId(id);

            // Assert
            Assert.Equal(id, resp.Id);
        }

        [Fact]
        public void UpdateComunity_ObjectIsNull()
        {
            UpdateComunityRequest? req = null;
            Assert.Throws<ArgumentNullException>(() => _comunitiesService.UpdateComunity(req));
        }

        [Fact]
        public void DeleteComunityByComId_CanDelete()
        {
            // Arrange
            var req = AddBasicComunityRequest();
            var added = _comunitiesService.AddComunity(req);

            // Act
            bool removed = _comunitiesService.DeleteComunityByComId(added.Id);
            var all = _comunitiesService.GetAllComunities();

            // Assert
            Assert.True(removed);
            Assert.DoesNotContain(all, c => c.Id == added.Id);
        }

        [Fact]
        public void AddComunityMember_ObjectIsNull()
        {
            AddComunityMemberRequest? req = null;
            Assert.Throws<ArgumentNullException>(() => _comunitiesService.AddComunityMember(req));
        }

        [Fact]
        public void AddComunityMember_ValidAndDuplicate()
        {
            // Arrange
            var com = _comunitiesService.AddComunity(AddBasicComunityRequest());
            var memberReq = AddBasicMemberRequest(com.Id);

            // Act
            var resp = _comunitiesService.AddComunityMember(memberReq);

            // Assert
            Assert.Equal(memberReq.ComunityId, resp.ComunityId);

            // Duplicate should throw
            Assert.Throws<DuplicateNameException>(() => _comunitiesService.AddComunityMember(memberReq));
        }

        [Fact]
        public void GetAndRemoveComunityMember()
        {
            var com = _comunitiesService.AddComunity(AddBasicComunityRequest());
            var memberReq = AddBasicMemberRequest(com.Id);
            var added = _comunitiesService.AddComunityMember(memberReq);

            var list = _comunitiesService.GetComunityMembers(com.Id);
            Assert.Contains(list, m => m.UserId == memberReq.UserId);

            bool removed = _comunitiesService.RemoveComunityMember(com.Id, memberReq.UserId);
            Assert.True(removed);

            var list2 = _comunitiesService.GetComunityMembers(com.Id);
            Assert.DoesNotContain(list2, m => m.UserId == memberReq.UserId);
        }

        [Fact]
        public void UpdateComunityMember_Valid()
        {
            var com = _comunitiesService.AddComunity(AddBasicComunityRequest());
            var memberReq = AddBasicMemberRequest(com.Id);
            _comunitiesService.AddComunityMember(memberReq);

            var updateReq = new UpdateComunityMemberRequest() { ComunityId = com.Id, UserId = memberReq.UserId, Role = ComunityRole.Moderator };
            var updated = _comunitiesService.UpdateComunityMember(updateReq);

            Assert.Equal(ComunityRole.Moderator, updated.Role);
        }

        [Fact]
        public void AddPost_And_Like_Workflow()
        {
            // Arrange
            var com = _comunitiesService.AddComunity(AddBasicComunityRequest());
            var postReq = AddBasicPostRequest(com.Id);

            // Act
            var postResp = _comunitiesService.AddPost(postReq);

            // Assert add post
            Assert.Equal(postReq.Title, postResp.Title);

            // Like
            var likeReq = AddBasicPostLikeRequest(postResp.Id);
            var likeResp = _comunitiesService.AddPostLike(likeReq);
            Assert.Equal(likeReq.PostId, likeResp.PostId);

            // Duplicate like should throw
            Assert.Throws<DuplicateNameException>(() => _comunitiesService.AddPostLike(likeReq));

            // Get likes
            var likes = _comunitiesService.GetPostLikes(postResp.Id);
            Assert.Single(likes);

            // Remove like
            bool removed = _comunitiesService.RemovePostLike(postResp.Id, likeReq.UserId);
            Assert.True(removed);
        }

        [Fact]
        public void GetFilteredComunities_ById()
        {
            // Arrange
            _comunitiesService.SeedMockComunities();
            var all = _comunitiesService.GetAllComunities();
            var id = all[0].Id;

            // Act
            var filtered = _comunitiesService.GetFilteredComunities(nameof(ComunityResponse.Id), id.ToString());

            // Assert
            Assert.Single(filtered);
            Assert.Equal(id, filtered[0].Id);
        }

        [Fact]
        public void GetFilteredComunities_ByTeacherId()
        {
            // Arrange
            _comunitiesService.SeedMockComunities();
            var all = _comunitiesService.GetAllComunities();
            var teacherId = all[0].TeacherId;

            // Act
            var filtered = _comunitiesService.GetFilteredComunities(nameof(ComunityResponse.TeacherId), teacherId.ToString());

            // Assert
            Assert.NotEmpty(filtered);
            Assert.Contains(filtered, c => c.TeacherId == teacherId);
        }

        [Fact]
        public void GetFilteredComunities_ByUsersCount()
        {
            // Arrange
            _comunitiesService.SeedMockComunities();
            var com = _comunitiesService.GetAllComunities()[0];
            // ensure at least one user is present
            if (com.UsersCount == 0)
            {
                // add a member to underlying service
                var addReq = AddBasicComunityRequest();
                var added = _comunitiesService.AddComunity(addReq);
            }

            // Act
            var filtered = _comunitiesService.GetFilteredComunities(nameof(ComunityResponse.UsersCount), "1");

            // Assert
            Assert.NotNull(filtered);
        }

        [Fact]
        public void GetFilteredComunities_ByPostsCount()
        {
            // Arrange
            _comunitiesService.SeedMockComunities();
            var com = _comunitiesService.GetAllComunities()[0];
            // ensure a post exists
            _comunitiesService.SeedMockPosts();

            // Act
            var updatedCom = _comunitiesService.GetAllComunities().First(c => c.Id == com.Id);
            var postsCount = updatedCom.PostsCount;
            var filtered = _comunitiesService.GetFilteredComunities(nameof(ComunityResponse.PostsCount), postsCount.ToString());

            // Assert
            Assert.NotNull(filtered);
            Assert.Contains(filtered, f => f.Id == com.Id);
        }

        [Fact]
        public void GetFilteredComunities_ByPostTitle()
        {
            // Arrange
            _comunitiesService.SeedMockComunities();
            _comunitiesService.SeedMockPosts();
            var all = _comunitiesService.GetAllComunities();
            var postTitle = "Annonce";

            // Act
            var filtered = _comunitiesService.GetFilteredComunities("Title", postTitle);

            // Assert
            Assert.NotNull(filtered);
            Assert.True(filtered.Any());
        }
    }

    public class CommunitiesServiceConcurrencyTests
    {
        [Fact]
        public void Concurrent_AddComunity_SameName_OnlyOneCreated()
        {
            var svc = new Services.ComunitiesService();
            string name = "ConcurrentCommunity";
            int attempts = 10;

            var tasks = Enumerable.Range(0, attempts).Select(i => Task.Run(() =>
            {
                try
                {
                    svc.AddComunity(new AddComunityRequest() { TeacherId = Guid.NewGuid(), Name = name, Description = "desc" });
                    return true;
                }
                catch (DuplicateNameException)
                {
                    return false;
                }
            })).ToArray();

            Task.WaitAll(tasks);

            var results = tasks.Select(t => t.Result).ToList();
            var successes = results.Count(r => r);

            var all = svc.GetAllComunities().Where(c => c.Name == name).ToList();

            // Only one community with the name must exist
            Assert.Equal(1, all.Count);
            // At least one task succeeded in creating
            Assert.Equal(1, successes);
        }

        [Fact]
        public void Concurrent_UpdateComunity_ToSameName_ResultUnique()
        {
            var svc = new Services.ComunitiesService();
            svc.SeedMockComunities();
            var all = svc.GetAllComunities();
            // ensure at least two comunities
            if (all.Count < 2)
            {
                for (int i = all.Count; i < 2; i++) svc.AddComunity(new AddComunityRequest() { TeacherId = Guid.NewGuid(), Name = $"Tmp{i}", Description = "tmp" });
                all = svc.GetAllComunities();
            }

            var first = all[0];
            var second = all[1];
            string targetName = "TargetConcurrentName";

            // Prepare two update requests trying to set same name
            var update1 = new UpdateComunityRequest() { Id = first.Id, TeacherId = first.TeacherId, Name = targetName, Description = "u1" };
            var update2 = new UpdateComunityRequest() { Id = second.Id, TeacherId = second.TeacherId, Name = targetName, Description = "u2" };

            var t1 = Task.Run(() => {
                try { svc.UpdateComunity(update1); return true; } catch (DuplicateNameException) { return false; }
            });
            var t2 = Task.Run(() => {
                try { svc.UpdateComunity(update2); return true; } catch (DuplicateNameException) { return false; }
            });

            Task.WaitAll(t1, t2);

            var results = new[] { t1.Result, t2.Result };
            var successes = results.Count(r => r);

            var found = svc.GetAllComunities().Where(c => c.Name == targetName).ToList();

            // Only one should have the target name
            Assert.Equal(1, found.Count);
            // At least one update succeeded
            Assert.True(successes >= 1);
        }
    }
}
