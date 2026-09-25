using Entities;
using Entities.Contexts;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO.Signals;

namespace Services;

public class SignaslService : ISignalsService
{
    private readonly ApplicationDbContext _db;
    public SignaslService(ApplicationDbContext db) => _db = db;

    // Keeps the service easy to instantiate in isolated unit tests; application DI supplies SQL Server.
    public SignaslService() : this(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)) { }

    public async Task<SignalResponse> AddSignal(AddSignalRequest request)
    {
        // Check if the DTO object is null
        ArgumentNullException.ThrowIfNull(request);

        // Model validation for the DTO object
        Helpers.HelpersValidation.ModelValidation(request);

        // Converting the request into a entity class
        Signal signal = request.ToSignal();

        // Add the object to the db
        _db.Signals.Add(signal);
        await _db.SaveChangesAsync();

        // Return the DTO response
        return SignalResponse.From(signal);
    }

    public async Task<List<SignalResponse>> GetAllSignals() => (await _db.Signals.AsNoTracking().OrderByDescending(s => s.Id).ToListAsync()).Select(SignalResponse.From).ToList();

    public async Task<SignalResponse> GetSignalById(Guid id)
    {
        // Find the corresponding signal and convert it in the DTO response
        Signal signal = await _db.Signals.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id) ?? throw new KeyNotFoundException($"Signal {id} was not found.");
        return SignalResponse.From(signal);
    }

    public async Task<SignalResponse> UpdateSignal(UpdateSignalRequest request)
    {
        // Check if the DTO object is null
        ArgumentNullException.ThrowIfNull(request);

        // Model validation for the DTO object
        Helpers.HelpersValidation.ModelValidation(request);

        // Searching for the corresponding signal
        Signal signal = await _db.Signals.FirstOrDefaultAsync(s => s.Id == request.Id) ?? throw new KeyNotFoundException($"Signal {request.Id} was not found.");
        
        // Fields updation
        signal.ProblemName = request.ProblemName;
        signal.ProblemDescription = request.ProblemDescription;
        signal.Level = request.Level;
        signal.Status = request.Status;
        signal.IsConfirmed = request.IsConfirmed;
        await _db.SaveChangesAsync();

        // Return the DTO response
        return SignalResponse.From(signal);
    }

    public async Task<bool> DeleteSignal(Guid id)
    {
        // Searche the corresponding signal
        Signal? signal = await _db.Signals.FirstOrDefaultAsync(s => s.Id == id);

        // If it is not found
        if (signal is null)
            return false;

        // Remove it from db
        _db.Signals.Remove(signal);
        await _db.SaveChangesAsync();
        return true;
    }
}
