using Entities;
using Entities.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepositoryContracts;

namespace Repository
{
    public class ReportsRepository :IReportsRepository 
    {

        private readonly ApplicationDbContext _db;
        private readonly ILogger<ReportsRepository>? _logger;

        public ReportsRepository(ApplicationDbContext db, ILogger<ReportsRepository>? logger = null)
        {
            this._db = db;
            _logger = logger;
        }

        private void LogOperation(string method, string operation, string arguments)
        {
            _logger?.LogInformation("[+][{Method}] {Operation} reached.", method, operation);
            _logger?.LogInformation("[+][{Method}] {Operation} received arguments: {Arguments}.", method, operation, arguments);
        }

        public async Task<Report?> AddReport(Report newReport)
        {
            LogOperation("POST", nameof(AddReport), $"ReportId:{newReport.Id}, PostId:{newReport.PostId}");
            await _db.AddAsync(newReport);
            int rowsAdded = await _db.SaveChangesAsync();
            if (rowsAdded <= 0)
                return null;
            return newReport;
        }

        public async Task<bool> DeleteReportByReportId(Guid ReportId)
        {
            LogOperation("POST", nameof(DeleteReportByReportId), $"ReportId:{ReportId}");
            Report? matchingReport = await _db.Reports.FirstOrDefaultAsync(r => r.Id == ReportId);

            if (matchingReport == null)
                return false;

            _db.Reports.Remove(matchingReport);
            int rowDeleted = await _db.SaveChangesAsync();

            return rowDeleted > 0;
        }

        public async Task<List<Report>> GetAllReports()
        {
            LogOperation("GET", nameof(GetAllReports), "(aucun)");
            return await _db.Reports.ToListAsync();
        }

        public async Task<Report?> GetReportByReportId(Guid ReportId)
        {
            LogOperation("GET", nameof(GetReportByReportId), $"ReportId:{ReportId}");
            return await _db.Reports.FirstOrDefaultAsync(r => r.Id == ReportId);
        }

        public async Task<Report?> UpdateReport(Report reportToUpdate)
        {
            LogOperation("POST", nameof(UpdateReport), $"ReportId:{reportToUpdate.Id}");
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
