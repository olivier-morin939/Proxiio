using Entities;
using Entities.Contexts;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO.Comunities;
using System.Data;

namespace Services;

public class ComunityMembersService : IComunityMembersService
{
    private readonly UsersDbContext _db;
    public ComunityMembersService(UsersDbContext db) => _db = db;

    // Keeps the service easy to instantiate in isolated unit tests; application DI supplies SQL Server.
    public ComunityMembersService() : this(new UsersDbContext(new DbContextOptionsBuilder<UsersDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)) { }

    public ComunityMemberResponse AddComunityMember(AddComunityMemberRequest? request)
    {
        // Check if the DTO object is null
        ArgumentNullException.ThrowIfNull(request);

        // Model validation for the DTO object
        Helpers.HelpersValidation.ModelValidation(request);

        // Check if the community exists before adding a member
        if (!_db.Comunities.Any(c => c.Id == request.ComunityId))
            throw new ArgumentNullException(nameof(request.ComunityId));

        // Check if the users exists before adding it as a member
        if (!_db.Users.Any(u => u.UserId == request.UserId)) 
            throw new ArgumentNullException(nameof(request.UserId));

        // Check if the user is already registered as a member in the community
        if (_db.ComunityMembers.Any(m => m.ComunityId == request.ComunityId && m.UserId == request.UserId))
            throw new DuplicateNameException("Member already exists in the community.");

        // Create a new community member
        ComunityMember member = new ComunityMember { ComunityId = request.ComunityId, UserId = request.UserId, Role = request.Role };

        // Add the community in the db
        _db.ComunityMembers.Add(member);
        _db.SaveChanges();

        // Return the member in DTO response
        return member.ToComunityMemberResponse();
    }

    public List<ComunityMemberResponse> GetComunityMembers(Guid comunityId) => _db.ComunityMembers.AsNoTracking().Where(m => m.ComunityId == comunityId).OrderBy(m => m.JoinedAt).ToList().Select(m => m.ToComunityMemberResponse()).ToList();

    public bool IsComunityMember(Guid comunityId, Guid userId) => _db.ComunityMembers.Any(m => m.ComunityId == comunityId && m.UserId == userId);

    public int GetComunityMembersCount(Guid comunityId) => _db.ComunityMembers.Count(m => m.ComunityId == comunityId);

    public bool RemoveComunityMember(Guid comunityId, Guid userId)
    {
        // Search for the member
        ComunityMember? member = _db.ComunityMembers.FirstOrDefault(m => m.ComunityId == comunityId && m.UserId == userId);
        
        // Check if we found the member
        if (member is null) 
            return false;

        // Delete the member in the community
        _db.ComunityMembers.Remove(member);
        _db.SaveChanges();
        return true;
    }

    public ComunityMemberResponse UpdateComunityMember(UpdateComunityMemberRequest request)
    {
        // Check if the DTO object is null
        ArgumentNullException.ThrowIfNull(request);


        // Model validation for the DTO object
        Helpers.HelpersValidation.ModelValidation(request);

        // Getting the member from the community
        ComunityMember member = _db.ComunityMembers.FirstOrDefault(m => m.ComunityId == request.ComunityId && m.UserId == request.UserId) ?? throw new ArgumentNullException("Member not found");
        
        // Updating the role in the community
        member.Role = request.Role;
        _db.SaveChanges();

        // Return the DTO response
        return member.ToComunityMemberResponse();
    }
}
