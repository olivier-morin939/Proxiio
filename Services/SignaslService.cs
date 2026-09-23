using Entities.Contexts;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO.Signals;

namespace Services;

public class SignaslService : ISignalsService
{
    private readonly UsersDbContext _db;
    public SignaslService(UsersDbContext db) => _db = db;
    public SignaslService() : this(new UsersDbContext(new DbContextOptionsBuilder<UsersDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)) { }

    public SignalResponse AddSignal(AddSignalRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Helpers.HelpersValidation.ModelValidation(request);
        var signal = request.ToSignal();
        _db.Signals.Add(signal);
        _db.SaveChanges();
        return SignalResponse.From(signal);
    }

    public List<SignalResponse> GetAllSignals() => _db.Signals.AsNoTracking().OrderByDescending(s => s.Id).ToList().Select(SignalResponse.From).ToList();

    public SignalResponse GetSignalById(Guid id)
    {
        var signal = _db.Signals.AsNoTracking().FirstOrDefault(s => s.Id == id) ?? throw new KeyNotFoundException($"Signal {id} was not found.");
        return SignalResponse.From(signal);
    }

    public SignalResponse UpdateSignal(UpdateSignalRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Helpers.HelpersValidation.ModelValidation(request);
        var signal = _db.Signals.FirstOrDefault(s => s.Id == request.Id) ?? throw new KeyNotFoundException($"Signal {request.Id} was not found.");
        signal.ProblemName = request.ProblemName;
        signal.ProblemDescription = request.ProblemDescription;
        signal.Level = request.Level;
        signal.Status = request.Status;
        signal.IsConfirmed = request.IsConfirmed;
        _db.SaveChanges();
        return SignalResponse.From(signal);
    }

    public bool DeleteSignal(Guid id)
    {
        var signal = _db.Signals.FirstOrDefault(s => s.Id == id);
        if (signal is null) return false;
        _db.Signals.Remove(signal);
        _db.SaveChanges();
        return true;
    }
}
