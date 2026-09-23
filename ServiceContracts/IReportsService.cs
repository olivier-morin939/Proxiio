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
        /// Add a report to the ReportsService
        /// </summary>
        /// <param name="addReportRequest"></param>
        /// <returns></returns>
        public ReportResponse AddReport(AddReportRequest? addReportRequest);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<ReportResponse> GetAllReports();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ReportId"></param>
        /// <returns></returns>
        public ReportResponse GetReportByReportId(Guid ReportId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateReportRequest"></param>
        /// <returns></returns>
        public ReportResponse UpdateReport(UpdateReportRequest? updateReportRequest);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ReportId"></param>
        /// <returns></returns>
        public bool DeleteReportByReportId(Guid ReportId);

    }
}
