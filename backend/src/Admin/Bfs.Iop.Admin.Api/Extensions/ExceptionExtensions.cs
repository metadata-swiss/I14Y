using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;

namespace Bfs.Iop.Admin.Api.Extensions;

public static class ExceptionExtensions
{
    public static ProblemDetails GetProblemDetails(this Exception ex, HttpStatusCode status)
        => new()
        { 
            Type = $"https://httpstatuses.com/{(int)status}",
            Status = (int)status,
            Title = status.ToString(),
            Detail = ex.Message
        };

    public static ProblemDetails GetProblemDetails(this ValidationException ex)
    {
        ArgumentNullException.ThrowIfNull(ex, nameof(ex));

        var errorCodes = ex.Errors.Select(x => x.ErrorCode).Distinct().ToArray();

        var status = errorCodes.Length > 1
            ? HttpStatusCode.BadRequest
            : errorCodes.First() switch
                {
                    "Forbidden" => HttpStatusCode.Forbidden,
                    "Unauthorized" => HttpStatusCode.Unauthorized,
                    "NotFound" => HttpStatusCode.NotFound,
                    _ => HttpStatusCode.BadRequest
                };

        return ex.GetProblemDetails(status);
    }
}