using Gate.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Gate.Api.Dtos;

public record IssueTicketRequest(
    [Required, StringLength(Ticket.CodeMaxLength)]
     [RegularExpression(@"^\s*[A-Za-z0-9-]+\s*$", ErrorMessage = "The code may only contain letters, digits and hyphens.")]
    string Code);

public record TicketResponse(
    Guid Id,
    Guid EventId,
    string Code,
    DateTimeOffset IssuedAt,
    DateTimeOffset? AdmittedAt,
    string? GateName)
{
    public static TicketResponse From(Ticket ticket) => new(
        ticket.Id,
        ticket.EventId,
        ticket.Code,
        ticket.IssuedAt,
        ticket.AdmittedAt,
        ticket.GateName);
}
