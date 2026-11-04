using Gate.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Gate.Api.Dtos;

public record AdmissionRequest(
    [Required, StringLength(Ticket.CodeMaxLength)] string Code,
    [Required, StringLength(Ticket.GateNameMaxLength)] string GateName);

public record AdmissionResponse(
    bool Admitted,
    string? Reason,
    string Code,
    Guid? EventId,
    string? EventName,
    int? AdmittedCount,
    int? Capacity,
    DateTimeOffset? AdmittedAt,
    string? GateName);

public record AdmissionRejectionReasons
{
    public const string UknownCode = "unknown_code";
    public const string EventClosed = "event_closed";
    public const string AlreadyAdmitted = "already_admitted";
    public const string Full = "full";
}
