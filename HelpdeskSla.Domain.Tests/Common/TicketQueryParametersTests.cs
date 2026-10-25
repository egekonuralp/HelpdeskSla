using Azure;
using HelpdeskSla.Api.DTOs;
using HelpdeskSla.Application.Common;
using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;
using HelpdeskSla.Domain.Tests.Fakes;
using HelpdeskSla.Infrastructure.Persistence;
using HelpdeskSla.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskSla.Domain.Tests.Common;

public class TicketQueryParametersTests
{
    [Fact]
    public void Constructor_UsesDefaultPaginationAndNoStatusFilter()
    {
        // Arrange
        var parameters = new TicketQueryParameters();

        // Assert
        Assert.Equal(1, parameters.PageNumber);
        Assert.Equal(10, parameters.PageSize);
        Assert.Null(parameters.Status);
    }

    [Fact]
    public async Task GetPagedAsync_StatusFilter_ReturnsOnlyMatchingTickets()
    {
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var context = new HelpdeskDbContext(options))
        {
            var ticket1 = new Ticket(
                "Network Issue",
                "Network connection failed",
                TicketPriority.High);

            ticket1.Assign(5);
            ticket1.StartProgress();

            var ticket2 = new Ticket(
                "Printer Issue",
                "Printer is not responding",
                TicketPriority.Medium);

            var ticket3 = new Ticket(
                "Database Error",
                "Database connection failed",
                TicketPriority.Critical);

            context.Tickets.Add(ticket1);
            context.Tickets.Add(ticket2);
            context.Tickets.Add(ticket3);
            await context.SaveChangesAsync();
        }

        var parameters = new TicketQueryParameters
        {
            PageNumber = 1,
            PageSize = 10,
            Status = TicketStatus.Open
        };

        using (var verificationContext = new HelpdeskDbContext(options))
        {
            var repository = new EfTicketRepository(verificationContext);
            var result = await repository.GetPagedAsync(parameters);

            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(1, result.TotalPages);
            Assert.All(result.Items, ticket => Assert.Equal(TicketStatus.Open, ticket.Status));
        }
    }
}