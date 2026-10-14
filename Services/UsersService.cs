using CsvHelper;
using Entities;
using Entities.Contexts;
using Entities.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO.Users;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Http;
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

        List<UserResponse> users = (await _repository.GetAllUsers()).Select(temp => temp.ToUserResponse()).ToList();


        MemoryStream memoryStream = new MemoryStream();

 
        StreamWriter streamWriter = new StreamWriter(memoryStream);


        CsvWriter csvWriter = new CsvWriter(streamWriter, culture: CultureInfo.InvariantCulture, leaveOpen: true);

  
        csvWriter.WriteHeader<UserResponse>();

        csvWriter.NextRecord();


        await csvWriter.WriteRecordsAsync(users);

   
        await streamWriter.FlushAsync();

        memoryStream.Position = 0;

        return memoryStream;

    }

    public async Task<MemoryStream> GetUsersEXCEL()
    {
        List<UserResponse> allUsers = (await _repository.GetAllUsers()).Select(user => user.ToUserResponse()).ToList();

        MemoryStream memoryStream = new MemoryStream();

        using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
        {

            ExcelWorksheet workSheet = excelPackage.Workbook.Worksheets.Add("UsersSheet");


            workSheet.Cells["A1"].Value = "User Id";
            workSheet.Cells["B1"].Value = "Name";
            workSheet.Cells["C1"].Value = "Email";
            workSheet.Cells["D1"].Value = "Role";
            workSheet.Cells["E1"].Value = "UserState";
            workSheet.Cells["F1"].Value = "Password";
            workSheet.Cells["G1"].Value = "DateOfBirth";
            workSheet.Cells["H1"].Value = "ReceiveNewsLetter";

            int row = 2;

            foreach(UserResponse userResponse in allUsers)
            {
                workSheet.Cells[$"A{row}"].Value = userResponse.UserId.ToString();
                workSheet.Cells[$"B{row}"].Value = userResponse.Name;
                workSheet.Cells[$"C{row}"].Value = userResponse.Email;
                workSheet.Cells[$"D{row}"].Value = userResponse.Role.ToString();
                workSheet.Cells[$"E{row}"].Value = userResponse.UserState.ToString();
                workSheet.Cells[$"F{row}"].Value = userResponse.Password;
                workSheet.Cells[$"G{row}"].Value = userResponse.DateOfBirth.ToString("dd MMM yyyy");
                workSheet.Cells[$"H{row}"].Value = userResponse.ReceiveNewsLetter.ToString();
                row++;
            }

            workSheet.Cells[$"A1:H{row}"].AutoFitColumns();


            await excelPackage.SaveAsync();

        }

        memoryStream.Position = 0;
        return memoryStream;

    }

    public async Task<int> UploadUsersFromExcel(IFormFile file)
    {
        using MemoryStream memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);


        memoryStream.Position = 0;

        int usersInsertedCount = 0;
        using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
        {
            ExcelWorksheet workSheet = excelPackage.Workbook.Worksheets.FirstOrDefault();
            if (workSheet?.Dimension == null)
                return 0;

            var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int col = workSheet.Dimension.Start.Column; col <= workSheet.Dimension.End.Column; col++)
            {
                string? header = Convert.ToString(workSheet.Cells[1, col].Value)?.Trim();
                if (!string.IsNullOrEmpty(header) && !headerMap.ContainsKey(header))
                    headerMap[header] = col;
            }

            string GetByHeader(int row, string headerName)
            {
                if (headerMap.TryGetValue(headerName, out int colIndex))
                    return Convert.ToString(workSheet.Cells[row, colIndex].Value) ?? string.Empty;
                return string.Empty;
            }

            var rng = new Random();
            // Load existing emails once to avoid querying DB inside the loop
            var existingEmails = (await _repository.GetAllUsers()).Where(u => !string.IsNullOrWhiteSpace(u.Email)).Select(u => u.Email!.ToLowerInvariant()).ToHashSet();
            DateTime GenerateRandomDate(Random r, int minAge = 18, int maxAge = 65)
            {
                var start = DateTime.Today.AddYears(-maxAge);
                var end = DateTime.Today.AddYears(-minAge);
                int rangeDays = (end - start).Days;
                return start.AddDays(r.Next(rangeDays + 1));
            }

            int startRow = 2;
            for (int row = startRow; row <= workSheet.Dimension.End.Row; row++)
            {
                string name = GetByHeader(row, "Name");
                string email = GetByHeader(row, "Email");
                string password = GetByHeader(row, "Password");


                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                    continue;

                // Skip if email already exists in database
                if (existingEmails.Contains(email.ToLowerInvariant()))
                    continue;

                string userIdStr = GetByHeader(row, "UserId");
                Guid userId = Guid.NewGuid();
                if (!string.IsNullOrWhiteSpace(userIdStr) && Guid.TryParse(userIdStr, out Guid parsedId) && parsedId != Guid.Empty)
                    userId = parsedId;

                string roleStr = GetByHeader(row, "Role");
                if (!Enum.TryParse<Role>(roleStr, true, out Role userRole))
                    continue;

                string stateStr = GetByHeader(row, "UserState");
                if (!Enum.TryParse<UserState>(stateStr, true, out UserState accountState))
                    continue;

                DateTime dateOfBirth = GenerateRandomDate(rng, 18, 65);

                string receiveStr = GetByHeader(row, "ReceiveNewsLetter");
                bool receiveNewsLetter = false;
                if (!string.IsNullOrWhiteSpace(receiveStr))
                    bool.TryParse(receiveStr, out receiveNewsLetter);

                string encryptedPassword = await _encryptionsService.EncryptData(password);

                

                User newUser = new User()
                {
                    UserId = userId,
                    Name = name,
                    Email = email,
                    Role = userRole,
                    UserState = accountState,
                    Password = encryptedPassword,
                    DateOfBirth = dateOfBirth,
                    ReceiveNewsLetter = receiveNewsLetter
                };

                User addedUser = await _repository.AddUser(newUser);
                if (addedUser != null)
                {
                    usersInsertedCount++;
                    // add to existingEmails to prevent duplicate insertions within same upload
                    existingEmails.Add(email.ToLowerInvariant());
                }
            }
        }

        return usersInsertedCount;
    }
}
