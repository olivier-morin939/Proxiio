using Entities;
using Entities.Contexts;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO.Signals;

namespace Services;

public class SignaslService : ISignalsService
{
    private readonly UsersDbContext _db;
    public SignaslService(UsersDbContext db) => _db = db;

    // Keeps the service easy to instantiate in isolated unit tests; application DI supplies SQL Server.
    public SignaslService() : this(new UsersDbContext(new DbContextOptionsBuilder<UsersDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)) { }

    public SignalResponse AddSignal(AddSignalRequest request)
    {
        // Check if the DTO object is null
        ArgumentNullException.ThrowIfNull(request);

        // Model validation for the DTO object
        Helpers.HelpersValidation.ModelValidation(request);

        // Converting the request into a entity class
        Signal signal = request.ToSignal();

        // Add the object to the db
        _db.Signals.Add(signal);
        _db.SaveChanges();

        // Return the DTO response
        return SignalResponse.From(signal);
    }

    public List<SignalResponse> GetAllSignals() => _db.Signals.AsNoTracking().OrderByDescending(s => s.Id).ToList().Select(SignalResponse.From).ToList();

    public SignalResponse GetSignalById(Guid id)
    {
        // Find the corresponding signal and convert it in the DTO response
        Signal signal = _db.Signals.AsNoTracking().FirstOrDefault(s => s.Id == id) ?? throw new KeyNotFoundException($"Signal {id} was not found.");
        return SignalResponse.From(signal);
    }

    public SignalResponse UpdateSignal(UpdateSignalRequest request)
    {
        // Check if the DTO object is null
        ArgumentNullException.ThrowIfNull(request);

        // Model validation for the DTO object
        Helpers.HelpersValidation.ModelValidation(request);

        // Searching for the corresponding signal
        Signal signal = _db.Signals.FirstOrDefault(s => s.Id == request.Id) ?? throw new KeyNotFoundException($"Signal {request.Id} was not found.");
        
        // Fields updation
        signal.ProblemName = request.ProblemName;
        signal.ProblemDescription = request.ProblemDescription;
        signal.Level = request.Level;
        signal.Status = request.Status;
        signal.IsConfirmed = request.IsConfirmed;
        _db.SaveChanges();

        // Return the DTO response
        return SignalResponse.From(signal);
    }

    public bool DeleteSignal(Guid id)
    {
        // Searche the corresponding signal
        Signal? signal = _db.Signals.FirstOrDefault(s => s.Id == id);

        // If it is not found
        if (signal is null)
            return false;

        // Remove it from db
        _db.Signals.Remove(signal);
        _db.SaveChanges();
        return true;
    }
}
