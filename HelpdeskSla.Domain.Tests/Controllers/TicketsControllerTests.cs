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

        // Act
        var result = await controller.GetAll(pageNumber: 1, pageSize: 10, cancellationToken: CancellationToken.None);

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
        var result = await controller.GetAll(pageNumber, pageSize, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}