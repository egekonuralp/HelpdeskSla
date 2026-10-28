using Azure;
using HelpdeskSla.Api.Controllers;
using HelpdeskSla.Api.DTOs;
using HelpdeskSla.Application.Common;
using HelpdeskSla.Application.Services;
using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;
using HelpdeskSla.Domain.Tests.Fakes;
using HelpdeskSla.Infrastructure.Persistence;
using HelpdeskSla.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
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

    [Fact]
    public async Task WaitForCustomer_InProgressTicket_ReturnsNoContent()
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

        // Act
        var result = await controller.WaitForCustomer(ticket.Id, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Equal(TicketStatus.PendingCustomer, ticket.Status);
        Assert.Equal(5, ticket.AssignedUserId);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task WaitForCustomer_InvalidTicketId_ReturnsBadRequest()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        // Act
        var result = await controller.WaitForCustomer(0, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task WaitForCustomer_TicketNotFound_ReturnsNotFound()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        // Act
        var result = await controller.WaitForCustomer(999, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task WaitForCustomer_AssignedTicket_ReturnsConflict()
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
        var result = await controller.WaitForCustomer(ticket.Id, CancellationToken.None);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(TicketStatus.Assigned, ticket.Status);
        Assert.Equal(5, ticket.AssignedUserId);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ResumeProgress_PendingCustomerTicket_ReturnsNoContent()
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
        ticket.WaitForCustomer();

        SetTicketId(ticket, 15);
        repository.Tickets.Add(ticket);

        // Act
        var result = await controller.ResumeProgress(ticket.Id, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Equal(TicketStatus.InProgress, ticket.Status);
        Assert.Equal(5, ticket.AssignedUserId);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ResumeProgress_InvalidTicketId_ReturnsBadRequest()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        // Act
        var result = await controller.ResumeProgress(0, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ResumeProgress_TicketNotFound_ReturnsNotFound()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        // Act
        var result = await controller.ResumeProgress(999, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task ResumeProgress_InProgressTicket_ReturnsConflict()
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

        // Act
        var result = await controller.ResumeProgress(ticket.Id, CancellationToken.None);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(TicketStatus.InProgress, ticket.Status);
        Assert.Equal(5, ticket.AssignedUserId);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Resolve_InProgressTicket_ReturnsNoContent()
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

        // Act
        var result = await controller.Resolve(ticket.Id, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Equal(TicketStatus.Resolved, ticket.Status);
        Assert.NotNull(ticket.ResolvedAt);
        Assert.NotNull(ticket.ResolutionTime);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Resolve_InvalidTicketId_ReturnsBadRequest()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        // Act
        var result = await controller.Resolve(0, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Resolve_TicketNotFound_ReturnsNotFound()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        // Act
        var result = await controller.Resolve(999, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Resolve_AlreadyResolvedTicket_ReturnsConflict()
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
        ticket.Resolve();

        var originalResolvedAt = ticket.ResolvedAt;

        SetTicketId(ticket, 15);
        repository.Tickets.Add(ticket);

        // Act
        var result = await controller.Resolve(ticket.Id, CancellationToken.None);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
        Assert.Equal(originalResolvedAt, ticket.ResolvedAt);
        Assert.Equal(TicketStatus.Resolved, ticket.Status);
    }

    [Fact]
    public async Task Close_ResolvedTicket_ReturnsNoContent()
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
        ticket.Resolve();

        var originalResolvedAt = ticket.ResolvedAt;
        var originalResolutionTime = ticket.ResolutionTime;
        var originalIsSlaBreached = ticket.IsSlaBreached;

        SetTicketId(ticket, 15);
        repository.Tickets.Add(ticket);

        // Act
        var result = await controller.Close(ticket.Id, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(TicketStatus.Closed, ticket.Status);
        Assert.Equal(5, ticket.AssignedUserId);

        Assert.Equal(originalResolvedAt, ticket.ResolvedAt);
        Assert.Equal(originalResolutionTime, ticket.ResolutionTime);
        Assert.Equal(originalIsSlaBreached, ticket.IsSlaBreached);
    }

    [Fact]
    public async Task Close_InvalidTicketId_ReturnsBadRequest()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        // Act
        var result = await controller.Close(0, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Close_TicketNotFound_ReturnsNotFound()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        // Act
        var result = await controller.Close(999, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Close_AlreadyClosedTicket_ReturnsConflict()
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
        ticket.Resolve();
        ticket.Close();

        var originalResolvedAt = ticket.ResolvedAt;
        var originalResolutionTime = ticket.ResolutionTime;
        var originalIsSlaBreached = ticket.IsSlaBreached;

        SetTicketId(ticket, 15);
        repository.Tickets.Add(ticket);

        // Act
        var result = await controller.Close(ticket.Id, CancellationToken.None);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
        Assert.Equal(TicketStatus.Closed, ticket.Status);

        Assert.Equal(originalResolvedAt, ticket.ResolvedAt);
        Assert.Equal(originalResolutionTime, ticket.ResolutionTime);
        Assert.Equal(originalIsSlaBreached, ticket.IsSlaBreached);
    }

    [Fact]
    public async Task UpdateTitle_InvalidBase64_ReturnsBadRequest()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        var request = new UpdateTicketTitleRequest
        {
            NewTitle = "Updated title",
            RowVersion = "invalid***"
        };

        // Act
        var result = await controller.UpdateTitle(1, request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task UpdateTitle_InvalidRowVersionLength_ReturnsBadRequest()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        var request = new UpdateTicketTitleRequest
        {
            NewTitle = "Updated title",
            RowVersion = "AQID"
        };

        // Act
        var result = await controller.UpdateTitle(1, request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task UpdateTitle_ValidRequest_ReturnsOkWithUpdatedTicket()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        var ticket = new Ticket(
            "Original title",
            "Original description",
            TicketPriority.Medium);

        SetTicketId(ticket, 15);
        repository.Tickets.Add(ticket);

        // Act
        var rowVersion = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, });

        var request = new UpdateTicketTitleRequest
        {
            NewTitle = "Updated title",
            RowVersion = rowVersion
        };

        var result = await controller.UpdateTitle(ticket.Id, request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TicketResponse>(okResult.Value);

        Assert.Equal(15, response.Id);
        Assert.Equal("Updated title", response.Title);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(Convert.ToBase64String(ticket.RowVersion), response.RowVersion);
    }

    [Fact]
    public async Task UpdateTitle_TicketNotFound_ReturnsNotFound()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        var ticket = new Ticket(
            "Original title",
            "Original description",
            TicketPriority.Medium);

        SetTicketId(ticket, 15);
        repository.Tickets.Add(ticket);

        var rowVersion = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, });

        var request = new UpdateTicketTitleRequest
        {
            NewTitle = "Updated title",
            RowVersion = rowVersion
        };

        // Act
        var result = await controller.UpdateTitle(999, request, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
        Assert.Equal("Original title", ticket.Title);
    }

    [Fact]
    public async Task UpdateTitle_ConcurrencyConflict_ReturnsConflict()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new ThrowingUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        var ticket = new Ticket(
            "Original title",
            "Original description",
            TicketPriority.Medium);

        SetTicketId(ticket, 15);
        repository.Tickets.Add(ticket);

        var rowVersion = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, });

        var request = new UpdateTicketTitleRequest
        {
            NewTitle = "Updated title",
            RowVersion = rowVersion
        };

        //Act
        var result = await controller.UpdateTitle(ticket.Id, request, CancellationToken.None);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status409Conflict, conflictResult.StatusCode);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task UpdateTitle_EmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        var repository = new FakeTicketRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new TicketService(repository, unitOfWork);
        var controller = new TicketsController(service);

        var ticket = new Ticket(
            "Original title",
            "Original description",
            TicketPriority.Medium);

        SetTicketId(ticket, 15);
        repository.Tickets.Add(ticket);

        var rowVersion = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, });

        var request = new UpdateTicketTitleRequest
        {
            NewTitle = "   ",
            RowVersion = rowVersion
        };

        // Act
        var result = await controller.UpdateTitle(ticket.Id, request, CancellationToken.None);

        // Assert
        var conflictResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, conflictResult.StatusCode);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
        Assert.Equal("Original title", ticket.Title);
    }
}