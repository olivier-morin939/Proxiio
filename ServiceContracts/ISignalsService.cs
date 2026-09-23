using ServiceContracts.DTO.Signals;

namespace ServiceContracts;

public interface ISignalsService
{
    SignalResponse AddSignal(AddSignalRequest request);
    List<SignalResponse> GetAllSignals();
    SignalResponse GetSignalById(Guid id);
    SignalResponse UpdateSignal(UpdateSignalRequest request);
    bool DeleteSignal(Guid id);
}
