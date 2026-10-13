using Entities;
using Entities.Contexts;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO.Signals;

namespace Services;

public class SignaslService : ISignalsService
{
    private readonly ISignalsRepository _repository;
    public SignaslService(ISignalsRepository repository) => _repository = repository;

    public async Task<SignalResponse> AddSignal(AddSignalRequest request)
    {

        ArgumentNullException.ThrowIfNull(request);

        Helpers.HelpersValidation.ModelValidation(request);

        Signal signalToAdd = request.ToSignal();

        Signal? addedSignal = await _repository.AddSignal(signalToAdd);

        if (addedSignal == null)
            throw new InvalidOperationException("The signal could not be added.");

        return SignalResponse.From(addedSignal);
    }

    public async Task<List<SignalResponse>> GetAllSignals() => (await _repository.GetAllSignals()).Select(s => s.ToSignalResponse()).ToList();

    public async Task<SignalResponse> GetSignalById(Guid id)
    {
        // Find the corresponding signal and convert it in the DTO response
        Signal signal = await _repository.GetSignalById(id) ?? throw new KeyNotFoundException($"Signal {id} was not found.");
        return SignalResponse.From(signal);
    }

    public async Task<SignalResponse> UpdateSignal(UpdateSignalRequest request)
    {

        ArgumentNullException.ThrowIfNull(request);


        Helpers.HelpersValidation.ModelValidation(request);

        Signal matchingSignal = await _repository.GetSignalById(request.Id) ?? throw new KeyNotFoundException($"Signal {request.Id} was not found.");
        Signal signalToUpdate = request.ToSignal();
        Signal? updatedSignal = await _repository.UpdateSignal(signalToUpdate);

        if (updatedSignal == null)
            throw new InvalidOperationException("The signal could not be updated.");


        return updatedSignal.ToSignalResponse();
    }

    public async Task<bool> DeleteSignal(Guid id)
    {

        Signal? signalToDelete = await _repository.GetSignalById(id);


        if (signalToDelete is null)
            return false;

        bool isDeleted = await _repository.DeleteSignal(signalToDelete.Id);

        if (!isDeleted)
            throw new InvalidOperationException("This signal could not be deleted.");

        return true;
    }
}
