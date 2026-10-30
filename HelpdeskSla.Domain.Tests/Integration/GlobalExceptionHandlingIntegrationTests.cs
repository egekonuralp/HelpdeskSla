using HelpdeskSla.Api.DTOs;
using HelpdeskSla.Application.Interfaces;
using HelpdeskSla.Domain.Entities;
using HelpdeskSla.Domain.Enums;
using HelpdeskSla.Domain.Tests.Fakes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using HelpdeskSla.Application.Exceptions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HelpdeskSla.Domain.Tests.Integration;

public class GlobalExceptionHandlingIntegrationTests
{
    private static void SetTicketId(Ticket ticket, int id)
    {
        var property = typeof(Ticket).GetProperty(nameof(Ticket.Id))!;

        property.SetValue(ticket, id);
    }

    private sealed class ConcurrencyThrowingUnitOfWork : IUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new TicketConcurrencyException("Ticket was modified by another request.");
        }

        public Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("Transactions are not required for this test.");
        }
    }

    private sealed class UnexpectedErrorUnitOfWork : IUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new Exception("Sensitive database connection information");
        }

        public Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("Transactions are not required for this test.");
        }
    }

    [Fact]
    public void WebApplicationFactory_CanBeCreated()
    {
        using var factory = new WebApplicationFactory<Program>();

        Assert.NotNull(factory);
    }

    [Fact]
    public async Task GetById_NonExistingTicket_Returns404()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/tickets/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task StartProgress_OpenTicket_Returns409ProblemDetails()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<FakeTicketRepository>();

        var ticket = new Ticket(
            "Network connection problem",
            "User cannot connect to the network",
            TicketPriority.High);

        SetTicketId(ticket, 15);
        repository.Tickets.Add(ticket);

        // Act
        var response = await client.PostAsync("/api/tickets/15/start-progress", content: null);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
        Assert.Equal("Conflict", problem.Title);
        Assert.Equal("/api/tickets/15/start-progress", problem.Instance);
        Assert.Equal(TicketStatus.Open, ticket.Status);
    }

    [Fact]
    public async Task UpdateTitle_NonExistingTicket_Returns404ProblemDetails()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var request = new UpdateTicketTitleRequest
        {
            NewTitle = "Updated Title",
            RowVersion = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 })
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/tickets/999/title", request);

        // Assert
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
        Assert.Equal("Resource not found.", problem.Title);
        Assert.Equal("/api/tickets/999/title", problem.Instance);
    }

    [Fact]
    public async Task UpdateTitle_ConcurrencyConflict_Returns409ProblemDetails()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();

        using var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IUnitOfWork>();
                services.AddScoped<IUnitOfWork, ConcurrencyThrowingUnitOfWork>();
            });
        });

        using var client = customizedFactory.CreateClient();
        using var scope = customizedFactory.Services.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<FakeTicketRepository>();

        var ticket = new Ticket(
            "Original title",
            "Original description",
            TicketPriority.Medium);

        SetTicketId(ticket, 15);
        repository.Tickets.Add(ticket);

        var request = new UpdateTicketTitleRequest
        {
            NewTitle = "Updated title",
            RowVersion = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 })
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/tickets/15/title", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
        Assert.Equal("Conflict", problem.Title);
        Assert.Equal("/api/tickets/15/title", problem.Instance);
    }

    [Fact]
    public async Task UpdateTitle_UnexpectedException_Returns500WithoutExposingDetails()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();

        using var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IUnitOfWork>();
                services.AddScoped<IUnitOfWork, UnexpectedErrorUnitOfWork>();
            });
        });

        using var client = customizedFactory.CreateClient();
        using var scope = customizedFactory.Services.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<FakeTicketRepository>();

        var ticket = new Ticket(
            "Original title",
            "Original description",
            TicketPriority.Medium);

        SetTicketId(ticket, 15);
        repository.Tickets.Add(ticket);

        var request = new UpdateTicketTitleRequest
        {
            NewTitle = "Updated title",
            RowVersion = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 })
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/tickets/15/title", request);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(500, problem.Status);
        Assert.Equal("Internal server error.", problem.Title);
        Assert.Equal("An unexpected error occurred.", problem.Detail);
        Assert.Equal("/api/tickets/15/title", problem.Instance);

        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("Sensitive database connection information", responseBody);
    }
}