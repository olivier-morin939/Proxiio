using Entities;
using Entities.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepositoryContracts;

namespace Repository
{
    public class SignalsRepository : ISignalsRepository
    {

        private readonly ApplicationDbContext _db;
        private readonly ILogger<SignalsRepository>? _logger;

        public SignalsRepository(ApplicationDbContext db, ILogger<SignalsRepository>? logger = null)
        {
            this._db = db;
            _logger = logger;
        }

        private void LogOperation(string method, string operation, string arguments)
        {
            _logger?.LogInformation("[+][{Method}] {Operation} reached.", method, operation);
            _logger?.LogInformation("[+][{Method}] {Operation} received arguments: {Arguments}.", method, operation, arguments);
        }

        public async Task<Signal?> AddSignal(Signal newSignal)
        {
            LogOperation("POST", nameof(AddSignal), $"SignalId:{newSignal.Id}");
            await _db.AddAsync(newSignal);
            int rowsAdded = await _db.SaveChangesAsync();

            if (rowsAdded <= 0)
                return null;

            return newSignal;
        }

        public async Task<bool> DeleteSignal(Guid id)
        {
            LogOperation("POST", nameof(DeleteSignal), $"SignalId:{id}");
            Signal? matchingSignal = await _db.Signals.FirstOrDefaultAsync(s => s.Id == id);

            if (matchingSignal == null)
                return false;

            _db.Signals.Remove(matchingSignal);
            int rowDeleted = await _db.SaveChangesAsync();

            return rowDeleted > 0;
        }

        public async Task<List<Signal>> GetAllSignals()
        {
            LogOperation("GET", nameof(GetAllSignals), "(aucun)");
            return await _db.Signals.ToListAsync();
        }

        public async Task<Signal?> GetSignalById(Guid id)
        {
            LogOperation("GET", nameof(GetSignalById), $"SignalId:{id}");
            return await _db.Signals.FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Signal?> UpdateSignal(Signal signalToUpdate)
        {
            LogOperation("POST", nameof(UpdateSignal), $"SignalId:{signalToUpdate.Id}");
            Signal? matchingSignal = await _db.Signals.FirstOrDefaultAsync(s => s.Id == signalToUpdate.Id);

            if (matchingSignal == null)
                return null;

            matchingSignal.IsConfirmed = signalToUpdate.IsConfirmed;
            matchingSignal.Level = signalToUpdate.Level;
            matchingSignal.ProblemDescription = signalToUpdate.ProblemDescription;
            matchingSignal.ProblemName = signalToUpdate.ProblemName;
            matchingSignal.Status = signalToUpdate.Status;
            int rowsUpdated = await _db.SaveChangesAsync();

            if (rowsUpdated <= 0)
                return null;

            return matchingSignal;
        }
    }
}
