using Entities;
using Entities.Contexts;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
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


        public UsersRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<User> AddUser(User user)
        {
            if(user == null)
                throw new ArgumentNullException(nameof(user));
            _db.Add(user);
            await _db.SaveChangesAsync();
            return user;


        }

        public async Task<bool> DeleteUser(Guid userId)
        {
            User? matchingUser = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            
            if (matchingUser == null)
                return false;

            _db.Users.Remove(matchingUser);
            int rowDeleted =  await _db.SaveChangesAsync();

            return rowDeleted > 0;

        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _db.Users.ToListAsync();
        }

        public async Task<int> GetAllUsersCount()
        {
            return await _db.Users.CountAsync();
        }

        public async Task<List<User>> GetFilteredUsers(Expression<Func<User, bool>> predicate)
        {
            return await _db.Users.Where(predicate).ToListAsync();
        }

        public async Task<User?> GetUserById(Guid userId)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User?> UpdateUser(User user)
        {
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

            return matchingUser;

        }
    }
}
