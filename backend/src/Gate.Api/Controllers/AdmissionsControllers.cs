using Gate.Api.Data;
using Gate.Api.Dtos;
using Gate.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gate.Api.Controllers;

[ApiController]
[Route("api/admissions")]
public class AdmissionsControllers(GateDbContext db, ILogger<AdmissionsControllers> logger) : ControllerBase
{

    [HttpPost]
    [ProducesResponseType<AdmissionResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdmissionResponse>> Admit(AdmissionRequest request,CancellationToken ct)
    {
        var code=Ticket.NormalizerCode(request.Code);
        var gateName = request.GateName.Trim();

        var ticket = await db.Tickets
            .Include(t=>t.Event)
            .FirstOrDefaultAsync(t => t.Code == code,ct);

        if (ticket is null)
        {
            logger.LogInformation("Gate {GateName} rejected {TicketCode}: {Reason}",
                gateName, code, AdmissionRejectionReasons.UknownCode);

            return new AdmissionResponse(false, AdmissionRejectionReasons.UknownCode,
                code, null, null, null, null, null, null);
        }

        var venueEvent = ticket.Event;

        var rejection =
            !venueEvent.IsOpen ? AdmissionRejectionReasons.EventClosed
            : ticket.AdmittedAt is not null ? AdmissionRejectionReasons.AlreadyAdmitted
            : venueEvent.AdmittedCount >= venueEvent.Capacity ? AdmissionRejectionReasons.Full
            : null;

        if (rejection is not null)
        {
            logger.LogInformation("Gate {GateName} rejected {TicketCode}: {Reason}",
                gateName, code, rejection);

            return new AdmissionResponse(false, rejection,ticket.Code,venueEvent.Id, venueEvent.Name,
                venueEvent.AdmittedCount, venueEvent.Capacity,ticket.AdmittedAt, ticket.GateName);
        }

        ticket.AdmittedAt = DateTimeOffset.UtcNow;
        ticket.GateName = gateName;
        venueEvent.AdmittedCount++;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Gate {GateName} admitted {TicketCode} to event {EventId} ({AdmittedCount}/{Capacity})",
            gateName, code, venueEvent.Id, venueEvent.AdmittedCount, venueEvent.Capacity);

        return new AdmissionResponse(true, null,
            code, venueEvent.Id, venueEvent.Name, venueEvent.AdmittedCount, venueEvent.Capacity,ticket.AdmittedAt, ticket.GateName);
    }
}
