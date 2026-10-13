using System;
using ServiceContracts.DTO.Reports;

namespace ServiceContracts
{
    /// <summary>
    /// This is the represenation of the data layer of the ReportsService.
    /// </summary>
    public interface IReportsService
    {

        /// <summary>
        /// Adds a new report based on the provided request data.
        /// </summary>
        /// <param name="addReportRequest">The request object containing the details of the report to add.</param>
        /// <returns>A ReportResponse object representing the newly created report.</returns>
        public Task<ReportResponse> AddReport(AddReportRequest? addReportRequest);

        /// <summary>
        /// Retrieves a list of all reports as response DTOs.
        /// </summary>
        /// <returns>A list containing all report responses.</returns>
        public Task<List<ReportResponse>> GetAllReports();

        /// <summary>
        /// Retrieves a specific report response based on its unique identifier.
        /// </summary>
        /// <param name="ReportId">The unique identifier (Guid) of the report to find.</param>
        /// <returns>The matching ReportResponse object, or null if not found.</returns>
        public Task<ReportResponse> GetReportByReportId(Guid ReportId);

        /// <summary>
        /// Updates an existing report using the provided update request data.
        /// </summary>
        /// <param name="updateReportRequest">The request object containing the updated report details.</param>
        /// <returns>A ReportResponse object representing the updated report.</returns>
        public Task<ReportResponse> UpdateReport(UpdateReportRequest? updateReportRequest);

        /// <summary>
        /// Deletes a report based on its unique identifier.
        /// </summary>
        /// <param name="ReportId">The unique identifier (Guid) of the report to delete.</param>
        /// <returns>True if the deletion was successful, otherwise false.</returns>
        public Task<bool> DeleteReportByReportId(Guid ReportId);

    }
}
