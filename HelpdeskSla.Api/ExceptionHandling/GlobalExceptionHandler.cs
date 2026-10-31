using HelpdeskSla.Application.Exceptions;
using HelpdeskSla.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HelpdeskSla.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            TicketConcurrencyException => StatusCodes.Status409Conflict,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            TicketInvalidStateException => StatusCodes.Status409Conflict,
            ArgumentException => StatusCodes.Status400BadRequest,

            _ => StatusCodes.Status500InternalServerError
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occured. Path: {RequestPath}", httpContext.Request.Path);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode switch
            {
                StatusCodes.Status404NotFound => "Resource not found.",
                StatusCodes.Status409Conflict => "Conflict",
                StatusCodes.Status400BadRequest => "Bad Request",

                _ => "Internal server error."
            },

            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });
    }
}