using System.Data;
using Entities;
using Entities.Contexts;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO.Comunities;
using ServiceContracts.DTO.Posts;
using ServiceContracts.DTO.Users;

namespace Services;

public class ComunitiesService : IComunitiesService
{
    private readonly ApplicationDbContext _db;
    public ComunitiesService(ApplicationDbContext db) => _db = db;

    // Keeps the service easy to instantiate in isolated unit tests; application DI supplies SQL Server.
    public ComunitiesService() : this(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)) { }

    public async Task<ComunityResponse> AddComunity(AddComunityRequest? request)
    {
        // Check if the request is null
        if(request == null) 
            throw new ArgumentNullException(nameof(request));

        // Model validation for the DTO object
        Helpers.HelpersValidation.ModelValidation(request);

        // Check if the user exists
        if (request.TeacherId == Guid.Empty || !await _db.Users.AnyAsync(u => u.UserId == request.TeacherId))
            throw new ArgumentException("The teacher must be an existing user.", nameof(request.TeacherId));

        // Check if we have an community name
        if (string.IsNullOrWhiteSpace(request.Name)) 
            throw new ArgumentException("A community name is required.", nameof(request.Name));

        // Check for duplicates
        if (await _db.Comunities.AnyAsync(c => c.Name != null && c.Name.ToLower() == request.Name.Trim().ToLower())) 
            throw new DuplicateNameException("A community with the same name already exists.");

        Comunity community = new Comunity { Id = Guid.NewGuid(), TeacherId = request.TeacherId, Name = request.Name.Trim(), Description = request.Description?.Trim() };
        
        // Add the new community to the db
        _db.Comunities.Add(community);
        _db.ComunityMembers.Add(new ComunityMember { ComunityId = community.Id, UserId = request.TeacherId, Role = ComunityRole.Teacher });
        await _db.SaveChangesAsync();

        // Map users and posts from the community and return it as a response
        return await MapCommunity(community);
    }

    public async Task<List<ComunityResponse>> GetAllComunities()
    {
        List<Comunity> communities = await _db.Comunities.AsNoTracking().ToListAsync();
        List<ComunityResponse> responses = new(communities.Count);
        foreach (Comunity community in communities)
            responses.Add(await MapCommunity(community));
        return responses;
    }

    public async Task<int> GetAllComunitiesCount() => await _db.Comunities.CountAsync();
    public async Task<int> GetComunityPostsCount(Guid comunityId) => await _db.Posts.CountAsync(p => p.CommunityId == comunityId);

    public async Task<ComunityResponse> GetComunityByComId(Guid ComId)
    {
        // Search the matching community and map users and posts from the community
        Comunity community = await _db.Comunities.AsNoTracking().FirstOrDefaultAsync(c => c.Id == ComId) ?? throw new ArgumentNullException(nameof(ComId));
        return await MapCommunity(community);
    }

    public async Task<ComunityResponse> UpdateComunity(UpdateComunityRequest request)
    {
        // Checking if the request is null
        ArgumentNullException.ThrowIfNull(request);

        // Model validation for the DTO object
        Helpers.HelpersValidation.ModelValidation(request);

        // Search the matching community
        Comunity community = await _db.Comunities.FirstOrDefaultAsync(c => c.Id == request.Id) ?? throw new ArgumentNullException(nameof(request.Id));

        // Search if the teacher exists
        if (request.TeacherId == Guid.Empty || !await _db.Users.AnyAsync(u => u.UserId == request.TeacherId)) 
            throw new ArgumentException("The teacher must be an existing user.", nameof(request.TeacherId));

        // Search for duplicates
        if (await _db.Comunities.AnyAsync(c => c.Id != request.Id && c.Name != null && c.Name.ToLower() == request.Name!.Trim().ToLower())) 
            throw new DuplicateNameException("A community with the same name already exists.");

        // Updating the informations
        community.TeacherId = request.TeacherId;
        community.Name = request.Name?.Trim();
        community.Description = request.Description?.Trim();
        await _db.SaveChangesAsync();

        // Map users and posts from the community and return it as a response
        return await MapCommunity(community);
    }

    public async Task<bool> DeleteComunityByComId(Guid ComId)
    {
        // Search the community
        Comunity? community = await _db.Comunities.FirstOrDefaultAsync(c => c.Id == ComId);

        // Checking if we found the community
        if (community is null)
            return false;

        // Delete the targeted community
        _db.Comunities.Remove(community);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<ComunityResponse>> GetFilteredComunities(string searchBy, string searchString)
    {
        // Get all communities first
        List<ComunityResponse> communities = await GetAllComunities();

        // Check if the searchBy and searchString is null or empty
        if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString)) 
            return communities;

        // Filtering the communities based on their searchBy and searchString
        return searchBy switch
        {
            nameof(ComunityResponse.Id) => communities.Where(c => c.Id.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList(),
            nameof(ComunityResponse.Name) => communities.Where(c => c.Name?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true).ToList(),
            nameof(ComunityResponse.Description) => communities.Where(c => c.Description?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true).ToList(),
            nameof(ComunityResponse.TeacherId) => communities.Where(c => c.TeacherId.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList(),
            nameof(ComunityResponse.UsersCount) => communities.Where(c => c.UsersCount.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList(),
            nameof(ComunityResponse.PostsCount) => communities.Where(c => c.PostsCount.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList(),
            _ => communities
        };
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

    private async Task<ComunityResponse> MapCommunity(Comunity community)
    {
        // Getting the users from the community
        List<User>     members = await (from member in _db.ComunityMembers.AsNoTracking().Where(m => m.ComunityId == community.Id)
                       join user in _db.Users.AsNoTracking() on member.UserId equals user.UserId
                       select user).ToListAsync();

        // Getting the posts from the community
        List<Post> posts = await _db.Posts.AsNoTracking().Where(p => p.CommunityId == community.Id).ToListAsync();

        // Return it as a DTO object
        return new ComunityResponse
        {
            Id = community.Id, TeacherId = community.TeacherId, Name = community.Name, Description = community.Description,
            UsersCount = members.Count, PostsCount = posts.Count,
            Users = members.Select(u => u.ToUserAddResponse()).ToList(),
            Posts = posts.Select(p => p.ToPostResponse()).ToList()
        };
    }
}
