using System;
using System.ComponentModel.DataAnnotations;
using Entities.Enums;

namespace Entities
{
    public class Signal
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(120)]
        public string? ProblemName { get;  set;  }

        [StringLength(254)]
        public string? ProblemDescription { get; set; }

        public BugSignalLevel Level { get; set; }
        public BugSignalStatus Status { get; set; }

        public bool IsConfirmed { get; set; }




    }
}
