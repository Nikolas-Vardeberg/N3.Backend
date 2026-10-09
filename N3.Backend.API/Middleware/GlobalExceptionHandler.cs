using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using N3.Backend.Domain.Exceptions;

namespace N3.Backend.API.Middleware;

/// <summary>
/// Turns known exceptions into ProblemDetails responses. Anything else falls through as 500.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            _ => (int?)null
        };

        if (statusCode is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = statusCode.Value;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Detail = exception.Message
            }
        });
    }
}
