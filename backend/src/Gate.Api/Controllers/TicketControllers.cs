using Gate.Api.Data;
using Gate.Api.Dtos;
using Gate.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Gate.Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/tickets")]
public class TicketControllers(GateDbContext db, ILogger<TicketControllers> logger) : ControllerBase
{

    [HttpPost]
    [ProducesResponseType<TicketResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TicketResponse>> Issue(Guid eventId, IssueTicketRequest request, CancellationToken ct)
    {
        var evemtExists = await db.Events.AnyAsync(x => x.Id == eventId, ct);

        if (!evemtExists)
        {
            return EventNotFound(eventId);
        }

        var code = Ticket.NormalizerCode(request.Code);

        if (await db.Tickets.AnyAsync(x => x.EventId == eventId && x.Code == code, ct))
        {
            return DuplicateCode(code);
        }

        var ticket = new Ticket
        {
            Id = Guid.CreateVersion7(),
            EventId = eventId,
            Code = code,
            IssuedAt = DateTimeOffset.UtcNow,
        };

        db.Tickets.Add(ticket);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return DuplicateCode(code);
        }

        logger.LogInformation("Ticket {TicketCode} issued for event {EventId} with code {Code}", ticket.Code, eventId);

        return CreatedAtAction(nameof(List), new { eventId }, TicketResponse.From(ticket));
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TicketResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TicketResponse>>> List(Guid eventId, CancellationToken ct)
    {
        var evemtExists = await db.Events.AnyAsync(x => x.Id == eventId, ct);
        if (!evemtExists)
        {
            return EventNotFound(eventId);
        }
        var tickets = await db.Tickets
            .AsNoTracking()
            .Where(x => x.EventId == eventId)
            .OrderBy(t => t.Code)
            .ToListAsync(ct);

        return tickets.Select(TicketResponse.From).ToList();
    }

    private ObjectResult EventNotFound(Guid eventId)=>
        Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Event not found",
            detail: $"There is no event with id '{eventId}'.");

    private ObjectResult DuplicateCode(string code) =>
        Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Duplicate ticket code",
            detail: $"A ticket with code '{code}' already exists.");
}

