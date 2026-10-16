using Entities;
using Entities.Contexts;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
namespace Repository
{
    /// <summary>
    /// The access to the data store of the User entity
    /// </summary>
    public class UsersRepository : IUsersRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<UsersRepository>? _logger;


        public UsersRepository(ApplicationDbContext db, ILogger<UsersRepository>? logger = null)
        {
            _db = db;
            _logger = logger;
        }

        private void LogOperation(string method, string operation, string arguments)
        {
            _logger?.LogInformation("[+][{Method}] {Operation} reached.", method, operation);
            _logger?.LogInformation("[+][{Method}] {Operation} received arguments: {Arguments}.", method, operation, arguments);
        }

        public async Task<User?> AddUser(User user)
        {
            LogOperation("POST", nameof(AddUser), $"UserId:{user.UserId}");

            await _db.AddAsync(user);
            int rowsAdded = await _db.SaveChangesAsync();

            if (rowsAdded <= 0)
                return null;

            return user;


        }

        public async Task<bool> DeleteUser(Guid userId)
        {
            LogOperation("POST", nameof(DeleteUser), $"UserId:{userId}");
            User? matchingUser = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            
            if (matchingUser == null)
                return false;

            _db.Users.Remove(matchingUser);
            int rowDeleted =  await _db.SaveChangesAsync();

            return rowDeleted > 0;

        }

        public async Task<List<User>> GetAllUsers()
        {
            LogOperation("GET", nameof(GetAllUsers), "(aucun)");
            return await _db.Users.ToListAsync();
        }

        public async Task<int> GetAllUsersCount()
        {
            LogOperation("GET", nameof(GetAllUsersCount), "(aucun)");
            return await _db.Users.CountAsync();
        }

        public async Task<List<User>> GetFilteredUsers(Expression<Func<User, bool>> predicate)
        {
            LogOperation("GET", nameof(GetFilteredUsers), "predicate: reçu");
            return await _db.Users.Where(predicate).ToListAsync();
        }

        public async Task<User?> GetUserById(Guid userId)
        {
            LogOperation("GET", nameof(GetUserById), $"UserId:{userId}");
            return await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User?> UpdateUser(User user)
        {
            LogOperation("POST", nameof(UpdateUser), $"UserId:{user.UserId}");
            User? matchingUser = await _db.Users.FirstOrDefaultAsync(u => u.UserId == user.UserId);

            if (matchingUser == null)
                return null;

            matchingUser.Name = user.Name;
            matchingUser.Email = user.Email;
            matchingUser.Role = user.Role;
            matchingUser.UserState = user.UserState;
            matchingUser.DateOfBirth = user.DateOfBirth;
            matchingUser.ReceiveNewsLetter = user.ReceiveNewsLetter;
            int rowsUpdated = await _db.SaveChangesAsync();

            if (rowsUpdated <= 0)
                return null;

            return matchingUser;

        }
    }
}
