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
    private readonly UsersDbContext _db;
    public ComunitiesService(UsersDbContext db) => _db = db;
    public ComunitiesService() : this(new UsersDbContext(new DbContextOptionsBuilder<UsersDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)) { }

    public ComunityResponse AddComunity(AddComunityRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Helpers.HelpersValidation.ModelValidation(request);
        if (request.TeacherId == Guid.Empty || !_db.Users.Any(u => u.UserId == request.TeacherId)) throw new ArgumentException("The teacher must be an existing user.", nameof(request.TeacherId));
        if (string.IsNullOrWhiteSpace(request.Name)) throw new ArgumentException("A community name is required.", nameof(request.Name));
        if (_db.Comunities.Any(c => c.Name != null && c.Name.ToLower() == request.Name.Trim().ToLower())) throw new DuplicateNameException("A community with the same name already exists.");
        var community = new Comunity { Id = Guid.NewGuid(), TeacherId = request.TeacherId, Name = request.Name.Trim(), Description = request.Description?.Trim() };
        _db.Comunities.Add(community);
        _db.ComunityMembers.Add(new ComunityMember { ComunityId = community.Id, UserId = request.TeacherId, Role = ComunityRole.Teacher });
        _db.SaveChanges();
        return MapCommunity(community);
    }

    public List<ComunityResponse> GetAllComunities() => _db.Comunities.AsNoTracking().ToList().Select(MapCommunity).ToList();

    public int GetAllComunitiesCount() => _db.Comunities.Count();
    public int GetComunityPostsCount(Guid comunityId) => _db.Posts.Count(p => p.CommunityId == comunityId);

    public ComunityResponse GetComunityByComId(Guid ComId)
    {
        var community = _db.Comunities.AsNoTracking().FirstOrDefault(c => c.Id == ComId) ?? throw new ArgumentNullException(nameof(ComId));
        return MapCommunity(community);
    }

    public ComunityResponse UpdateComunity(UpdateComunityRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Helpers.HelpersValidation.ModelValidation(request);
        var community = _db.Comunities.FirstOrDefault(c => c.Id == request.Id) ?? throw new ArgumentNullException(nameof(request.Id));
        if (request.TeacherId == Guid.Empty || !_db.Users.Any(u => u.UserId == request.TeacherId)) throw new ArgumentException("The teacher must be an existing user.", nameof(request.TeacherId));
        if (_db.Comunities.Any(c => c.Id != request.Id && c.Name != null && c.Name.ToLower() == request.Name!.Trim().ToLower())) throw new DuplicateNameException("A community with the same name already exists.");
        community.TeacherId = request.TeacherId;
        community.Name = request.Name?.Trim();
        community.Description = request.Description?.Trim();
        _db.SaveChanges();
        return MapCommunity(community);
    }

    public bool DeleteComunityByComId(Guid ComId)
    {
        var community = _db.Comunities.FirstOrDefault(c => c.Id == ComId);
        if (community is null) return false;
        _db.Comunities.Remove(community);
        _db.SaveChanges();
        return true;
    }

    public List<ComunityResponse> GetFilteredComunities(string searchBy, string searchString)
    {
        var communities = GetAllComunities();
        if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString)) return communities;
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

    public List<ComunityResponse> GetSortedComunities(List<ComunityResponse> communities, string sortBy, SortOption sortOrder)
    {
        if (string.IsNullOrEmpty(sortBy)) return communities;
        var descending = sortOrder != SortOption.ASC;
        return sortBy switch
        {
            nameof(ComunityResponse.Name) => descending ? communities.OrderByDescending(c => c.Name).ToList() : communities.OrderBy(c => c.Name).ToList(),
            nameof(ComunityResponse.Description) => descending ? communities.OrderByDescending(c => c.Description).ToList() : communities.OrderBy(c => c.Description).ToList(),
            nameof(ComunityResponse.TeacherId) => descending ? communities.OrderByDescending(c => c.TeacherId).ToList() : communities.OrderBy(c => c.TeacherId).ToList(),
            nameof(ComunityResponse.UsersCount) => descending ? communities.OrderByDescending(c => c.UsersCount).ToList() : communities.OrderBy(c => c.UsersCount).ToList(),
            nameof(ComunityResponse.PostsCount) => descending ? communities.OrderByDescending(c => c.PostsCount).ToList() : communities.OrderBy(c => c.PostsCount).ToList(),
            _ => communities
        };
    }

    private ComunityResponse MapCommunity(Comunity community)
    {
        var members = (from member in _db.ComunityMembers.AsNoTracking().Where(m => m.ComunityId == community.Id)
                       join user in _db.Users.AsNoTracking() on member.UserId equals user.UserId
                       select user).ToList();
        var posts = _db.Posts.AsNoTracking().Where(p => p.CommunityId == community.Id).ToList();
        return new ComunityResponse
        {
            Id = community.Id, TeacherId = community.TeacherId, Name = community.Name, Description = community.Description,
            UsersCount = members.Count, PostsCount = posts.Count,
            Users = members.Select(u => u.ToUserAddResponse()).ToList(),
            Posts = posts.Select(p => p.ToPostResponse()).ToList()
        };
    }
}
