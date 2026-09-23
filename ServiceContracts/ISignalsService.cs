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
        SignalResponse AddSignal(AddSignalRequest request);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<SignalResponse> GetAllSignals();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        SignalResponse GetSignalById(Guid id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        SignalResponse UpdateSignal(UpdateSignalRequest request);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool DeleteSignal(Guid id);
    }
}
