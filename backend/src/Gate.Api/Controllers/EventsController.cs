using Gate.Api.Data;
using Gate.Api.Dtos;
using Gate.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;

namespace Gate.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsControllers(GateDbContext db, ILogger<EventsControllers> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<EventResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventResponse>> Create(CreateEventRequest request, CancellationToken ct)
    {
        var venueEvent = new VenueEvent
        {
            Id = Guid.CreateVersion7(),
            Name = request.Name.Trim(),
            Capacity = request.Capacity,
            IsOpen = false,
            AdmittedCount = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Events.Add(venueEvent);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Event {EventId} '{EventName}' created with capacity {Capacity}",
            venueEvent.Id, venueEvent.Name, venueEvent.Capacity);

        return CreatedAtAction(nameof(GetById), new { id = venueEvent.Id }, EventResponse.From(venueEvent));
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<EventResponse>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyList<EventResponse>> List(CancellationToken ct)
    {
        var events = await db.Events
            .AsNoTracking()
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);

        return events.Select(EventResponse.From).ToList();
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<EventResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> GetById(Guid id, CancellationToken ct)
    {
        var venueEvent = await db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

        if (venueEvent is null)
        {
            return EventNotFound(id);
        }

        return EventResponse.From(venueEvent);
    }

    [HttpPost("{id:guid}/open")]
    [ProducesResponseType<EventResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> Open(Guid id, CancellationToken ct)
    {
        var response = await SetOpen(id, isOpen: true, ct);

        return response is null ? EventNotFound(id) : response;
    }

    [HttpPost("{id:guid}/close")]
    [ProducesResponseType<EventResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> Close(Guid id, CancellationToken ct)
    {
        var response = await SetOpen(id, isOpen: false, ct);

        return response is null ? EventNotFound(id) : response;
    }


    [HttpGet("{id:guid}/occupancy")]
    [ProducesResponseType<EventResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> Occupancy(Guid id, bool isOpen, CancellationToken ct)
    {
        var venueEvent = await db.Events.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (venueEvent is null)
        {
            return EventNotFound(id);
        }

        if (venueEvent.IsOpen != isOpen)
        {
            venueEvent.IsOpen = isOpen;

            await db.SaveChangesAsync(ct);

            logger.LogInformation("Event {EventId} is now {State}", venueEvent.Id, isOpen ? "open" : "closed");
        }

        return EventResponse.From(venueEvent);
    }

    private async Task<EventResponse?> SetOpen(Guid id, bool isOpen, CancellationToken ct)
    {
        var venueEvent = await db.Events.FirstOrDefaultAsync(e => e.Id == id, ct);

        if (venueEvent is null)
        {
            return null;
        }

        venueEvent.IsOpen = isOpen;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Event {EventId} is now {State}", venueEvent.Id, isOpen ? "open" : "closed");

        return EventResponse.From(venueEvent);
    }
    private ObjectResult EventNotFound(Guid id) =>
        Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Event not found",
            detail: $"There is no event with id '{id}'.");
}
