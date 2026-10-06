using Entities;
using Entities.Contexts;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO.Users;
using System.Data;
using CsvHelper;
using System.Globalization;
using System.IO;
namespace Services;

public class UsersService : IUsersService
{
    private readonly IEncryptionsService _encryptionsService;
    private readonly IUsersRepository _repository;

    public UsersService(IUsersRepository repository, IEncryptionsService encryptionsService) { _repository = repository; _encryptionsService = encryptionsService; }

    // Keeps the service easy to instantiate in isolated unit tests; application DI supplies SQL Server.
    //public UsersService() : this(new UsersDbContext(new DbContextOptionsBuilder<UsersDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options), new IEncryptionsService()) { }

    public async Task<UserResponse> AddUser(AddUserRequest? request)
    {
        // Check if the DTO object is null
        if(request == null)
            throw new ArgumentNullException(nameof(request));

        // Model validation for the DTO object
        Helpers.HelpersValidation.ModelValidation(request);

        // Validating the mandatory fields
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || request.DateOfBirth.ToString() == null) 
            throw new ArgumentException("Name, email, password and date of birth are required.");

        // Check if the password and confirm password match
        if (request.Password != request.ConfirmPassword) 
            throw new ArgumentException("Passwords do not match.", nameof(request.ConfirmPassword));

        // Look for duplicate users
        if ((await _repository.GetAllUsers()).Any(u => u.Email == request.Email))
            throw new DuplicateNameException(nameof(request.Email));

        // Create the users in the db
        User user = request.ToUser();
        user.Password = await _encryptionsService.EncryptData(user.Password!);
        User createdUser = await _repository.AddUser(user);


        // Return the DTO response
        return createdUser.ToUserResponse();
    }

    public async Task<List<UserResponse>> GetAllUsers() => (await _repository.GetAllUsers()).Select(u => u.ToUserResponse()).ToList();

    public async Task<int> GetAllUsersCount() => await _repository.GetAllUsersCount();

    public async Task<UserResponse?> GetUserById(Guid userId)
    {
        // Check if the id is valid
        if (userId == Guid.Empty) 
            throw new ArgumentNullException(nameof(userId));

        // Search for the user
        User? matchingUser = await _repository.GetUserById(userId);

        if (matchingUser == null)
            return null;
        
        // Return the DTO response
        return matchingUser.ToUserResponse();
    }

    public async Task<UserResponse?> UpdateUser(UpdateUserRequest? request)
    {
        // Check if the request is null
        ArgumentNullException.ThrowIfNull(request);

        // Search the targeted user
        User? matchingUser = await _repository.GetUserById(request.UserId);
        if (matchingUser == null)
            return null;


        // Look for duplicate users
        if ((await _repository.GetAllUsers()).Any(u => u.UserId != matchingUser.UserId && u.Email == request.Email))
            throw new DuplicateNameException(nameof(request.Email));

        // Look if the password and confirm password match
        if (request.Password != request.ConfirmPassword) 
            throw new ArgumentException(nameof(request.ConfirmPassword));

        // Model validation for the DTO object
        Helpers.HelpersValidation.ModelValidation(request);

        // Updating the user informations
        User? updatedUser = await _repository.UpdateUser(matchingUser);
        if (updatedUser == null)
            return null;

        // Return the DTO response
        return updatedUser.ToUserResponse();
    }

    public async Task<bool> DeleteUser(Guid userId)
    {
        // Search the targeted user
        User? user = await _repository.GetUserById(userId);

        // Check if we found a user
        if (user is null)
            return false;

        // Delete the user
        return await _repository.DeleteUser(userId);
    }

    public async Task<List<UserResponse>> GetFilteredUsers(string searchBy, string searchString)
    {
        // Get all users first
        List<User> users = await _repository.GetAllUsers();

        // Checking if the searchBy field and searchString is empty or null
        if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString)) 
            return users.Select(u => u.ToUserResponse()).ToList();




        // Filtering the informations based on the searchBy and searchString
        List<User> filteredUsers = searchBy switch
        {
            nameof(UserResponse.Name) =>
                await _repository.GetFilteredUsers(temp => (!string.IsNullOrEmpty(temp.Name) ? temp.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true)),
         
            nameof(UserResponse.Email) => 
                await _repository.GetFilteredUsers(temp => (!string.IsNullOrEmpty(temp.Email) ? temp.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true)),
            
            nameof(UserResponse.Role) =>
                await _repository.GetFilteredUsers(temp => (!string.IsNullOrEmpty(temp.Role.ToString()) ? temp.Role.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase) : true)),

            nameof(UserResponse.UserState) =>
                await _repository.GetFilteredUsers(temp => (!string.IsNullOrEmpty(temp.UserState.ToString()) ? temp.UserState.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase) : true)),

            nameof(UserResponse.DateOfBirth) => 
                await _repository.GetFilteredUsers(temp => (!string.IsNullOrEmpty(temp.DateOfBirth.ToString()) ? temp.DateOfBirth.ToString("dd MMM yyyy").Contains(searchString, StringComparison.OrdinalIgnoreCase) : true)),
               
            _ => users
        };
        return filteredUsers.Select(u => u.ToUserResponse()).ToList();
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

    public async Task<MemoryStream> GetUsersCSV()
    {
        // Get all the users
        List<UserResponse> users = await GetAllUsers();

        // Create a new memory stream
        MemoryStream memoryStream = new MemoryStream();

        // Create a new stream writer 
        StreamWriter streamWriter = new StreamWriter(memoryStream);

        // Create a new csv writer
        CsvWriter csvWriter = new CsvWriter(streamWriter, culture: CultureInfo.InvariantCulture, leaveOpen: true);

        // Write all the headers automatically
        csvWriter.WriteHeader<UserResponse>();

        csvWriter.NextRecord();

        // Write the informations
        await csvWriter.WriteRecordsAsync(users);

        // Reset the position to the beginning of the stream
        memoryStream.Position = 0;

        return memoryStream;

    }
}
