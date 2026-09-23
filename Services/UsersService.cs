using Entities;
using Entities.Contexts;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO.Users;
using System.Data;

namespace Services;

public class UsersService : IUsersService
{
    private readonly UsersDbContext _db;

    public UsersService(UsersDbContext db) => _db = db;

    // Keeps the service easy to instantiate in isolated unit tests; application DI supplies SQL Server.
    public UsersService() : this(new UsersDbContext(new DbContextOptionsBuilder<UsersDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)) { }

    public UserResponse AddUser(AddUserRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Helpers.HelpersValidation.ModelValidation(request);
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password)) throw new ArgumentException("Name, email and password are required.");
        if (request.Password != request.ConfirmPassword) throw new ArgumentException("Passwords do not match.", nameof(request.ConfirmPassword));
        if (_db.Users.Any(u => u.Email == request.Email)) throw new DuplicateNameException(nameof(request.Email));
        var user = request.ToUser();
        _db.Users.Add(user);
        _db.SaveChanges();
        return user.ToUserAddResponse();
    }

    public List<UserResponse> GetAllUsers() => _db.Users.AsNoTracking().ToList().Select(u => u.ToUserAddResponse()).ToList();

    public int GetAllUsersCount() => _db.Users.Count();

    public UserResponse GetUserById(Guid userId)
    {
        if (userId == Guid.Empty) throw new ArgumentNullException(nameof(userId));
        var user = _db.Users.AsNoTracking().FirstOrDefault(u => u.UserId == userId) ?? throw new ArgumentNullException(nameof(userId));
        return user.ToUserAddResponse();
    }

    public UserResponse UpdateUser(UpdateUserRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var user = _db.Users.FirstOrDefault(u => u.UserId == request.UserId) ?? throw new ArgumentNullException(nameof(request.UserId));
        if (_db.Users.Any(u => u.UserId != user.UserId && u.Email == request.Email)) throw new DuplicateNameException(nameof(request.Email));
        if (request.Password != request.ConfirmPassword) throw new ArgumentException(nameof(request.ConfirmPassword));
        Helpers.HelpersValidation.ModelValidation(request);
        user.Name = request.Name;
        user.Email = request.Email;
        user.DateOfBirth = request.DateOfBirth;
        user.ReceiveNewsLetter = request.ReceiveNewsLetter;
        if (!string.IsNullOrWhiteSpace(request.Password)) user.Password = request.Password;
        user.Role = request.Role;
        user.UserState = request.UserState;
        _db.SaveChanges();
        return user.ToUserAddResponse();
    }

    public bool DeleteUser(Guid userId)
    {
        var user = _db.Users.FirstOrDefault(u => u.UserId == userId);
        if (user is null) return false;
        _db.Users.Remove(user);
        _db.SaveChanges();
        return true;
    }

    public List<UserResponse> GetFilteredUsers(string searchBy, string searchString)
    {
        var users = _db.Users.AsNoTracking().ToList();
        if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString)) return users.Select(u => u.ToUserAddResponse()).ToList();
        var filtered = searchBy switch
        {
            nameof(UserResponse.Name) => users.Where(u => u.Name?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true),
            nameof(UserResponse.Email) => users.Where(u => u.Email?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true),
            nameof(UserResponse.Role) => users.Where(u => u.Role.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)),
            nameof(UserResponse.UserState) => users.Where(u => u.UserState.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)),
            nameof(UserResponse.DateOfBirth) => users.Where(u => u.DateOfBirth?.ToString("dd MMM yyyy").Contains(searchString, StringComparison.OrdinalIgnoreCase) == true),
            _ => users
        };
        return filtered.Select(u => u.ToUserAddResponse()).ToList();
    }

    public List<UserResponse> GetSortedUsers(List<UserResponse> users, string sortBy, SortOption sortOrder)
    {
        if (string.IsNullOrEmpty(sortBy)) return users;
        var descending = sortOrder != SortOption.ASC;
        return sortBy switch
        {
            nameof(UserResponse.Name) => descending ? users.OrderByDescending(u => u.Name).ToList() : users.OrderBy(u => u.Name).ToList(),
            nameof(UserResponse.Email) => descending ? users.OrderByDescending(u => u.Email).ToList() : users.OrderBy(u => u.Email).ToList(),
            nameof(UserResponse.Role) => descending ? users.OrderByDescending(u => u.Role).ToList() : users.OrderBy(u => u.Role).ToList(),
            nameof(UserResponse.UserState) => descending ? users.OrderByDescending(u => u.UserState).ToList() : users.OrderBy(u => u.UserState).ToList(),
            nameof(UserResponse.DateOfBirth) => descending ? users.OrderByDescending(u => u.DateOfBirth).ToList() : users.OrderBy(u => u.DateOfBirth).ToList(),
            _ => users
        };
    }
}
