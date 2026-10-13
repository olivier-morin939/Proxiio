using Entities;
namespace RepositoryContracts
{
    public interface IReportsRepository
    {
        /// <summary>
        /// Adds a new report to the database.
        /// </summary>
        /// <param name="newReport">The Report object to add.</param>
        /// <returns>The newly created report with its generated identifier.</returns>
        public Task<Report?> AddReport(Report newReport);

        /// <summary>
        /// Retrieves a list of all recorded reports.
        /// </summary>
        /// <returns>A list containing all reports.</returns>
        public Task<List<Report>> GetAllReports();

        /// <summary>
        /// Retrieves a specific report based on its unique identifier.
        /// </summary>
        /// <param name="ReportId">The unique identifier (Guid) of the report to find.</param>
        /// <returns>The matching Report object, or null if not found.</returns>
        public Task<Report?> GetReportByReportId(Guid ReportId);

        /// <summary>
        /// Updates the information of an existing report.
        /// </summary>
        /// <param name="reportToUpdate">The Report object containing the updated data.</param>
        /// <returns>The updated report.</returns>
        public Task<Report?> UpdateReport(Report reportToUpdate);

        /// <summary>
        /// Deletes a report from the database based on its identifier.
        /// </summary>
        /// <param name="ReportId">The unique identifier (Guid) of the report to delete.</param>
        /// <returns>True if the deletion was successful, otherwise false.</returns>
        public Task<bool> DeleteReportByReportId(Guid ReportId);
    }
}
