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
        // Check if the DTO object is null
        ArgumentNullException.ThrowIfNull(request);

        // Model validation for the DTO object
        Helpers.HelpersValidation.ModelValidation(request);

        // Validating the mandatory fields
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password)) 
            throw new ArgumentException("Name, email and password are required.");

        // Check if the password and confirm password match
        if (request.Password != request.ConfirmPassword) 
            throw new ArgumentException("Passwords do not match.", nameof(request.ConfirmPassword));

        // Look for duplicates
        if (_db.Users.Any(u => u.Email == request.Email)) 
            throw new DuplicateNameException(nameof(request.Email));

        // Create the users in the db
        User user = request.ToUser();
        _db.Users.Add(user);
        _db.SaveChanges();

        // Return the DTO response
        return user.ToUserAddResponse();
    }

    public List<UserResponse> GetAllUsers() => _db.Users.AsNoTracking().ToList().Select(u => u.ToUserAddResponse()).ToList();

    public int GetAllUsersCount() => _db.Users.Count();

    public UserResponse GetUserById(Guid userId)
    {
        // Check if the id is valid
        if (userId == Guid.Empty) 
            throw new ArgumentNullException(nameof(userId));

        // Search for the user
        User user = _db.Users.AsNoTracking().FirstOrDefault(u => u.UserId == userId) ?? throw new ArgumentNullException(nameof(userId));
        
        // Return the DTO response
        return user.ToUserAddResponse();
    }

    public UserResponse UpdateUser(UpdateUserRequest? request)
    {
        // Check if the request is null
        ArgumentNullException.ThrowIfNull(request);

        // Search the targeted user
        User user = _db.Users.FirstOrDefault(u => u.UserId == request.UserId) ?? throw new ArgumentNullException(nameof(request.UserId));
        
        // Look for duplicate users
        if (_db.Users.Any(u => u.UserId != user.UserId && u.Email == request.Email))
            throw new DuplicateNameException(nameof(request.Email));

        // Look if the password and confirm password match
        if (request.Password != request.ConfirmPassword) 
            throw new ArgumentException(nameof(request.ConfirmPassword));

        // Model validation for the DTO object
        Helpers.HelpersValidation.ModelValidation(request);

        // Updating the user informations
        user.Name = request.Name;
        user.Email = request.Email;
        user.DateOfBirth = request.DateOfBirth;
        user.ReceiveNewsLetter = request.ReceiveNewsLetter;

        if (!string.IsNullOrWhiteSpace(request.Password)) 
            user.Password = request.Password;

        user.Role = request.Role;
        user.UserState = request.UserState;
        _db.SaveChanges();

        // Return the DTO response
        return user.ToUserAddResponse();
    }

    public bool DeleteUser(Guid userId)
    {
        // Search the targeted user
        User? user = _db.Users.FirstOrDefault(u => u.UserId == userId);

        // Check if we found a user
        if (user is null)
            return false;

        // Delete the user
        _db.Users.Remove(user);
        _db.SaveChanges();
        return true;
    }

    public List<UserResponse> GetFilteredUsers(string searchBy, string searchString)
    {
        // Get all users first
        List<User> users = _db.Users.AsNoTracking().ToList();

        // Checking if the searchBy field and searchString is empty or null
        if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString)) 
            return users.Select(u => u.ToUserAddResponse()).ToList();

        // Filtering the informations based on the searchBy and searchString
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
        // Checking if the sortBy field and empty or null
        if (string.IsNullOrEmpty(sortBy)) 
            return users;

        // Check if the sort order is descending
        bool descending = sortOrder != SortOption.ASC;

        // Sort by the sortBy and sort order
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
