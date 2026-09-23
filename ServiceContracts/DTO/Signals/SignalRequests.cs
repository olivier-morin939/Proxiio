using System.ComponentModel.DataAnnotations;
using Entities;
using Entities.Enums;

namespace ServiceContracts.DTO.Signals;

public class AddSignalRequest
{
    [Required, StringLength(120)] public string? ProblemName { get; set; }
    [StringLength(254)] public string? ProblemDescription { get; set; }
    public BugSignalLevel Level { get; set; }
    public BugSignalStatus Status { get; set; }
    public bool IsConfirmed { get; set; }
    public Signal ToSignal() => new() { Id = Guid.NewGuid(), ProblemName = ProblemName, ProblemDescription = ProblemDescription, Level = Level, Status = Status, IsConfirmed = IsConfirmed };
}

public class UpdateSignalRequest : AddSignalRequest
{
    [Required] public Guid Id { get; set; }
    public new Signal ToSignal() => new() { Id = Id, ProblemName = ProblemName, ProblemDescription = ProblemDescription, Level = Level, Status = Status, IsConfirmed = IsConfirmed };
}

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
