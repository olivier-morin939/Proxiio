using Entities;
using Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ServiceContracts.DTO.Signals
{
    public class AddSignalRequest
    {
        [Required, StringLength(120)]
        public string? ProblemName { get; set; }

        [StringLength(254)] 
        public string? ProblemDescription { get; set; }

        public BugSignalLevel Level { get; set; }
        public BugSignalStatus Status { get; set; }
        public bool IsConfirmed { get; set; }
        public Signal ToSignal() => new() { Id = Guid.NewGuid(), ProblemName = ProblemName, ProblemDescription = ProblemDescription, Level = Level, Status = Status, IsConfirmed = IsConfirmed };
    }
}
