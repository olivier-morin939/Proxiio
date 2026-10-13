using ServiceContracts.DTO.Signals;

namespace ServiceContracts
{
    /// <summary>
    /// This is the represenation of the data layer of the SignalsService.
    /// </summary>
    public interface ISignalsService
    {
        /// <summary>
        /// Adds a new signal based on the provided request data.
        /// </summary>
        /// <param name="request">The request object containing the details of the signal to add.</param>
        /// <returns>A SignalResponse object representing the newly created signal.</returns>
        Task<SignalResponse> AddSignal(AddSignalRequest request);

        /// <summary>
        /// Retrieves a list of all signals as response DTOs.
        /// </summary>
        /// <returns>A list containing all signal responses.</returns>
        Task<List<SignalResponse>> GetAllSignals();

        /// <summary>
        /// Retrieves a specific signal response based on its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier (Guid) of the signal to find.</param>
        /// <returns>The matching SignalResponse object, or null if not found.</returns>
        Task<SignalResponse> GetSignalById(Guid id);

        /// <summary>
        /// Updates an existing signal using the provided update request data.
        /// </summary>
        /// <param name="request">The request object containing the updated signal details.</param>
        /// <returns>A SignalResponse object representing the updated signal.</returns>
        Task<SignalResponse> UpdateSignal(UpdateSignalRequest request);

        /// <summary>
        /// Deletes a signal based on its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier (Guid) of the signal to delete.</param>
        /// <returns>True if the deletion was successful, otherwise false.</returns>
        Task<bool> DeleteSignal(Guid id);
    }
}
