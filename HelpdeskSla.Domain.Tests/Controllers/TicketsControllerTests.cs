using HelpdeskSla.Api.Controllers;
using HelpdeskSla.Api.DTOs;
using HelpdeskSla.Application.Common;
using HelpdeskSla.Application.Services;
using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;
using HelpdeskSla.Domain.Tests.Fakes;
using HelpdeskSla.Infrastructure.Persistence;
using HelpdeskSla.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
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
        var controller = new TicketsController(service);

        var parameters = new TicketQueryParameters
        {
            PageNumber = 1,
            PageSize = 10,
            Status = TicketStatus.Open
        };

        // Act
        var result = await controller.GetAll(parameters, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PagedResult<TicketResponse>>(okResult.Value);

        Assert.Empty(response.Items);
        Assert.Equal(1, response.PageNumber);
        Assert.Equal(10, response.PageSize);
        Assert.Equal(0, response.TotalCount);
        Assert.Equal(0, response.TotalPages);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task GetAll_InvalidPagination_ReturnsBadRequest(int pageNumber, int pageSize)
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        // Act
        var parameters = new TicketQueryParameters
        {
            PageNumber = pageNumber,
            PageSize= pageSize
        };

        var result = await controller.GetAll(parameters, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);      
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task GetAll_StatusFilter_ReturnsOnlyOpenTickets()
    {
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();

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

        repository.Tickets.Add(ticket1);
        repository.Tickets.Add(ticket2);

        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        var parameters = new TicketQueryParameters
        {
            PageNumber = 1,
            PageSize = 10,
            Status = TicketStatus.Open
        };

        var result = await controller.GetAll(parameters, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);

        var response = Assert.IsType<PagedResult<TicketResponse>>(okResult.Value);

        Assert.Single(response.Items);
        Assert.Equal(1, response.TotalCount);
        Assert.Equal(1, response.TotalPages);

        var returnedTicket = response.Items[0];
        Assert.Equal("Printer Issue", returnedTicket.Title);
        Assert.Equal("Open", returnedTicket.Status);
        Assert.Equal("Medium", returnedTicket.Priority);
    }

    [Fact]
    public async Task GetAll_InvalidStatus_ReturnsBadRequest()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        var parameters = new TicketQueryParameters
        {
            Status = (TicketStatus)999
        };

        // Act
        var result = await controller.GetAll(parameters, CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);

        Assert.Equal(400, badRequest.StatusCode);
    }
}