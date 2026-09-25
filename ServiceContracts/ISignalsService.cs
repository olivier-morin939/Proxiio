using ServiceContracts.DTO.Signals;

namespace ServiceContracts
{
    /// <summary>
    /// This is the represenation of the data layer of the SignalsService.
    /// </summary>
    public interface ISignalsService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SignalResponse> AddSignal(AddSignalRequest request);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<List<SignalResponse>> GetAllSignals();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<SignalResponse> GetSignalById(Guid id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SignalResponse> UpdateSignal(UpdateSignalRequest request);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> DeleteSignal(Guid id);
    }
}
