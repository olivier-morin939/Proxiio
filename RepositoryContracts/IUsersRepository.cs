using Entities;
using System.Linq.Expressions;
namespace RepositoryContracts
{
    public interface IUsersRepository
    {

       /// <summary>
       /// Add a user to the data store
       /// </summary>
       /// <param name="user">The new user to insert</param>
       /// <returns>Returns the newly created user</returns>
        Task<User> AddUser(User user);

      
        /// <summary>
        /// Get all users from the data store
        /// </summary>
        /// <returns>Returns all the users</returns>
        Task<List<User>> GetAllUsers();

        /// <summary>
        /// Get the users count of the data store
        /// </summary>
        /// <returns>Return the count of the users(int)</returns>
        Task<int> GetAllUsersCount();

         /// <summary>
         /// Get all users based on the filter expression
         /// </summary>
         /// <param name="predicate">The LINQ expression to executre</param>
         /// <returns>Returns the users filtered with the LINQ expression</returns>
        Task<List<User>> GetFilteredUsers(Expression<Func<User, bool>> predicate);

        /// <summary>
        /// Get the targeted user in the data store
        /// </summary>
        /// <param name="userId">The targeted user id (GUID)</param>
        /// <returns>Returns the found user or null</returns>
        Task<User?> GetUserById(Guid userId);

        /// <summary>
        /// Update the targeted user in the data store
        /// </summary>
        /// <param name="user">The user to update</param>
        /// <returns>Returns the newly updated user or null</returns>
        Task<User?> UpdateUser(User user);

        /// <summary>
        /// Delete the targeted user in the data store
        /// </summary>
        /// <param name="userId">The targeted user id (GUID)</param>
        /// <returns>Returns True if the operation was successfull, otherwise False.</returns>
        Task<bool> DeleteUser(Guid userId);
    }
}
