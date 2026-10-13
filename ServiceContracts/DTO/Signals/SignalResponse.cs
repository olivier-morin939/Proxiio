using Entities;
using Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceContracts.DTO.Signals
{
    public class SignalResponse
    {
        public Guid Id { get; set; }
        public string? ProblemName { get; set; }
        public string? ProblemDescription { get; set; }
        public BugSignalLevel Level { get; set; }
        public BugSignalStatus Status { get; set; }
        public bool IsConfirmed { get; set; }
        public static SignalResponse From(Signal signal) => new() { Id = signal.Id, ProblemName = signal.ProblemName, ProblemDescription = signal.ProblemDescription, Level = signal.Level, Status = signal.Status, IsConfirmed = signal.IsConfirmed };
    }

    public static class SignalResponseExtension
    {
        public static SignalResponse ToSignalResponse(this Signal signal)
        {
            return new SignalResponse()
            {
                Id = signal.Id,
                ProblemName = signal.ProblemName,
                ProblemDescription = signal.ProblemDescription,
                Level = signal.Level,
                Status = signal.Status,
                IsConfirmed = signal.IsConfirmed
            };
        }
    }

}
