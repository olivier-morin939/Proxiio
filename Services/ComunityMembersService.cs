using Entities;
using Entities.Contexts;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO.Comunities;
using System.Data;

namespace Services;

public class ComunityMembersService : IComunityMembersService
{
    private readonly ApplicationDbContext _db;
    public ComunityMembersService(ApplicationDbContext db) => _db = db;

    // Keeps the service easy to instantiate in isolated unit tests; application DI supplies SQL Server.
    public ComunityMembersService() : this(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)) { }

    public async Task<ComunityMemberResponse> AddComunityMember(AddComunityMemberRequest? request)
    {
        // Check if the DTO object is null
        ArgumentNullException.ThrowIfNull(request);

        // Model validation for the DTO object
        Helpers.HelpersValidation.ModelValidation(request);

        // Check if the community exists before adding a member
        if (!await _db.Comunities.AnyAsync(c => c.Id == request.ComunityId))
            throw new ArgumentNullException(nameof(request.ComunityId));

        // Check if the users exists before adding it as a member
        if (!await _db.Users.AnyAsync(u => u.UserId == request.UserId)) 
            throw new ArgumentNullException(nameof(request.UserId));

        // Check if the user is already registered as a member in the community
        if (await _db.ComunityMembers.AnyAsync(m => m.ComunityId == request.ComunityId && m.UserId == request.UserId))
            throw new DuplicateNameException("Member already exists in the community.");

        // Create a new community member
        ComunityMember member = new ComunityMember { ComunityId = request.ComunityId, UserId = request.UserId, Role = request.Role };

        // Add the community in the db
        _db.ComunityMembers.Add(member);
        await _db.SaveChangesAsync();

        // Return the member in DTO response
        return member.ToComunityMemberResponse();
    }

    public async Task<List<ComunityMemberResponse>> GetComunityMembers(Guid comunityId) => (await _db.ComunityMembers.AsNoTracking().Where(m => m.ComunityId == comunityId).OrderBy(m => m.JoinedAt).ToListAsync()).Select(m => m.ToComunityMemberResponse()).ToList();

    public async Task<bool> IsComunityMember(Guid comunityId, Guid userId) => await _db.ComunityMembers.AnyAsync(m => m.ComunityId == comunityId && m.UserId == userId);

    public async Task<int> GetComunityMembersCount(Guid comunityId) => await _db.ComunityMembers.CountAsync(m => m.ComunityId == comunityId);

    public async Task<bool> RemoveComunityMember(Guid comunityId, Guid userId)
    {
        // Search for the member
        ComunityMember? member = await _db.ComunityMembers.FirstOrDefaultAsync(m => m.ComunityId == comunityId && m.UserId == userId);
        
        // Check if we found the member
        if (member is null) 
            return false;

        // Delete the member in the community
        _db.ComunityMembers.Remove(member);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<ComunityMemberResponse> UpdateComunityMember(UpdateComunityMemberRequest request)
    {
        // Check if the DTO object is null
        ArgumentNullException.ThrowIfNull(request);


        // Model validation for the DTO object
        Helpers.HelpersValidation.ModelValidation(request);

        // Getting the member from the community
        ComunityMember member = await _db.ComunityMembers.FirstOrDefaultAsync(m => m.ComunityId == request.ComunityId && m.UserId == request.UserId) ?? throw new ArgumentNullException("Member not found");
        
        // Updating the role in the community
        member.Role = request.Role;
        await _db.SaveChangesAsync();

        // Return the DTO response
        return member.ToComunityMemberResponse();
    }
}
