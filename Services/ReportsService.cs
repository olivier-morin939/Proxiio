using Entities;
using Entities.Contexts;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO.Reports;

namespace Services;

public class ReportsService : IReportsService
{
    private readonly ApplicationDbContext _db;
    public ReportsService(ApplicationDbContext db) => _db = db;

    // Keeps the service easy to instantiate in isolated unit tests; application DI supplies SQL Server.
    public ReportsService() : this(new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)) { }

    public async Task<ReportResponse> AddReport(AddReportRequest? request)
    {
        // Check if the DTO object is null
        ArgumentNullException.ThrowIfNull(request);

        // Model validations for the DTO object
        Helpers.HelpersValidation.ModelValidation(request);

        // Check if the post id is valid and found
        if (request.PostId == Guid.Empty || !await _db.Posts.AnyAsync(p => p.Id == request.PostId)) 
            throw new ArgumentException("The reported post was not found.", nameof(request.PostId));

        // Convert the request into entity post class
        Report report = request.ToReport();

        // Add it to the db
        _db.Reports.Add(report);
        await _db.SaveChangesAsync();

        return report.ToReportResponse();
    }

    public async Task<List<ReportResponse>> GetAllReports() => (await _db.Reports.AsNoTracking().OrderByDescending(r => r.Id).ToListAsync()).Select(r => r.ToReportResponse()).ToList();

    public async Task<ReportResponse> GetReportByReportId(Guid ReportId)
    {
        // Search the corresponding report and convert it into the DTO response
        Report report = await _db.Reports.AsNoTracking().FirstOrDefaultAsync(r => r.Id == ReportId) ?? throw new KeyNotFoundException($"Report {ReportId} was not found.");
        return report.ToReportResponse();
    }

    public async Task<ReportResponse> UpdateReport(UpdateReportRequest? request)
    {
        // Checking if the DTO object is null
        ArgumentNullException.ThrowIfNull(request);

        // Validations for the DTO object
        Helpers.HelpersValidation.ModelValidation(request);

        // Check if the report exist first
        Report report = await _db.Reports.FirstOrDefaultAsync(r => r.Id == request.Id) ?? throw new KeyNotFoundException($"Report {request.Id} was not found.");
        
        // Check if the post exist second
        if (!await _db.Posts.AnyAsync(p => p.Id == request.PostId)) 
            throw new ArgumentException("The reported post was not found.", nameof(request.PostId));

        // Fields updation
        report.PostId = request.PostId;
        report.TypeOfReport = request.TypeOfReport;
        report.MessageOfReport = request.MessageOfReport;
        report.StatusOfReport = request.StatusOfReport;
        await _db.SaveChangesAsync();

        // Return the DTO response
        return report.ToReportResponse();
    }

    public async Task<bool> DeleteReportByReportId(Guid ReportId)
    {
        // Search the corresponding report
        Report? report = await _db.Reports.FirstOrDefaultAsync(r => r.Id == ReportId);

        // Check if it is found
        if (report is null) 
            return false;

        // Remove the report from db
        _db.Reports.Remove(report);
        await _db.SaveChangesAsync();
        return true;
    }
}
