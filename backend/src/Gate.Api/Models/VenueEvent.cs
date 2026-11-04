namespace Gate.Api.Models;

public class VenueEvent
{
    public Guid Id { get; set; }
    public string Name { get; set; }=string.Empty;
    public int Capacity { get; set; }
    public bool IsOpen { get; set; }
    public int AdmittedCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<Ticket> Tickets { get; set; } = [];
}
