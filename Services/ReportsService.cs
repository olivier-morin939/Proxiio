using Entities;
using Entities.Contexts;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO.Reports;

namespace Services;

public class ReportsService : IReportsService
{
    private readonly IReportsRepository _repository;
    public ReportsService(IReportsRepository repository) => _repository = repository;


    public async Task<ReportResponse> AddReport(AddReportRequest? request)
    {

        ArgumentNullException.ThrowIfNull(request);


        Helpers.HelpersValidation.ModelValidation(request);

        Report? matchingReport = await _repository.GetReportByReportId(request.PostId);

        if (matchingReport == null) 
            throw new ArgumentException("The reported post was not found.", nameof(request.PostId));

        Report reportToAdd = request.ToReport();


        Report? addedReport = await _repository.AddReport(reportToAdd);
        if (addedReport == null)
            throw new InvalidOperationException("The report could not be added.");

        return addedReport.ToReportResponse();
    }

    public async Task<List<ReportResponse>> GetAllReports() => (await _repository.GetAllReports()).Select(r => r.ToReportResponse()).ToList();

    public async Task<ReportResponse> GetReportByReportId(Guid ReportId)
    {

        Report report = await _repository.GetReportByReportId(ReportId) ?? throw new KeyNotFoundException($"Report {ReportId} was not found.");
        return report.ToReportResponse();
    }

    public async Task<ReportResponse> UpdateReport(UpdateReportRequest? request)
    {

        ArgumentNullException.ThrowIfNull(request);


        Helpers.HelpersValidation.ModelValidation(request);


        Report matchingReportToUpdate = await _repository.GetReportByReportId(request.Id) ?? throw new KeyNotFoundException($"Report {request.Id} was not found.");
        Report? updatedReport = await _repository.UpdateReport(matchingReportToUpdate);

        if (updatedReport == null)
            throw new InvalidOperationException("The report could not be updated.");

        return updatedReport.ToReportResponse();
    }

    public async Task<bool> DeleteReportByReportId(Guid ReportId)
    {

        Report? report = await _repository.GetReportByReportId(ReportId);


        if (report is null) 
            return false;


        bool isDeleted = await _repository.DeleteReportByReportId(ReportId);
        if (!isDeleted)

            throw new InvalidOperationException("The report could not be deleted.");

        return true;
    }
}
