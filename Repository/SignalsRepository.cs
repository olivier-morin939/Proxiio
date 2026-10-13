using Entities;
using Entities.Contexts;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace Repository
{
    public class SignalsRepository : ISignalsRepository
    {

        private readonly ApplicationDbContext _db;

        public SignalsRepository(ApplicationDbContext db)
        {
            this._db = db;
        }

        public async Task<Signal?> AddSignal(Signal newSignal)
        {
            await _db.AddAsync(newSignal);
            int rowsAdded = await _db.SaveChangesAsync();

            if (rowsAdded <= 0)
                return null;

            return newSignal;
        }

        public async Task<bool> DeleteSignal(Guid id)
        {
            Signal? matchingSignal = await _db.Signals.FirstOrDefaultAsync(s => s.Id == id);

            if (matchingSignal == null)
                return false;

            _db.Signals.Remove(matchingSignal);
            int rowDeleted = await _db.SaveChangesAsync();

            return rowDeleted > 0;
        }

        public async Task<List<Signal>> GetAllSignals()
        {
            return await _db.Signals.ToListAsync();
        }

        public async Task<Signal?> GetSignalById(Guid id)
        {
            return await _db.Signals.FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Signal?> UpdateSignal(Signal signalToUpdate)
        {
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
