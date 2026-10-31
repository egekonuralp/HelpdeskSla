using System.Text.Json;
using HelpdeskSla.Api.ExceptionHandling;
using HelpdeskSla.Application.Exceptions;
using HelpdeskSla.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HelpdeskSla.Domain.Tests.ExceptionHandling;

public class GlobalExceptionHandlerTests
{
    private static async Task<(int StatusCode, string ResponseBody)> HandleExceptionAsync(Exception exception, 
        ILogger<GlobalExceptionHandler>? logger = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProblemDetails();

        using var serviceProvider = services.BuildServiceProvider();

        var problemDetailsService = serviceProvider.GetRequiredService<IProblemDetailsService>();

        var handler = new GlobalExceptionHandler(problemDetailsService, 
            logger ?? serviceProvider.GetRequiredService<ILogger<GlobalExceptionHandler>>());

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };

        httpContext.Request.Path = "/api/tickets/1/title";
        httpContext.Request.Headers.Accept = "application/json";

        await using var responseStream = new MemoryStream();
        httpContext.Response.Body = responseStream;

        var handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);
        Assert.True(handled);

        responseStream.Position = 0;

        using var reader = new StreamReader(responseStream);
        var responseBody = await reader.ReadToEndAsync();

        return (httpContext.Response.StatusCode, responseBody);
    }

    [Fact]
    public async Task TryHandleAsync_ConcurrencyException_Returns409ProblemDetails()
    {
        // Arrange
        var exception = new TicketConcurrencyException("Ticket was modified by another request.");

        // Act
        var result = await HandleExceptionAsync(exception);

        // Assert
        Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);

        using var json = JsonDocument.Parse(result.ResponseBody);
        Assert.Equal(409, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Conflict", json.RootElement.GetProperty("title").GetString());
        Assert.Equal("Ticket was modified by another request.", json.RootElement.GetProperty("detail").GetString());
        Assert.Equal("/api/tickets/1/title", json.RootElement.GetProperty("instance").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_UnexpectedException_Returns500WithoutExposingInternalDetails()
    {
        // Arrange
        var exception = new Exception("Sensitive database connection information");

        // Act
        var result = await HandleExceptionAsync(exception);

        // assert
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);

        using var json = JsonDocument.Parse(result.ResponseBody);
        Assert.Equal(500, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("An unexpected error occurred.", json.RootElement.GetProperty("detail").GetString());
        Assert.NotEqual("Sensitive database connection information", json.RootElement.GetProperty("detail").GetString());
        Assert.DoesNotContain("Sensitive database connection information", result.ResponseBody);
    }

    [Fact]
    public async Task TryHandleAsync_KeyNotFoundException_Returns404ProblemDetails()
    {
        // Arrange
        var exception = new KeyNotFoundException("Ticket not found.");

        // Act
        var result = await HandleExceptionAsync(exception);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);

        using var json = JsonDocument.Parse(result.ResponseBody);
        Assert.Equal(404, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Resource not found.", json.RootElement.GetProperty("title").GetString());
        Assert.Equal("Ticket not found.", json.RootElement.GetProperty("detail").GetString());
        Assert.Equal("/api/tickets/1/title", json.RootElement.GetProperty("instance").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_InvalidOperationException_Returns409ProblemDetails()
    {
        // Arrange
        var exception = new TicketInvalidStateException("Ticket cannot be resolved.");

        // Act 
        var result = await HandleExceptionAsync(exception);

        // Assert
        Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);

        using var json = JsonDocument.Parse(result.ResponseBody);
        Assert.Equal(409, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Conflict", json.RootElement.GetProperty("title").GetString());
        Assert.Equal("Ticket cannot be resolved.", json.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_UnexpectedException_LogsError()
    {
        // Arrange
        var logger = new FakeLogger<GlobalExceptionHandler>();
        var exception = new Exception("Database connection failed.");

        // Act
        var result = await HandleExceptionAsync(exception, logger);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        Assert.Contains(LogLevel.Error, logger.LoggedLevels);
    }

    [Fact]
    public async Task TryHandleAsync_ArgumentException_Returns400ProblemDetails()
    {
        // Arrange
        var exception = new ArgumentException("Invalid ticket priority");

        // Act
        var result = await HandleExceptionAsync(exception);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);

        using var json = JsonDocument.Parse(result.ResponseBody);
        Assert.Equal(400, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Bad Request", json.RootElement.GetProperty("title").GetString());
    }
}