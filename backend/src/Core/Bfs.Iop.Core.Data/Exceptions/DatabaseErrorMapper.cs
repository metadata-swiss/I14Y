using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Bfs.Iop.Core.Data.Exceptions;

/// <summary>
/// Turns a database failure into the response the caller should get.
/// </summary>
public static class DatabaseErrorMapper
{
    private const string GenericMessage = "The request could not be completed because of a database error.";
    private const string UnavailableMessage = "The database is temporarily unavailable. Please try again later.";

    /// <summary>
    /// Classifies <paramref name="exception"/> by SQLSTATE when it originated in the database server.
    /// Failures that cannot be classified become a generic 500.
    /// </summary>
    public static ProblemDetails ToProblemDetails(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception, nameof(exception));

        var (status, detail, errorCode) = Classify(exception);

        var problemDetails = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{status}",
            Status = status,
            Title = ReasonPhrases.GetReasonPhrase(status),
            Detail = detail
        };

        problemDetails.Extensions["errorCode"] = errorCode;

        return problemDetails;
    }

    /// <summary>
    /// True when <paramref name="exception"/> originated in the database, at any depth.
    /// </summary>
    public static bool CanMap(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception, nameof(exception));

        return Find<NpgsqlException>(exception) is not null
            || Find<DbUpdateException>(exception) is not null;
    }

    private static (int Status, string Detail, string ErrorCode) Classify(Exception exception)
    {
        if (Find<DbUpdateConcurrencyException>(exception) is not null)
        {
            return (409, "The record was modified by another user in the meantime. Please reload and try again.", "DB_CONCURRENCY_CONFLICT");
        }

        // No SQLSTATE means the server was never reached or the connection dropped mid-command.
        if (Find<NpgsqlException>(exception) is not null)
        {
            return (503, UnavailableMessage, "DB_UNAVAILABLE");
        }

        return (500, GenericMessage, "DB_ERROR");
    }

    private static T? Find<T>(Exception exception) where T : Exception
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is T match)
            {
                return match;
            }
        }

        return null;
    }
}
