using ExpenseManager.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace ExpenseManager.API.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteProblemDetailsAsync(context, ex);
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, Exception ex)
    {
        var (status, title, errors) = ex switch
        {
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized", (IDictionary<string, string[]>?)null),
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found", (IDictionary<string, string[]>?)null),
            DomainRuleException => (StatusCodes.Status400BadRequest, "Domain validation failed", (IDictionary<string, string[]>?)null),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Bad request", (IDictionary<string, string[]>?)null),
            ValidationException ve => (
                StatusCodes.Status400BadRequest,
                "Validation failed",
                ve.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray())
            ),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error", (IDictionary<string, string[]>?)null)
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = MediaTypeNames.Application.Json;

        ProblemDetails problem = errors is null
            ? new ProblemDetails { Status = status, Title = title, Detail = ex.Message }
            : new ValidationProblemDetails(errors) { Status = status, Title = title };

        await context.Response.WriteAsJsonAsync(problem);
    }
}

