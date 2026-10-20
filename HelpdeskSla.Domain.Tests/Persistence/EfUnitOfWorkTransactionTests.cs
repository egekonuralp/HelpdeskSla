using HelpdeskSla.Application.Interfaces;
using HelpdeskSla.Application.Services;
using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;
using HelpdeskSla.Infrastructure.Persistence;
using HelpdeskSla.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskSla.Domain.Tests.Persistence;

public class EfUnitOfWorkTransactionTests
{
    [Fact]
    public async Task UnitOfWork_BothSavesSucceed_CommitsAllChanges()
    {
        // Arrange
        var databaseName = $"HelpdeskSlaUowTest_{Guid.NewGuid():N}";
        var connectionString = $@"Server=(localdb)\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        int ticketId;

        try
        {
            await using (var setupContext = new HelpdeskDbContext(options))
            {
                await setupContext.Database.EnsureCreatedAsync();

                var ticket = new Ticket(
                    "Eski Başlık",
                    "Test Amaçlı Test",
                    TicketPriority.High);

                setupContext.Tickets.Add(ticket);
                await setupContext.SaveChangesAsync();

                ticketId = ticket.Id;
            }

            // Act         
            await using (var updateContext = new HelpdeskDbContext(options))
            {
                var repository = new EfTicketRepository(updateContext);
                var unitOfWork = new EfUnitOfWork(updateContext);

                await using var transaction = await unitOfWork.BeginTransactionAsync();

                try
                {
                    var ticket = await repository.GetByIdForUpdateAsync(ticketId);
                    Assert.NotNull(ticket);

                    ticket.UpdateTitle("Yeni Başlık");
                    await unitOfWork.SaveChangesAsync();

                    var auditLog = new TicketAuditLog(ticketId, "Eski Başlık", "Yeni Başlık");

                    await repository.AddAuditLogAsync(auditLog);
                    await unitOfWork.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollBackAsync();
                    throw;
                }
            }

            // Assert
            await using (var verificationContext = new HelpdeskDbContext(options))
            {
                var savedTicket = await verificationContext.Tickets
                    .AsNoTracking()
                    .SingleAsync(x => x.Id == ticketId);

                var savedAuditLog = await verificationContext.TicketAuditLogs
                    .AsNoTracking()
                    .SingleAsync();

                Assert.Equal("Yeni Başlık", savedTicket.Title);
                Assert.Equal(ticketId, savedAuditLog.TicketId);
                Assert.Equal("Eski Başlık", savedAuditLog.OldTitle);
                Assert.Equal("Yeni Başlık", savedAuditLog.NewTitle);
            }
        }
        finally
        {
            await using var cleanupContext = new HelpdeskDbContext(options);
            await cleanupContext.Database.EnsureDeletedAsync();
        }
    }

    [Fact]
    public async Task UnitOfWork_SecondSaveFails_RollsBackFirstSave()
    {
        // Arrange
        var databaseName = $"HelpdeskSlaUowTest_{Guid.NewGuid():N}";
        var connectionString = $@"Server=(localdb)\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        int ticketId;

        try
        {
            await using (var setupContext = new HelpdeskDbContext(options))
            {
                await setupContext.Database.EnsureCreatedAsync();

                var ticket = new Ticket(
                    "Eski Başlık",
                    "Test Amaçlı Test",
                    TicketPriority.High);

                setupContext.Tickets.Add(ticket);
                await setupContext.SaveChangesAsync();

                ticketId = ticket.Id;
            }

            // Act         
            await using (var updateContext = new HelpdeskDbContext(options))
            {
                var repository = new EfTicketRepository(updateContext);
                var unitOfWork = new EfUnitOfWork(updateContext);

                await using var transaction = await unitOfWork.BeginTransactionAsync();

                var ticket = await repository.GetByIdForUpdateAsync(ticketId);
                Assert.NotNull(ticket);

                ticket.UpdateTitle("Yeni Başlık");
                await unitOfWork.SaveChangesAsync();

                var auditLog = new TicketAuditLog(int.MaxValue, "Eski Başlık", "Yeni Başlık");

                await repository.AddAuditLogAsync(auditLog);

                await Assert.ThrowsAsync<DbUpdateException>(() =>
                    unitOfWork.SaveChangesAsync());

                await transaction.RollBackAsync();
            }

            // Assert
            await using (var verificationContext =  new HelpdeskDbContext(options))
            {
                var savedTicket = await verificationContext.Tickets
                    .AsNoTracking()
                    .SingleAsync(x => x.Id == ticketId);

                var auditLogCount = await verificationContext.TicketAuditLogs
                    .CountAsync();

                Assert.Equal("Eski Başlık", savedTicket.Title);
                Assert.Equal(0, auditLogCount);
            }
        }
        finally
        {
            await using var cleanupContext = new HelpdeskDbContext(options);
            await cleanupContext.Database.EnsureDeletedAsync();
        }
    }
}