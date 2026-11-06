namespace Gate.Api.Models;

public class Ticket
{
    public const int CodeMaxLength = 32;
    public const int GateNameMaxLength = 50;

    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public VenueEvent? Event { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTimeOffset IssuedAt { get; set; }
    public DateTimeOffset AdmittedAt { get; set; }

    public string? GateName { get; set; }
    public static string NormalizerCode(string code) => code.Trim().ToUpperInvariant();
}

