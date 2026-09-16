using System;
using Entities;
using Entities.Enums;
using ServiceContracts.DTO.Users;

namespace ServiceContracts
{
    /// <summary>
    /// This is the represenation of the data layer of the UsersService.
    /// </summary>
    public interface IUsersService
    {
        /// <summary>
        /// Seed some mock users data inside the UsersService
        /// </summary>
        void SeedMockUsers();

        /// <summary>
        /// Add a user to the UsersService
        /// </summary>
        /// <param name="addUserRequest">A DTO object of type AddUserRequest which contains the adding informations of the User object</param>
        /// <returns>Returns an DTO object of type UserResponse that contains all the informations of the user</returns>
        UserResponse AddUser(AddUserRequest? addUserRequest);

        /// <summary>
        /// Get all the users contained inside the UsersService
        /// </summary>
        /// <returns>Returns a list of DTO object of type UserResponse that contains all the informations of the user</returns>
        List<UserResponse> GetAllUsers();

        /// <summary>
        /// Get the counts of all the users contained inside the UsersService
        /// </summary>
        /// <returns>An integer representing the number of users in the system</returns>
        int GetAllUsersCount();


        /// <summary>
        /// Get all the users filter by a property 
        /// </summary>
        /// <param name="searchBy">The property name that will filter in</param>
        /// <param name="searchString">The content that will be filtered</param>
        /// <returns>Returns a list of DTO object of type UserResponse that contains all the informations of the user filtered by property field</returns>
        List<UserResponse> GetFilteredUsers(string searchBy, string searchString);

        /// <summary>
        /// Get all the users sorted by property name and by order
        /// </summary>
        /// <param name="allUserResponses">The current state of the user responses list</param>
        /// <param name="sortBy">The property field to sort by</param>
        /// <param name="sortOrder">The given order to sort in, ASC or DESC</param>
        /// <returns>Returns a list of DTO object of type UserResponse that contains all the informations of the user sorted by property field</returns>
        List<UserResponse> GetSortedUsers(List<UserResponse> allUserResponses, string sortBy, SortOption sortOrder);

        /// <summary>
        /// Get the desired user contained inside the UsersService
        /// </summary>
        /// <param name="userId">The desired Guid of the user to get</param>
        /// <returns>Returns an DTO object of type UserResponse that contains all the informations of the user</returns>
        UserResponse GetUserById(Guid userId);

        /// <summary>
        /// Update the desired user contained inside the UsersService
        /// </summary>
        /// <param name="updateUserRequest">A DTO object of type UpdateUserRequest which contains the updating informations of the User object</param>
        /// <returns>Returns an DTO object of type UserResponse that contains all the informations of the user</returns>
        UserResponse UpdateUser(UpdateUserRequest? updateUserRequest);

        /// <summary>
        ///  Delete the desired user contained inside the UsersService
        /// </summary>
        /// <param name="userId">The desired Guid of the user to delete</param>
        /// <returns>True if the operation is a success, otherwise False</returns>
        bool DeleteUser(Guid userId);

    }
}
