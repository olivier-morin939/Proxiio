using Entities;
using Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ServiceContracts.DTO.Reports
{
    public class AddReportRequest
    {
        [Required(ErrorMessage = "Post Id is required to deliver a report.")]
        public Guid PostId { get; set; }


        public ReportType TypeOfReport { get; set; }

        [Required(ErrorMessage = "Message Of Report is required to deliver a report.")]
        [StringLength(254, MinimumLength = 1, ErrorMessage = "Message Of Report should be between 1 and 254 char long.")]
        public string? MessageOfReport { get; set; }

        public ReportStatus StatusOfReport { get; set; }


        public Report ToReport()
        {
            return new Report()
            {
                Id = Guid.NewGuid(),
                PostId = this.PostId,
                TypeOfReport = this.TypeOfReport,
                MessageOfReport = this.MessageOfReport,
                StatusOfReport = this.StatusOfReport
            };
        }


    }
}
