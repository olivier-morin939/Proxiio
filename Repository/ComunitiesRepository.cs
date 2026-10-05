using Azure.Core;
using Entities;
using Entities.Contexts;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using System.Linq.Expressions;
namespace Repository
{
    public class ComunitiesRepository : IComunitiesRepository
    {

        private readonly ApplicationDbContext _db;

        public ComunitiesRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Comunity> AddComunity(Comunity comunity, Guid creatorId)
        {
            await _db.Comunities.AddAsync(comunity);
            await _db.ComunityMembers.AddAsync(new ComunityMember { ComunityId = comunity.Id, UserId = creatorId, Role = ComunityRole.Teacher });
            await _db.SaveChangesAsync();
            return comunity;
        }

        public async Task<int> GetAllComunitiesCount()
        {
            return await _db.Comunities.CountAsync();
        }

        public async Task<Comunity> GetComunityById(Guid comunityId)
        {
            return await _db.Comunities.FindAsync(comunityId);
        
        }

        public async Task<List<Comunity>> GetAllComunities()
        {
            return await _db.Comunities.AsNoTracking().ToListAsync();
        }

        public async Task<bool> UserExistsInComunity(Guid userId, Guid comunityId)
        {
            Comunity? targetedComunity = await GetComunityById(comunityId);
            if(targetedComunity == null)
                return false;

            return targetedComunity?.Users?.Any(u => u.UserId == userId) ?? false;
        }

        public async Task<int> GetAllComunityPostsCount(Guid comId)
        {
           return await _db.Posts.CountAsync(p => p.CommunityId == comId);
        }

        public async Task<Comunity> UpdateComunity(Comunity comunity)
        {
            Comunity? matchingCom = await _db.Comunities.FirstOrDefaultAsync(c => c.Id == comunity.Id);

            if (matchingCom == null)
                return null;

            matchingCom.Name = comunity.Name;
            matchingCom.Description = comunity.Description;
            // Update other properties as needed
            int rowsUpdated = await _db.SaveChangesAsync();

            return matchingCom;
        }

        public async Task<bool> DeleteComunity(Guid comunityId)
        {
            Comunity? targetedCom = await _db.Comunities.FindAsync(comunityId);
            if (targetedCom == null)
                return false;

            _db.Comunities.Remove(targetedCom);
            int rowsDeleted = await _db.SaveChangesAsync();
            return rowsDeleted > 0;
        }

        public async Task<List<Comunity>> GetFilteredComunities(Expression<Func<Comunity, bool>> predicate)
        {
            return await _db.Comunities.Where(predicate).ToListAsync();
        }
    }
}
