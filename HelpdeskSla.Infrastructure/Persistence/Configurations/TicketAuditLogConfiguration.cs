using HelpdeskSla.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpdeskSla.Infrastructure.Persistence.Configurations;

public class TicketAuditLogConfiguration : IEntityTypeConfiguration<TicketAuditLog>
{
    public void Configure(EntityTypeBuilder<TicketAuditLog> builder)
    {
        builder.ToTable("TicketAuditLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OldTitle)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.NewTitle)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ChangedAt)
            .IsRequired();

        builder.HasOne<Ticket>()
            .WithMany()
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}