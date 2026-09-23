using System;
using System.ComponentModel.DataAnnotations;
using Entities.Enums;


namespace Entities
{
    /// <summary>
    ///  Represente la table Report
    /// </summary>
    public class Report
    {
        [Key]
        public Guid Id { get; set; }

        public Guid PostId { get; set; }

        public ReportType TypeOfReport { get; set; }

        [StringLength(254)]
        public string? MessageOfReport { get; set; }

        public ReportStatus StatusOfReport { get; set;}

    }
}
