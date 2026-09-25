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
    private readonly IEncryptionsService _encryptionsService;
    private readonly ApplicationDbContext _db;

    public UsersService(ApplicationDbContext db, IEncryptionsService encryptionsService) { _db = db; _encryptionsService = encryptionsService; }

    // Keeps the service easy to instantiate in isolated unit tests; application DI supplies SQL Server.
    //public UsersService() : this(new UsersDbContext(new DbContextOptionsBuilder<UsersDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options), new IEncryptionsService()) { }

    public async Task<UserResponse> AddUser(AddUserRequest? request)
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
        if (await _db.Users.AnyAsync(u => u.Email == request.Email)) 
            throw new DuplicateNameException(nameof(request.Email));

        // Create the users in the db
        User user = request.ToUser();
        user.Password = await _encryptionsService.EncryptData(user.Password!);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Return the DTO response
        return user.ToUserAddResponse();
    }

    public async Task<List<UserResponse>> GetAllUsers() => (await _db.Users.AsNoTracking().ToListAsync()).Select(u => u.ToUserAddResponse()).ToList();

    public async Task<int> GetAllUsersCount() => await _db.Users.CountAsync();

    public async Task<UserResponse> GetUserById(Guid userId)
    {
        // Check if the id is valid
        if (userId == Guid.Empty) 
            throw new ArgumentNullException(nameof(userId));

        // Search for the user
        User user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId) ?? throw new ArgumentNullException(nameof(userId));
        
        // Return the DTO response
        return user.ToUserAddResponse();
    }

    public async Task<UserResponse> UpdateUser(UpdateUserRequest? request)
    {
        // Check if the request is null
        ArgumentNullException.ThrowIfNull(request);

        // Search the targeted user
        User user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == request.UserId) ?? throw new ArgumentNullException(nameof(request.UserId));
        
        // Look for duplicate users
        if (await _db.Users.AnyAsync(u => u.UserId != user.UserId && u.Email == request.Email))
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
        await _db.SaveChangesAsync();

        // Return the DTO response
        return user.ToUserAddResponse();
    }

    public async Task<bool> DeleteUser(Guid userId)
    {
        // Search the targeted user
        User? user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);

        // Check if we found a user
        if (user is null)
            return false;

        // Delete the user
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<UserResponse>> GetFilteredUsers(string searchBy, string searchString)
    {
        // Get all users first
        List<User> users = await _db.Users.AsNoTracking().ToListAsync();

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

    public Task<List<UserResponse>> GetSortedUsers(List<UserResponse> users, string sortBy, SortOption sortOrder)
    {
        // Checking if the sortBy field and empty or null
        if (string.IsNullOrEmpty(sortBy)) 
            return Task.FromResult(users);

        // Check if the sort order is descending
        bool descending = sortOrder != SortOption.ASC;

        // Sort by the sortBy and sort order
        List<UserResponse> sortedUsers = sortBy switch
        {
            nameof(UserResponse.Name) => descending ? users.OrderByDescending(u => u.Name).ToList() : users.OrderBy(u => u.Name).ToList(),
            nameof(UserResponse.Email) => descending ? users.OrderByDescending(u => u.Email).ToList() : users.OrderBy(u => u.Email).ToList(),
            nameof(UserResponse.Role) => descending ? users.OrderByDescending(u => u.Role).ToList() : users.OrderBy(u => u.Role).ToList(),
            nameof(UserResponse.UserState) => descending ? users.OrderByDescending(u => u.UserState).ToList() : users.OrderBy(u => u.UserState).ToList(),
            nameof(UserResponse.DateOfBirth) => descending ? users.OrderByDescending(u => u.DateOfBirth).ToList() : users.OrderBy(u => u.DateOfBirth).ToList(),
            _ => users
        };
        return Task.FromResult(sortedUsers);
    }
}
