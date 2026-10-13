using Entities;
using Entities.Contexts;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace Repository
{
    public class ReportsRepository :IReportsRepository 
    {

        private readonly ApplicationDbContext _db;

        public ReportsRepository(ApplicationDbContext db)
        {
            this._db = db;
        }

        public async Task<Report?> AddReport(Report newReport)
        {
            await _db.AddAsync(newReport);
            int rowsAdded = await _db.SaveChangesAsync();
            if (rowsAdded <= 0)
                return null;
            return newReport;
        }

        public async Task<bool> DeleteReportByReportId(Guid ReportId)
        {
            Report? matchingReport = await _db.Reports.FirstOrDefaultAsync(r => r.Id == ReportId);

            if (matchingReport == null)
                return false;

            _db.Reports.Remove(matchingReport);
            int rowDeleted = await _db.SaveChangesAsync();

            return rowDeleted > 0;
        }

        public async Task<List<Report>> GetAllReports()
        {
            return await _db.Reports.ToListAsync();
        }

        public async Task<Report?> GetReportByReportId(Guid ReportId)
        {
            return await _db.Reports.FirstOrDefaultAsync(r => r.Id == ReportId);
        }

        public async Task<Report?> UpdateReport(Report reportToUpdate)
        {
            Report? matchingReport = await _db.Reports.FirstOrDefaultAsync(r => r.Id == reportToUpdate.Id);

            if (matchingReport == null)
                return null;

            matchingReport.PostId = reportToUpdate.PostId;
            matchingReport.MessageOfReport = reportToUpdate.MessageOfReport;
            matchingReport.StatusOfReport = reportToUpdate.StatusOfReport;
            matchingReport.TypeOfReport = reportToUpdate.TypeOfReport;
            int rowsUpdated = await _db.SaveChangesAsync();

            if (rowsUpdated <= 0)
                return null;

            return matchingReport;
        }
    }
}
