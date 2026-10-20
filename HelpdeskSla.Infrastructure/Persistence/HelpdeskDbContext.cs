using HelpdeskSla.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskSla.Infrastructure.Persistence;

public class HelpdeskDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HelpdeskDbContext).Assembly);
    }

    public HelpdeskDbContext(DbContextOptions<HelpdeskDbContext> options) : base(options)
    {

    }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketAuditLog> TicketAuditLogs => Set<TicketAuditLog>();
}