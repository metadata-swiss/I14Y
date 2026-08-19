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
    private const string ForeignKeyViolationMessage = "The request conflicts with existing related data in the database.";

    /// <summary>
    /// Foreign key constraint violations become a 409; unclassified failures become a generic 500.
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
        if (Find<PostgresException>(exception) is { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        {
            return (409, ForeignKeyViolationMessage, "DB_FOREIGN_KEY_CONFLICT");
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
