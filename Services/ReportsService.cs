using Entities;
using Entities.Contexts;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO.Reports;

namespace Services;

public class ReportsService : IReportsService
{
    private readonly UsersDbContext _db;
    public ReportsService(UsersDbContext db) => _db = db;

    // Keeps the service easy to instantiate in isolated unit tests; application DI supplies SQL Server.
    public ReportsService() : this(new UsersDbContext(new DbContextOptionsBuilder<UsersDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)) { }

    public ReportResponse AddReport(AddReportRequest? request)
    {
        // Check if the DTO object is null
        ArgumentNullException.ThrowIfNull(request);

        // Model validations for the DTO object
        Helpers.HelpersValidation.ModelValidation(request);

        // Check if the post id is valid and found
        if (request.PostId == Guid.Empty || !_db.Posts.Any(p => p.Id == request.PostId)) 
            throw new ArgumentException("The reported post was not found.", nameof(request.PostId));

        // Convert the request into entity post class
        Report report = request.ToReport();

        // Add it to the db
        _db.Reports.Add(report);
        _db.SaveChanges();

        return report.ToReportResponse();
    }

    public List<ReportResponse> GetAllReports() => _db.Reports.AsNoTracking().OrderByDescending(r => r.Id).ToList().Select(r => r.ToReportResponse()).ToList();

    public ReportResponse GetReportByReportId(Guid ReportId)
    {
        // Search the corresponding report and convert it into the DTO response
        Report report = _db.Reports.AsNoTracking().FirstOrDefault(r => r.Id == ReportId) ?? throw new KeyNotFoundException($"Report {ReportId} was not found.");
        return report.ToReportResponse();
    }

    public ReportResponse UpdateReport(UpdateReportRequest? request)
    {
        // Checking if the DTO object is null
        ArgumentNullException.ThrowIfNull(request);

        // Validations for the DTO object
        Helpers.HelpersValidation.ModelValidation(request);

        // Check if the report exist first
        Report report = _db.Reports.FirstOrDefault(r => r.Id == request.Id) ?? throw new KeyNotFoundException($"Report {request.Id} was not found.");
        
        // Check if the post exist second
        if (!_db.Posts.Any(p => p.Id == request.PostId)) 
            throw new ArgumentException("The reported post was not found.", nameof(request.PostId));

        // Fields updation
        report.PostId = request.PostId;
        report.TypeOfReport = request.TypeOfReport;
        report.MessageOfReport = request.MessageOfReport;
        report.StatusOfReport = request.StatusOfReport;
        _db.SaveChanges();

        // Return the DTO response
        return report.ToReportResponse();
    }

    public bool DeleteReportByReportId(Guid ReportId)
    {
        // Search the corresponding report
        Report? report = _db.Reports.FirstOrDefault(r => r.Id == ReportId);

        // Check if it is found
        if (report is null) 
            return false;

        // Remove the report from db
        _db.Reports.Remove(report);
        _db.SaveChanges();
        return true;
    }
}
