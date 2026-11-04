using Gate.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Gate.Api.Data;

public class GateDbContext(DbContextOptions<GateDbContext> options) : DbContext(options)
{
    public DbSet<VenueEvent> Events => Set<VenueEvent>();
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VenueEvent>(venueEvent =>
        {
            venueEvent.HasKey(e => e.Id);
            venueEvent.Property(e => e.Name).HasMaxLength(100).IsRequired();
            venueEvent.ToTable(table => table.HasCheckConstraint("ck_events_capacity_positive", "capacit >= 1"));
        });

        modelBuilder.Entity<Ticket>(ticket =>
        {
            ticket.HasKey(t => t.Id);
            ticket.Property(t => t.Code).HasMaxLength(Ticket.CodeMaxLength).IsRequired();
            ticket.Property(t => t.GateName).HasMaxLength(Ticket.GateNameMaxLength);

            ticket.HasIndex(t => t.Code).IsUnique();

            ticket.HasOne(t => t.Event)
                .WithMany(e => e.Tickets)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });


    }
}
