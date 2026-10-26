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
    private static void SetTicketId(Ticket ticket, int id)
    {
        var property = typeof(Ticket).GetProperty(nameof(Ticket.Id))!;

        property.SetValue(ticket, id);
    }

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

    [Fact]
    public async Task GetAll_StatusAndPriorityFilter_ReturnsMatchingTickets()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();

        var ticket1 = new Ticket(
            "Network Issue",
            "Network connection failed",
            TicketPriority.Critical);

        var ticket2 = new Ticket(
            "Printer Issue",
            "Printer is not responding",
            TicketPriority.Medium);

        var ticket3 = new Ticket(
            "Database Error",
            "Database errorss",
            TicketPriority.Critical);

        ticket3.Assign(5);
        ticket3.StartProgress();

        var ticket4 = new Ticket(
            "Login Problem",
            "Some login problem",
            TicketPriority.Critical);

        repository.Tickets.Add(ticket1);
        repository.Tickets.Add(ticket2);
        repository.Tickets.Add(ticket3);
        repository.Tickets.Add(ticket4);

        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        var parameters = new TicketQueryParameters
        {
            PageNumber = 1,
            PageSize = 10,
            Status = TicketStatus.Open,
            Priority = TicketPriority.Critical
        };

        var result = await controller.GetAll(parameters, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        var response = Assert.IsType<PagedResult<TicketResponse>>(okResult.Value);

        Assert.Equal(2, response.TotalCount);
        Assert.Equal(2, response.Items.Count);
        Assert.All(response.Items, ticket =>
        {
            Assert.Equal("Open", ticket.Status);
            Assert.Equal("Critical", ticket.Priority);
        });
    }

    [Fact]
    public async Task GetAll_InvalidPriority_ReturnsBadRequest()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        var parameters = new TicketQueryParameters
        {
            Priority = (TicketPriority)999
        };

        // Act
        var result = await controller.GetAll(parameters, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetAll_InvalidSortBy_ReturnsBadRequest()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        var parameters = new TicketQueryParameters
        {
            SortBy = "banana"
        };

        // Act & Assert
        var result = await controller.GetAll(parameters, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetAll_InvalidSortDirection_ReturnsBadRequest()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        var parameters = new TicketQueryParameters
        {
            SortDirection = "fast"
        };

        // Act & Assert
        var result = await controller.GetAll(parameters, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Assign_ValidRequest_ReturnsNoContent()
    {
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        var ticket = new Ticket(
            "Network Issue",
            "Network connection failed",
            TicketPriority.Critical);

        SetTicketId(ticket, 15);
        repository.Tickets.Add(ticket);

        var request = new AssignTicketRequest
        {
            UserId = 5
        };

        // Act
        var result = await controller.Assign(ticket.Id, request, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Equal(TicketStatus.Assigned, ticket.Status);
        Assert.Equal(5, ticket.AssignedUserId);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Assign_InvalidUserId_ReturnsBadRequest()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        var request = new AssignTicketRequest
        {
            UserId = 0
        };

        // Act
        var result = await controller.Assign(15, request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Assign_TicketNotFound_ReturnsNotFound()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        var request = new AssignTicketRequest
        {
            UserId = 5
        };

        // Act
        var result = await controller.Assign(999, request, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Assign_InProgressTicket_ReturnsConflict()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        var ticket = new Ticket(
            "Network connection problem",
            "User cannot connect to the network",
            TicketPriority.High);

        ticket.Assign(5);
        ticket.StartProgress();

        SetTicketId(ticket, 15);
        repository.Tickets.Add(ticket);

        var request = new AssignTicketRequest
        {
            UserId = 10
        };

        // Act
        var result = await controller.Assign(ticket.Id, request, CancellationToken.None);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(TicketStatus.InProgress, ticket.Status);
        Assert.Equal(5, ticket.AssignedUserId);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Assign_InvalidTicketId_ReturnsBadRequest()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        var request = new AssignTicketRequest
        {
            UserId = 5
        };

        // Act
        var result = await controller.Assign(0, request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Assign_NullRequest_ReturnsBadRequest()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        // Act
        var result = await controller.Assign(15, null!, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task StartProgress_AssignedTicket_ReturnsNoContent()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        var ticket = new Ticket(
            "Network connection problem",
            "User cannot connect to the network",
            TicketPriority.High);

        ticket.Assign(5);

        SetTicketId(ticket, 15);
        repository.Tickets.Add(ticket);

        // Act
        var result = await controller.StartProgress(ticket.Id, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Equal(TicketStatus.InProgress, ticket.Status);
        Assert.Equal(5, ticket.AssignedUserId);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task StartProgress_InvalidTicketId_ReturnsBadRequest()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        // Act
        var result = await controller.StartProgress(0, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount); 
    }

    [Fact]
    public async Task StartProgress_TicketNotFound_ReturnsNotFound()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        // Act
        var result = await controller.StartProgress(999, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task StartProgress_OpenTicket_ReturnsConflict()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        var ticket = new Ticket(
            "Network connection problem",
            "User cannot connect to the network",
            TicketPriority.High);

        SetTicketId(ticket, 15);
        repository.Tickets.Add(ticket);

        // Act
        var result = await controller.StartProgress(ticket.Id, CancellationToken.None);

        // Assert
        Assert.Null(ticket.AssignedUserId);
        Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
        Assert.Equal(TicketStatus.Open, ticket.Status);
    }
}