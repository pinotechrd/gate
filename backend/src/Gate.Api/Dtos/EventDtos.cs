
using Gate.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Gate.Api.Dtos;

public record CreateEventRequest(
    [Required, StringLength(100)] string Name,
    [Range(1, 1_000_000)] int Capacity);


public record EventResponse(
    Guid Id,
    string Name,
    int Capacity,
    bool IsOpen,
    int AdmittedCount,
    DateTimeOffset CreatedAt)
{
    public static EventResponse From(VenueEvent venueEvent) => new(
        venueEvent.Id,
        venueEvent.Name,
        venueEvent.Capacity,
        venueEvent.IsOpen,
        venueEvent.AdmittedCount,
        venueEvent.CreatedAt);
}

public record OccupancyResponse(
    Guid EventId,
    int AdmittedCount,
    int Capacity,
    int Remaining,
    double Percentage);
