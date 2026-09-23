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
    public ComunityMembersService() : this(new UsersDbContext(new DbContextOptionsBuilder<UsersDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)) { }

    public ComunityMemberResponse AddComunityMember(AddComunityMemberRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Helpers.HelpersValidation.ModelValidation(request);
        if (!_db.Comunities.Any(c => c.Id == request.ComunityId)) throw new ArgumentNullException(nameof(request.ComunityId));
        if (!_db.Users.Any(u => u.UserId == request.UserId)) throw new ArgumentNullException(nameof(request.UserId));
        if (_db.ComunityMembers.Any(m => m.ComunityId == request.ComunityId && m.UserId == request.UserId)) throw new DuplicateNameException("Member already exists in the community.");
        var member = new ComunityMember { ComunityId = request.ComunityId, UserId = request.UserId, Role = request.Role };
        _db.ComunityMembers.Add(member);
        _db.SaveChanges();
        return member.ToComunityMemberResponse();
    }

    public List<ComunityMemberResponse> GetComunityMembers(Guid comunityId) => _db.ComunityMembers.AsNoTracking().Where(m => m.ComunityId == comunityId).OrderBy(m => m.JoinedAt).ToList().Select(m => m.ToComunityMemberResponse()).ToList();

    public bool IsComunityMember(Guid comunityId, Guid userId) => _db.ComunityMembers.Any(m => m.ComunityId == comunityId && m.UserId == userId);

    public int GetComunityMembersCount(Guid comunityId) => _db.ComunityMembers.Count(m => m.ComunityId == comunityId);

    public bool RemoveComunityMember(Guid comunityId, Guid userId)
    {
        var member = _db.ComunityMembers.FirstOrDefault(m => m.ComunityId == comunityId && m.UserId == userId);
        if (member is null) return false;
        _db.ComunityMembers.Remove(member);
        _db.SaveChanges();
        return true;
    }

    public ComunityMemberResponse UpdateComunityMember(UpdateComunityMemberRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Helpers.HelpersValidation.ModelValidation(request);
        var member = _db.ComunityMembers.FirstOrDefault(m => m.ComunityId == request.ComunityId && m.UserId == request.UserId) ?? throw new ArgumentNullException("Member not found");
        member.Role = request.Role;
        _db.SaveChanges();
        return member.ToComunityMemberResponse();
    }
}
