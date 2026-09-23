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
    public ReportsService() : this(new UsersDbContext(new DbContextOptionsBuilder<UsersDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)) { }

    public ReportResponse AddReport(AddReportRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Helpers.HelpersValidation.ModelValidation(request);
        if (request.PostId == Guid.Empty || !_db.Posts.Any(p => p.Id == request.PostId)) throw new ArgumentException("The reported post was not found.", nameof(request.PostId));
        var report = request.ToReport();
        _db.Reports.Add(report);
        _db.SaveChanges();
        return report.ToReportResponse();
    }

    public List<ReportResponse> GetAllReports() => _db.Reports.AsNoTracking().OrderByDescending(r => r.Id).ToList().Select(r => r.ToReportResponse()).ToList();

    public ReportResponse GetReportByReportId(Guid ReportId)
    {
        var report = _db.Reports.AsNoTracking().FirstOrDefault(r => r.Id == ReportId) ?? throw new KeyNotFoundException($"Report {ReportId} was not found.");
        return report.ToReportResponse();
    }

    public ReportResponse UpdateReport(UpdateReportRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Helpers.HelpersValidation.ModelValidation(request);
        var report = _db.Reports.FirstOrDefault(r => r.Id == request.Id) ?? throw new KeyNotFoundException($"Report {request.Id} was not found.");
        if (!_db.Posts.Any(p => p.Id == request.PostId)) throw new ArgumentException("The reported post was not found.", nameof(request.PostId));
        report.PostId = request.PostId;
        report.TypeOfReport = request.TypeOfReport;
        report.MessageOfReport = request.MessageOfReport;
        report.StatusOfReport = request.StatusOfReport;
        _db.SaveChanges();
        return report.ToReportResponse();
    }

    public bool DeleteReportByReportId(Guid ReportId)
    {
        var report = _db.Reports.FirstOrDefault(r => r.Id == ReportId);
        if (report is null) return false;
        _db.Reports.Remove(report);
        _db.SaveChanges();
        return true;
    }
}
