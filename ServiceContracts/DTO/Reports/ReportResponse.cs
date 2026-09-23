using Entities;
using Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceContracts.DTO.Reports
{
    public class ReportResponse
    {
        public Guid Id { get; set; }

        public Guid PostId { get; set; }

        public ReportType TypeOfReport { get; set; }

        public string? MessageOfReport { get; set; }

        public ReportStatus StatusOfReport { get; set; }
    }

    public static class ReportResponseExtension
    {
    
        public static ReportResponse ToReportResponse(this Report report) 
        {

            return new ReportResponse()
            {
                Id = report.Id,
                PostId = report.PostId,
                TypeOfReport = report.TypeOfReport,
                MessageOfReport = report.MessageOfReport,
                StatusOfReport = report.StatusOfReport
            };
        
        }
    
    }


}
