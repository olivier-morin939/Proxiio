using Entities;
using Entities.Enums;
using ServiceContracts;
using ServiceContracts.DTO.Comunities;
using ServiceContracts.DTO.Posts;
using ServiceContracts.DTO.Users;
using Services.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Services
{
    public class ComunitiesService : IComunitiesService
    {
        private readonly object _lock = new object();
        private readonly List<Comunity> _comunities = new List<Comunity>();
        private readonly List<ComunityMember> _members = new();


        // --- Comunity management ---
        public ComunityResponse AddComunity(AddComunityRequest? addComunityRequest)
        {
            if (addComunityRequest == null) 
                throw new ArgumentNullException(nameof(addComunityRequest));

            HelpersValidation.ModelValidation(addComunityRequest);

            Comunity newCom = addComunityRequest.ToComunity();

            // ensure unique name (case-insensitive) and add atomically
            lock (_lock)
            {
                if (!string.IsNullOrEmpty(newCom.Name))
                {
                    bool exists = _comunities.Any(c => !string.IsNullOrEmpty(c.Name) && c.Name.Equals(newCom.Name, StringComparison.OrdinalIgnoreCase));
                    if (exists) throw new DuplicateNameException("A comunity with the same name already exists");
                }

                _comunities.Add(newCom);
            }

            return newCom.ToComunityResponse();
        }

        public List<ComunityResponse> GetFilteredComunities(string searchBy, string searchString)
        {
            if (string.IsNullOrEmpty(searchBy))
            {
                return GetAllComunities();
            }

            if (string.IsNullOrEmpty(searchString))
            {
                return GetAllComunities();
            }

            List<Comunity> filtered = new List<Comunity>();

            switch (searchBy)
            {
                case nameof(ComunityResponse.Id):
                    filtered = _comunities.Where(c => c.Id.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;

                case nameof(ComunityResponse.Name):
                    filtered = _comunities.Where(c => c.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;

                case nameof(ComunityResponse.Description):
                    filtered = _comunities.Where(c => c.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;

                case nameof(ComunityResponse.TeacherId):
                    filtered = _comunities.Where(c => c.TeacherId.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;

                case nameof(ComunityResponse.UsersCount):
                    filtered = _comunities.Where(c => (c.Users?.Count ?? 0).ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;

                case nameof(ComunityResponse.PostsCount):
                    filtered = _comunities.Where(c => (c.PostsList?.Count ?? 0).ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;

                default:
                    // fallback: search in posts titles
                    filtered = _comunities;
                    break;
            }

            return filtered.Select(c => c.ToComunityResponse()).ToList();
        }

        public List<ComunityResponse> GetSortedComunities(List<ComunityResponse> allComResponses, string sortBy, SortOption sortOrder)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return allComResponses;
            }


            return sortBy switch
            {
                nameof(ComunityResponse.Name) => sortOrder == SortOption.ASC
                    ? allComResponses.OrderBy(com => com.Name).ToList()
                    : allComResponses.OrderByDescending(com => com.Name).ToList(),

                nameof(ComunityResponse.Description) => sortOrder == SortOption.ASC
                    ? allComResponses.OrderBy(com => com.Description).ToList()
                    : allComResponses.OrderByDescending(com => com.Description).ToList(),

                nameof(ComunityResponse.TeacherId) => sortOrder == SortOption.ASC
                    ? allComResponses.OrderBy(com => com.TeacherId.ToString()).ToList()
                    : allComResponses.OrderByDescending(com => com.TeacherId.ToString()).ToList(),

                nameof(ComunityResponse.UsersCount) => sortOrder == SortOption.ASC
                    ? allComResponses.OrderBy(com => com.UsersCount ).ToList()
                    : allComResponses.OrderByDescending(com => com.UsersCount).ToList(),

                nameof(ComunityResponse.PostsCount) => sortOrder == SortOption.ASC
                    ? allComResponses.OrderBy(com => com.PostsCount).ToList()
                    : allComResponses.OrderByDescending(com => com.PostsCount).ToList(),

                _ => allComResponses
            };
        }

        public List<ComunityResponse> GetAllComunities()
        {
            return _comunities.Select(c => c.ToComunityResponse()).ToList();
        }

        public ComunityResponse GetComunityByComId(Guid ComId)
        {
            Comunity? com = _comunities.FirstOrDefault(c => c.Id == ComId);
            if (com == null) 
                throw new ArgumentNullException(nameof(ComId));

            return com.ToComunityResponse();
        }

        public ComunityResponse UpdateComunity(UpdateComunityRequest updateComunityRequest)
        {
            if (updateComunityRequest == null) 
                throw new ArgumentNullException(nameof(updateComunityRequest));

            HelpersValidation.ModelValidation(updateComunityRequest);

            // perform find, uniqueness check and update atomically
            lock (_lock)
            {
                Comunity? com = _comunities.FirstOrDefault(c => c.Id == updateComunityRequest.Id);
                if (com == null)
                    throw new ArgumentNullException($"No comunity found with ID: {updateComunityRequest.Id}", nameof(updateComunityRequest.Id));

                // ensure unique name on update (case-insensitive) excluding current comunity
                if (!string.IsNullOrEmpty(updateComunityRequest.Name))
                {
                    bool exists = _comunities.Any(c => c.Id != updateComunityRequest.Id && !string.IsNullOrEmpty(c.Name) && c.Name.Equals(updateComunityRequest.Name, StringComparison.OrdinalIgnoreCase));
                    if (exists) throw new DuplicateNameException("A comunity with the same name already exists");
                }

                com.TeacherId = updateComunityRequest.TeacherId;
                com.Name = updateComunityRequest.Name;
                com.Description = updateComunityRequest.Description;

                return com.ToComunityResponse();
            }
        }

        public bool DeleteComunityByComId(Guid ComId)
        {
            int index = _comunities.FindIndex(c => c.Id == ComId);
            if (index == -1) 
                return false;

            _comunities.RemoveAt(index);

            return true;
        }

        public void SeedMockComunities()
        {
            _comunities.Clear();

            var com1 = new Comunity()
            {
                Id = Guid.Parse("144082DD-E392-4E5E-B518-A83C7DEBB437"),
                TeacherId = Guid.NewGuid(),
                Name = "Community One",
                Description = "Description for community one",
                Users = new List<User>()
                {
                    new User(){ UserId = Guid.NewGuid(), Name = "Teacher1" }
                },
                PostsList = new List<Post>()
            };

            // add couple posts
            com1.PostsList.Add(new Post() { Id = Guid.NewGuid(), UserId = com1.TeacherId, Title = "Bienvenue", Body = "Bienvenue dans la communauté", ImagesPath = new List<string>() { "/img/welcome.png" } });
            com1.PostsList.Add(new Post() { Id = Guid.NewGuid(), UserId = com1.TeacherId, Title = "Announcements", Body = "General announcements", ImagesPath = new List<string>() });

            _comunities.Add(com1);

            // add 9 more test communities (total 10)
            for (int i = 2; i <= 10; i++)
            {
                var com = new Comunity()
                {
                    Id = Guid.NewGuid(),
                    TeacherId = Guid.NewGuid(),
                    Name = $"Community {i}",
                    Description = $"Description for community {i}",
                    Users = new List<User>() { new User() { UserId = Guid.NewGuid(), Name = $"Teacher{i}" } },
                    PostsList = new List<Post>()
                };

                // vary posts count: some communities get 0, some 1, some 2 posts
                if (i % 3 == 0)
                {
                    com.PostsList.Add(new Post() { Id = Guid.NewGuid(), UserId = com.TeacherId, Title = $"Post A for community {i}", Body = "Auto-generated post A", ImagesPath = new List<string>() });
                    com.PostsList.Add(new Post() { Id = Guid.NewGuid(), UserId = com.TeacherId, Title = $"Post B for community {i}", Body = "Auto-generated post B", ImagesPath = new List<string>() });
                }
                else if (i % 2 == 0)
                {
                    com.PostsList.Add(new Post() { Id = Guid.NewGuid(), UserId = com.TeacherId, Title = $"Post for community {i}", Body = "Auto-generated post", ImagesPath = new List<string>() });
                }

                _comunities.Add(com);
            }
        }

        public void SeedMockPosts()
        {
            // Ensure at least one comunity exists
            if (!_comunities.Any()) SeedMockComunities();

            // add a sample post to first comunity
            var post = new Post()
            {
                Id = Guid.NewGuid(),
                UserId = _comunities[0].TeacherId,
                Title = "Annonce",
                Body = "Ceci est une annonce",
                ImagesPath = new List<string>()
                {
                    "/img/announce.png"
                }
            };

            _comunities[0].PostsList.Add(post);
        }


        // --- Comunity members management ---
        public ComunityMemberResponse AddComunityMember(AddComunityMemberRequest? addComunityMemberRequest)
        {
            if (addComunityMemberRequest == null) throw new ArgumentNullException(nameof(addComunityMemberRequest));

            HelpersValidation.ModelValidation(addComunityMemberRequest);

            // verify comunity exists
            var com = _comunities.FirstOrDefault(c => c.Id == addComunityMemberRequest.ComunityId);
            if (com == null) throw new ArgumentNullException(nameof(addComunityMemberRequest.ComunityId));

            // check duplicate
            bool exists = _members.Any(m => m.ComunityId == addComunityMemberRequest.ComunityId && m.UserId == addComunityMemberRequest.UserId);
            if (exists) throw new DuplicateNameException("Member already exists in the comunity");

            var member = new ComunityMember()
            {
                ComunityId = addComunityMemberRequest.ComunityId,
                UserId = addComunityMemberRequest.UserId,
                Role = addComunityMemberRequest.Role
            };

            _members.Add(member);

            return member.ToComunityMemberResponse();
        }

        public List<ComunityMemberResponse> GetComunityMembers(Guid comunityId)
        {
            return _members.Where(m => m.ComunityId == comunityId).Select(m => m.ToComunityMemberResponse()).ToList();
        }

        public bool RemoveComunityMember(Guid comunityId, Guid userId)
        {
            int idx = _members.FindIndex(m => m.ComunityId == comunityId && m.UserId == userId);
            if (idx == -1) return false;
            _members.RemoveAt(idx);
            return true;
        }

        public ComunityMemberResponse UpdateComunityMember(UpdateComunityMemberRequest updateComunityMemberRequest)
        {
            if (updateComunityMemberRequest == null) throw new ArgumentNullException(nameof(updateComunityMemberRequest));

            HelpersValidation.ModelValidation(updateComunityMemberRequest);

            var member = _members.FirstOrDefault(m => m.ComunityId == updateComunityMemberRequest.ComunityId && m.UserId == updateComunityMemberRequest.UserId);
            if (member == null) throw new ArgumentNullException("Member not found");

            member.Role = updateComunityMemberRequest.Role;

            return member.ToComunityMemberResponse();
        }

        // --- Post likes management ---
        public PostLikeResponse AddPostLike(AddPostLikeRequest? addPostLikeRequest)
        {
            if (addPostLikeRequest == null) throw new ArgumentNullException(nameof(addPostLikeRequest));

            HelpersValidation.ModelValidation(addPostLikeRequest);

            var post = _comunities.SelectMany(c => c.PostsList ?? new List<Post>()).FirstOrDefault(p => p.Id == addPostLikeRequest.PostId);
            if (post == null) throw new ArgumentNullException(nameof(addPostLikeRequest.PostId));

            // prevent duplicate like from same user
            bool already = post.Likes.Any(l => l.UserId == addPostLikeRequest.UserId);
            if (already) throw new DuplicateNameException("User already liked this post");

            var like = new PostLike()
            {
                PostId = addPostLikeRequest.PostId,
                UserId = addPostLikeRequest.UserId,
            };

            post.Likes.Add(like);

            return like.ToPostLikeResponse();
        }

        public bool RemovePostLike(Guid postId, Guid userId)
        {
            var post = _comunities.SelectMany(c => c.PostsList ?? new List<Post>()).FirstOrDefault(p => p.Id == postId);
            if (post == null) return false;

            int idx = post.Likes.FindIndex(l => l.UserId == userId);
            if (idx == -1) return false;
            post.Likes.RemoveAt(idx);
            return true;
        }

        public List<PostLikeResponse> GetPostLikes(Guid postId)
        {
            var post = _comunities.SelectMany(c => c.PostsList ?? new List<Post>()).FirstOrDefault(p => p.Id == postId);
            if (post == null) return new List<PostLikeResponse>();
            return post.Likes.Select(l => l.ToPostLikeResponse()).ToList();
        }

        // --- Post management ---

        public List<PostResponse> GetAllPosts()
        {
            var posts = _comunities.SelectMany(c => c.PostsList ?? new List<Post>()).ToList();
            if (posts == null || posts.Count == 0) return new List<PostResponse>();
            return posts.Select(p => p.ToPostResponse()).ToList();
        }
        public PostResponse AddPost(AddPostRequest? addPostRequest)
        {
            if (addPostRequest == null) throw new ArgumentNullException(nameof(addPostRequest));

            var fields = new Dictionary<string, string?>
            {
                { nameof(addPostRequest.Title), addPostRequest.Title }
            };

            foreach (var (fieldName, fieldValue) in fields)
            {
                if (string.IsNullOrWhiteSpace(fieldValue))
                {
                    throw new ArgumentException("Property cannot be empty", fieldName);
                }
            }

            HelpersValidation.ModelValidation(addPostRequest);

            Post newPost = addPostRequest.ToPost();

            // If comunity specified, add to it
            if (addPostRequest.ComunityId.HasValue && addPostRequest.ComunityId.Value != Guid.Empty)
            {
                var com = _comunities.FirstOrDefault(c => c.Id == addPostRequest.ComunityId.Value);
                if (com == null)
                {
                    // create new comunity container if not found
                    com = new Comunity() { Id = addPostRequest.ComunityId.Value, TeacherId = addPostRequest.UserId };
                    _comunities.Add(com);
                }

                com.PostsList ??= new List<Post>();
                com.PostsList.Add(newPost);
            }
            else
            {
                // add to first comunity if exists, otherwise create default
                if (_comunities.Any())
                {
                    _comunities[0].PostsList.Add(newPost);
                }
                else
                {
                    var com = new Comunity() { Id = Guid.NewGuid(), TeacherId = addPostRequest.UserId, PostsList = new List<Post>() { newPost } };
                    _comunities.Add(com);
                }
            }

            return newPost.ToPostResponse();
        }

        public PostResponse GetPostByPostId(Guid PostId)
        {
            var post = _comunities.SelectMany(c => c.PostsList ?? new List<Post>()).FirstOrDefault(p => p.Id == PostId);
            if (post == null) throw new ArgumentNullException(nameof(PostId));
            return post.ToPostResponse();
        }

        public PostResponse UpdatePost(UpdatePostRequest updatePostRequest)
        {
            if (updatePostRequest == null) throw new ArgumentNullException(nameof(updatePostRequest));

            var fields = new Dictionary<string, string?>
            {
                { nameof(updatePostRequest.Title), updatePostRequest.Title }
            };

            foreach (var (fieldName, fieldValue) in fields)
            {
                if (string.IsNullOrWhiteSpace(fieldValue))
                {
                    throw new ArgumentException("Property cannot be empty", fieldName);
                }
            }

            HelpersValidation.ModelValidation(updatePostRequest);

            var post = _comunities.SelectMany(c => c.PostsList ?? new List<Post>()).FirstOrDefault(p => p.Id == updatePostRequest.Id);
            if (post == null) throw new ArgumentNullException($"No post found with ID: {updatePostRequest.Id}", nameof(updatePostRequest.Id));

            // update fields
            post.Title = updatePostRequest.Title;
            post.Body = updatePostRequest.Body;
            post.ImagesPath = updatePostRequest.ImagesPath ?? new List<string>();
            post.AdditionalsPath = updatePostRequest.AdditionalsPath ?? new List<string>();

            return post.ToPostResponse();
        }

        public bool DeletePostByPostId(Guid PostId)
        {
            foreach (var com in _comunities)
            {
                var index = com.PostsList?.FindIndex(p => p.Id == PostId) ?? -1;
                if (index != -1)
                {
                    com.PostsList.RemoveAt(index);
                    return true;
                }
            }

            return false;
        }

        public int GetAllComunitiesCount()
        {
            return _comunities.Count();
        }

        public int GetComunityPostsCount(Guid comunityId)
        {
            Comunity? com = _comunities.FirstOrDefault(c => c.Id == comunityId);
            if (com == null)
                throw new ArgumentNullException(nameof(comunityId));

            return com.PostsList.Count() > 0 ? com.PostsList.Count() : 0;
        }

        public int GetComunityMembersCount(Guid comunityId)
        {
            Comunity? com = _comunities.FirstOrDefault(c => c.Id == comunityId);
            if (com == null)
                throw new ArgumentNullException(nameof(comunityId));

            return com.Users.Count() > 0 ? com.Users.Count() : 0;
        }

        public int GetAllPostsCount()
        {
            return _comunities.SelectMany(c => c.PostsList).Count();
        }
    }
}
