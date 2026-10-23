using HelpdeskSla.Application.Services;
using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using HelpdeskSla.Api.Controllers;
using HelpdeskSla.Api.DTOs;
using HelpdeskSla.Domain.Tests.Fakes;
using HelpdeskSla.Infrastructure.Repositories;
using HelpdeskSla.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HelpdeskSla.Domain.Tests.Controllers;

public class TicketsControllerTests
{
    [Fact]
    public async Task GetAll_EmptyRepository_ReturnsOkWithEmptyList()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);

        // Act
        var controller = new TicketsController(service);

        var result = await controller.GetAll(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        var tickets = Assert.IsType<List<TicketResponse>>(okResult.Value);

        Assert.Empty(tickets);
    }
}