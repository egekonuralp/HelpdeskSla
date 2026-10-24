using HelpdeskSla.Application.Common;
using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;
using HelpdeskSla.Infrastructure.Persistence;
using HelpdeskSla.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HelpdeskSla.Domain.Tests.Common;

public class PagedResultTests
{
    [Theory]
    [InlineData(35, 10, 4)]
    [InlineData(30, 10, 3)]
    [InlineData(0, 10, 0)]
    [InlineData(1, 10, 1)]
    public void TotalPages_CalculatesCorrectly(int totalCount, int pageSize, int expectedTotalPages)
    {
        // Arrange
        var result = new PagedResult<int>(Array.Empty<int>(), 1, pageSize, totalCount);

        // Act
        var totalPages = result.TotalPages;

        // Assert
        Assert.Equal(expectedTotalPages, totalPages);
    }

    [Fact]
    public async Task GetPagedAsync_SecondPage_ReturnsCorrectTickets()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<HelpdeskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using (var context = new HelpdeskDbContext(options))
        {
            var ticket1 = new Ticket(
                "Network Issue",
                "Network connection failed",
                TicketPriority.High);

            var ticket2 = new Ticket(
                "Printer Issue",
                "Printer is not responding",
                TicketPriority.Medium);

            var ticket3 = new Ticket(
                "Database Error",
                "Database connection failed",
                TicketPriority.Critical);

            var ticket4 = new Ticket(
                "Just Problem",
                "I have a problem",
                TicketPriority.High);

            var ticket5 = new Ticket(
                "Random Problem",
                "Just a random problem",
                TicketPriority.Critical);

            context.Tickets.Add(ticket1);
            context.Tickets.Add(ticket2);
            context.Tickets.Add(ticket3);
            context.Tickets.Add(ticket4);
            context.Tickets.Add(ticket5);
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = new HelpdeskDbContext(options))
        {
            var repository = new EfTicketRepository(context);

            var result = await repository.GetPagedAsync(2, 2);

            // Assert
            Assert.Equal(2, result.PageNumber);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
            Assert.Equal(2, result.Items.Count);

            var orderedIds = await context.Tickets
                .OrderBy(t => t.Id)
                .Select(t => t.Id)
                .ToListAsync();

            Assert.Equal(orderedIds[2], result.Items[0].Id);
            Assert.Equal(orderedIds[3], result.Items[1].Id);    
        }
    }
}
