using System;
using ServiceContracts.DTO;

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
        /// Get all the courses contained inside the UsersService
        /// </summary>
        /// <returns>Returns a list of DTO object of type UserResponse that contains all the informations of the user</returns>
        List<UserResponse> GetAllUsers();

        /// <summary>
        /// Get the desired course contained inside the UsersService
        /// </summary>
        /// <param name="userId">The desired Guid of the user to get</param>
        /// <returns>Returns an DTO object of type UserResponse that contains all the informations of the user</returns>
        UserResponse GetUserById(Guid userId);

        /// <summary>
        /// Update the desired course contained inside the UsersService
        /// </summary>
        /// <param name="updateUserRequest">A DTO object of type UpdateUserRequest which contains the updating informations of the User object</param>
        /// <returns>Returns an DTO object of type UserResponse that contains all the informations of the user</returns>
        UserResponse UpdateUser(UpdateUserRequest? updateUserRequest);

        /// <summary>
        ///  Delete the desired course contained inside the UsersService
        /// </summary>
        /// <param name="userId">The desired Guid of the user to delete</param>
        /// <returns>True if the operation is a success, otherwise False</returns>
        bool DeleteUser(Guid userId);

    }
}
