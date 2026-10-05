using System.Data;
using Entities;
using Entities.Contexts;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO.Comunities;
using ServiceContracts.DTO.Posts;
using ServiceContracts.DTO.Users;

namespace Services;

public class ComunitiesService : IComunitiesService
{
    private readonly IComunitiesRepository _repository;
    public ComunitiesService(IComunitiesRepository repository) => _repository = repository;

    public async Task<ComunityResponse> AddComunity(AddComunityRequest? request)
    {
        // Check if the request is null
        if(request == null) 
            throw new ArgumentNullException(nameof(request));

        // Model validation for the DTO object
        Helpers.HelpersValidation.ModelValidation(request);

        // Check if the user exists
        if (request.TeacherId == Guid.Empty)
            throw new ArgumentException("The teacher must be have a valid ID.", nameof(request.TeacherId));

        // Check if we have an community name
        if (string.IsNullOrWhiteSpace(request.Name)) 
            throw new ArgumentException("A community name is required.", nameof(request.Name));

        // Check for duplicates
        if ((await _repository.GetAllComunities()).Any(c => c.Name != null && c.Name.ToLower() == request.Name.Trim().ToLower())) 
            throw new DuplicateNameException("A community with the same name already exists.");

        Comunity community = new Comunity { Id = Guid.NewGuid(), TeacherId = request.TeacherId, Name = request.Name.Trim(), Description = request.Description?.Trim() };
        
        // Add the new community to the db
        Comunity addedCommunity = await _repository.AddComunity(community, request.TeacherId);

        return addedCommunity.ToComunityResponse();
    }

    public async Task<List<ComunityResponse>> GetAllComunities()
    {
        var raw = await _repository.GetAllComunities();
        // Garantir que raw n'est jamais null
        var communities = raw ?? new List<Comunity>();

        return communities
            .Select(c => new ComunityResponse
            {
                Id = c.Id,
                TeacherId = c.TeacherId,
                Name = c.Name,
                Description = c.Description,
                UsersCount = c.Users?.Count() ?? 0,
                PostsCount = c.PostsList?.Count() ?? 0,
                Users = (c.Users ?? Enumerable.Empty<User>()).Select(u => new UserResponse
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = u.Email,
                    DateOfBirth = u.DateOfBirth,
                    UserState = u.UserState,
                    Role = u.Role,
                    ReceiveNewsLetter = u.ReceiveNewsLetter
                }).ToList(),
                Posts = (c.PostsList ?? Enumerable.Empty<Post>()).Select(p => new PostResponse
                {
                    Id = p.Id,
                    Title = p.Title,
                    Body = p.Body,
                    CreatedAt = p.CreatedAt,
                    ModifiedAt = p.ModifiedAt,
                    AdditionalsPath = p.AdditionalsPath,
                    ImagesPath = p.ImagesPath,
                    UserId = p.UserId,
                    CommunityId = p.CommunityId
                }).ToList()
            })
            .ToList();
    }

    public async Task<int> GetAllComunitiesCount() => await _repository.GetAllComunitiesCount();
    public async Task<int> GetComunityPostsCount(Guid comunityId) => await _repository.GetAllComunityPostsCount(comunityId);

    public async Task<ComunityResponse> GetComunityByComId(Guid ComId)
    {

        Comunity? community = await _repository.GetComunityById(ComId);

        if (community == null)
            return null;

        return new ComunityResponse
        {
            Id = community.Id,
            TeacherId = community.TeacherId,
            Name = community.Name,
            Description = community.Description,
            UsersCount = community.Users?.Count ?? 0,
            PostsCount = community.PostsList?.Count ?? 0,
            Users = community.Users?.Select(u => new UserResponse
            {
                UserId = u.UserId,
                Name = u.Name,
                Email = u.Email,
                DateOfBirth = u.DateOfBirth,
                UserState = u.UserState,
                Role = u.Role,
                ReceiveNewsLetter = u.ReceiveNewsLetter
            }).ToList() ?? new(),
            Posts = community.PostsList?.Select(p => new PostResponse
            {
                Id = p.Id,
                Title = p.Title,
                Body = p.Body,
                CreatedAt = p.CreatedAt,
                ModifiedAt = p.ModifiedAt,
                AdditionalsPath = p.AdditionalsPath,
                ImagesPath = p.ImagesPath,
                UserId = p.UserId,
                CommunityId = p.CommunityId
            }).ToList() ?? new()
        };


    }

    public async Task<ComunityResponse> UpdateComunity(UpdateComunityRequest request)
    {
        // Checking if the request is null
        ArgumentNullException.ThrowIfNull(request);

        // Model validation for the DTO object
        Helpers.HelpersValidation.ModelValidation(request);

        // Search the matching community
        Comunity community = await _repository.GetComunityById(request.Id) ?? throw new ArgumentNullException(nameof(request.Id));

        // Search if the teacher has a valid ID
        if (request.TeacherId == Guid.Empty)
            throw new ArgumentException("The teacher must have a valid ID.", nameof(request.TeacherId));

        // Search if the teacher exists
        if (!await _repository.UserExistsInComunity(request.TeacherId, request.Id))
            throw new ArgumentException("The teacher must be an existing user.", nameof(request.TeacherId));

        // Search for duplicates
        if ((await _repository.GetAllComunities()).Any(c => c.Id != request.Id && c.Name != null && c.Name.ToLower() == request.Name!.Trim().ToLower()))
            throw new DuplicateNameException("A community with the same name already exists.");

        // Updating the informations
        Comunity updatedCom = await _repository.UpdateComunity(community);

        return updatedCom.ToComunityResponse();
    }

    public async Task<bool> DeleteComunityByComId(Guid ComId)
    {
        // Search the community
        Comunity? community = await _repository.GetComunityById(ComId);

        // Checking if we found the community
        if (community == null)
            return false;

        // Delete the targeted community
        return await _repository.DeleteComunity(ComId);
    }

    public async Task<List<ComunityResponse>> GetFilteredComunities(string searchBy, string searchString)
    {
        // Get all communities first
        List<Comunity> communities = await _repository.GetAllComunities();

        // Check if the searchBy and searchString is null or empty
        if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString)) 
            return communities.Select(c => c.ToComunityResponse()).ToList();

        List<Comunity> filteredComunities =  searchBy switch
        {
            nameof(ComunityResponse.Id) => await _repository.GetFilteredComunities(temp => (!string.IsNullOrEmpty(temp.Id.ToString()) ? temp.Id.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase) : true)),
            nameof(ComunityResponse.Name) => await _repository.GetFilteredComunities(temp => (!string.IsNullOrEmpty(temp.Name) ? temp.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true)),
            nameof(ComunityResponse.Description) => await _repository.GetFilteredComunities(temp => (!string.IsNullOrEmpty(temp.Description) ? temp.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true)),
            nameof(ComunityResponse.TeacherId) => await _repository.GetFilteredComunities(temp => temp.TeacherId.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)),
            nameof(ComunityResponse.UsersCount) => await _repository.GetFilteredComunities(temp => temp.Users.Count().ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)),
            nameof(ComunityResponse.PostsCount) => await _repository.GetFilteredComunities(temp => temp.PostsList.Count().ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)),
            _ => communities
        };

        return filteredComunities.Select(c => c.ToComunityResponse()).ToList();
    }

    public Task<List<ComunityResponse>> GetSortedComunities(List<ComunityResponse> communities, string sortBy, SortOption sortOrder)
    {
        // Check if the sort by is set
        if (string.IsNullOrEmpty(sortBy)) 
            return Task.FromResult(communities);

        // Look if it is in descending order
        bool descending = sortOrder != SortOption.ASC;

        // Sort base on the sortBy field and the sort order: ASC or DESC
        List<ComunityResponse> sortedCommunities = sortBy switch
        {
            nameof(ComunityResponse.Name) => descending ? communities.OrderByDescending(c => c.Name).ToList() : communities.OrderBy(c => c.Name).ToList(),
            nameof(ComunityResponse.Description) => descending ? communities.OrderByDescending(c => c.Description).ToList() : communities.OrderBy(c => c.Description).ToList(),
            nameof(ComunityResponse.TeacherId) => descending ? communities.OrderByDescending(c => c.TeacherId).ToList() : communities.OrderBy(c => c.TeacherId).ToList(),
            nameof(ComunityResponse.UsersCount) => descending ? communities.OrderByDescending(c => c.UsersCount).ToList() : communities.OrderBy(c => c.UsersCount).ToList(),
            nameof(ComunityResponse.PostsCount) => descending ? communities.OrderByDescending(c => c.PostsCount).ToList() : communities.OrderBy(c => c.PostsCount).ToList(),
            _ => communities
        };
        return Task.FromResult(sortedCommunities);
    }

}
