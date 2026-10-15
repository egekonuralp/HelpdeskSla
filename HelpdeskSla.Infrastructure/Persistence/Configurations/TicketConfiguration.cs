using HelpdeskSla.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpdeskSla.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");

        builder.HasKey(ticket => ticket.Id);

        builder.Property(ticket => ticket.Id)
            .ValueGeneratedOnAdd();

        builder.Property(ticket => ticket.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ticket => ticket.Description)
            .IsRequired();

        builder.Property(ticket => ticket.Priority)
            .HasConversion<int>();

        builder.Property(ticket => ticket.Status)
            .HasConversion<int>();

        builder.Property(ticket => ticket.CreatedAt)
            .IsRequired();

        builder.Property(ticket => ticket.ResolvedAt)
            .IsRequired(false);

        builder.Ignore(ticket => ticket.ResolutionTime);
        builder.Ignore(ticket => ticket.SlaDeadline);
        builder.Ignore(ticket => ticket.RemainingSlaTime);
        builder.Ignore(ticket => ticket.IsSlaBreached);
        builder.Ignore(ticket => ticket.SlaStatus);
    }
}
