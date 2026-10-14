using CsvHelper;
using Entities;
using Entities.Contexts;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO.Reports;
using ServiceContracts.DTO.Users;
using System.Globalization;

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

    public async Task<MemoryStream> GetReportsCSV()
    {

        List<ReportResponse> reports = (await _repository.GetAllReports()).Select(report => report.ToReportResponse()).ToList();


        MemoryStream memoryStream = new MemoryStream();


        StreamWriter streamWriter = new StreamWriter(memoryStream);


        CsvWriter csvWriter = new CsvWriter(streamWriter, culture: CultureInfo.InvariantCulture, leaveOpen: true);


        csvWriter.WriteHeader<ReportResponse>();

        csvWriter.NextRecord();


        await csvWriter.WriteRecordsAsync(reports);


        await streamWriter.FlushAsync();

        memoryStream.Position = 0;

        return memoryStream;

    }

    public async Task<MemoryStream> GetReportsEXCEL()
    {
        List<ReportResponse> allReports = (await _repository.GetAllReports()).Select(report => report.ToReportResponse()).ToList();

        MemoryStream memoryStream = new MemoryStream();

        using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
        {

            ExcelWorksheet workSheet = excelPackage.Workbook.Worksheets.Add("ReportsSheet");


            workSheet.Cells["A1"].Value = "Report Id";
            workSheet.Cells["B1"].Value = "Post Id";
            workSheet.Cells["C1"].Value = "Type of Report";
            workSheet.Cells["D1"].Value = "Message of Report";
            workSheet.Cells["E1"].Value = "Status of Report";

            int row = 2;

            foreach (ReportResponse reportResponse in allReports)
            {
                workSheet.Cells[$"A{row}"].Value = reportResponse.Id.ToString();
                workSheet.Cells[$"B{row}"].Value = reportResponse.PostId.ToString();
                workSheet.Cells[$"C{row}"].Value = reportResponse.TypeOfReport.ToString();
                workSheet.Cells[$"D{row}"].Value = reportResponse.MessageOfReport;
                workSheet.Cells[$"E{row}"].Value = reportResponse.StatusOfReport.ToString();

                row++;
            }

            workSheet.Cells[$"A1:E{row}"].AutoFitColumns();


            await excelPackage.SaveAsync();

        }

        memoryStream.Position = 0;
        return memoryStream;
    }


}
