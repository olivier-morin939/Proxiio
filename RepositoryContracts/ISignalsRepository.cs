using Entities;
namespace RepositoryContracts
{
    public interface ISignalsRepository
    {

        /// <summary>
        /// Adds a new signal to the database.
        /// </summary>
        /// <param name="newSignal">The Signal object to add.</param>
        /// <returns>The newly created signal with its generated identifier.</returns>
        Task<Signal?> AddSignal(Signal newSignal);

        /// <summary>
        /// Retrieves a list of all recorded signals.
        /// </summary>
        /// <returns>A list containing all signals.</returns>
        Task<List<Signal>> GetAllSignals();

        /// <summary>
        /// Retrieves a specific signal based on its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier (Guid) of the signal to find.</param>
        /// <returns>The matching Signal object, or null if not found.</returns>
        Task<Signal?> GetSignalById(Guid id);

        /// <summary>
        /// Updates the information of an existing signal.
        /// </summary>
        /// <param name="signalToUpdate">The Signal object containing the updated data.</param>
        /// <returns>The updated signal.</returns>
        Task<Signal?> UpdateSignal(Signal signalToUpdate);

        /// <summary>
        /// Deletes a signal from the database based on its identifier.
        /// </summary>
        /// <param name="id">The unique identifier (Guid) of the signal to delete.</param>
        /// <returns>True if the deletion was successful, otherwise false.</returns>
        Task<bool> DeleteSignal(Guid id);

    }
}
