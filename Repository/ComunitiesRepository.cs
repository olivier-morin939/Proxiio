using Azure.Core;
using Entities;
using Entities.Contexts;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using System.Linq.Expressions;
namespace Repository
{
    public class ComunitiesRepository : IComunitiesRepository
    {

        private readonly ApplicationDbContext _db;
        private readonly ILogger<ComunitiesRepository>? _logger;

        public ComunitiesRepository(ApplicationDbContext db, ILogger<ComunitiesRepository>? logger = null)
        {
            _db = db;
            _logger = logger;
        }

        private void LogOperation(string method, string operation, string arguments)
        {
            _logger?.LogInformation("[+][{Method}] {Operation} reached.", method, operation);
            _logger?.LogInformation("[+][{Method}] {Operation} received arguments: {Arguments}.", method, operation, arguments);
        }

        public async Task<Comunity?> AddComunity(Comunity comunity, Guid creatorId)
        {
            LogOperation("POST", nameof(AddComunity), $"CommunityId:{comunity.Id}, CreatorId:{creatorId}");
            await _db.Comunities.AddAsync(comunity);
            await _db.ComunityMembers.AddAsync(new ComunityMember { ComunityId = comunity.Id, UserId = creatorId, Role = ComunityRole.Teacher });
            int rowsAdded = await _db.SaveChangesAsync();

            if (rowsAdded <= 0)
                return null;

            return comunity;
        }

        public async Task<int> GetAllComunitiesCount()
        {
            LogOperation("GET", nameof(GetAllComunitiesCount), "(aucun)");
            return await _db.Comunities.CountAsync();
        }

        public async Task<Comunity?> GetComunityById(Guid comunityId)
        {
            LogOperation("GET", nameof(GetComunityById), $"CommunityId:{comunityId}");
            return await _db.Comunities.FindAsync(comunityId);
        
        }

        public async Task<List<Comunity>> GetAllComunities()
        {
            LogOperation("GET", nameof(GetAllComunities), "(aucun)");
            return await _db.Comunities.AsNoTracking().ToListAsync();
        }

        public async Task<bool> UserExistsInComunity(Guid userId, Guid comunityId)
        {
            LogOperation("GET", nameof(UserExistsInComunity), $"UserId:{userId}, CommunityId:{comunityId}");
            Comunity? targetedComunity = await GetComunityById(comunityId);
            if(targetedComunity == null)
                return false;

            return targetedComunity?.Users?.Any(u => u.UserId == userId) ?? false;
        }

        public async Task<int> GetAllComunityPostsCount(Guid comId)
        {
           LogOperation("GET", nameof(GetAllComunityPostsCount), $"CommunityId:{comId}");
           return await _db.Posts.CountAsync(p => p.CommunityId == comId);
        }

        public async Task<Comunity?> UpdateComunity(Comunity comunity)
        {
            LogOperation("POST", nameof(UpdateComunity), $"CommunityId:{comunity.Id}");
            Comunity? matchingCom = await _db.Comunities.FirstOrDefaultAsync(c => c.Id == comunity.Id);

            if (matchingCom == null)
                return null;

            matchingCom.Name = comunity.Name;
            matchingCom.Description = comunity.Description;
            // Update other properties as needed
            int rowsUpdated = await _db.SaveChangesAsync();

            if (rowsUpdated <= 0)
                return null;

            return matchingCom;
        }

        public async Task<bool> DeleteComunity(Guid comunityId)
        {
            LogOperation("POST", nameof(DeleteComunity), $"CommunityId:{comunityId}");
            Comunity? targetedCom = await _db.Comunities.FindAsync(comunityId);
            if (targetedCom == null)
                return false;

            _db.Comunities.Remove(targetedCom);
            int rowsDeleted = await _db.SaveChangesAsync();
            return rowsDeleted > 0;
        }

        public async Task<List<Comunity>> GetFilteredComunities(Expression<Func<Comunity, bool>> predicate)
        {
            LogOperation("GET", nameof(GetFilteredComunities), "predicate: reçu");
            return await _db.Comunities.Where(predicate).ToListAsync();
        }
    }
}
