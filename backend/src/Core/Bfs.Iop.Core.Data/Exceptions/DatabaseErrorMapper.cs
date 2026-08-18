using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Bfs.Iop.Core.Data.Exceptions;

/// <summary>
/// Turns a database failure into the response the caller should get.
/// <para>
/// The details are deliberately generic. A <see cref="PostgresException"/> exposes the failing
/// constraint, the column and sometimes the rejected value in its message; none of that is copied
/// here, because this is what gets serialized into the response. The original exception is logged
/// by Entity Framework, and <c>errorCode</c> together with the <c>traceId</c> that the middleware
/// adds is what correlates a bug report with that log entry.
/// </para>
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
        // While saving, Entity Framework wraps the database failure, so the chain has to be walked.
        if (Find<PostgresException>(exception) is { } postgresException)
        {
            return FromSqlState(postgresException.SqlState);
        }

        // Raised when a row was changed or deleted by someone else between loading and saving.
        // Entity Framework detects this from the row count, so there is no SQLSTATE.
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

    private static (int Status, string Detail, string ErrorCode) FromSqlState(string sqlState) => sqlState switch
    {
        // Class 23 - integrity constraint violation. The caller sent data the schema rejects.
        "23505" => (409, "A record with the same unique value already exists.", "DB_UNIQUE_VIOLATION"),
        "23503" => (409, "The record is referenced by other resources, or references a resource that does not exist.", "DB_FOREIGN_KEY_VIOLATION"),
        "23001" => (409, "The record is referenced by other resources.", "DB_RESTRICT_VIOLATION"),
        "23P01" => (409, "The record conflicts with an existing one.", "DB_EXCLUSION_VIOLATION"),
        "23502" => (400, "A required field is missing.", "DB_NOT_NULL_VIOLATION"),
        "23514" => (400, "A field contains a value that is not allowed.", "DB_CHECK_VIOLATION"),

        // Class 22 - data exception. The value itself is malformed or out of range.
        "22001" => (400, "A field exceeds the maximum allowed length.", "DB_VALUE_TOO_LONG"),
        "22003" => (400, "A numeric field is out of the allowed range.", "DB_NUMERIC_OUT_OF_RANGE"),
        "22007" or "22008" => (400, "A date or time field has an invalid value.", "DB_INVALID_DATETIME"),
        "22P02" => (400, "A field has a value that could not be interpreted.", "DB_INVALID_VALUE_FORMAT"),

        // Class 40 - transaction rollback. Retrying the request usually succeeds.
        "40001" => (409, "The request conflicted with another request. Please try again.", "DB_SERIALIZATION_FAILURE"),
        "40P01" => (409, "The request conflicted with another request. Please try again.", "DB_DEADLOCK_DETECTED"),

        // Class 57 - operator intervention.
        "57014" => (504, "The database request took too long and was cancelled.", "DB_QUERY_TIMEOUT"),
        "57P01" or "57P02" or "57P03" => (503, UnavailableMessage, "DB_UNAVAILABLE"),

        // Class 53 - insufficient resources.
        "53100" => (503, UnavailableMessage, "DB_DISK_FULL"),
        "53200" => (503, UnavailableMessage, "DB_OUT_OF_MEMORY"),
        "53300" => (503, UnavailableMessage, "DB_TOO_MANY_CONNECTIONS"),

        // Class 42 - the schema does not match what the application expects, or a grant is missing.
        // Both are deployment problems, so the caller only gets the error code.
        "42501" => (500, GenericMessage, "DB_PERMISSION_DENIED"),
        "42P01" or "42703" or "42P02" or "42883" => (500, GenericMessage, "DB_SCHEMA_MISMATCH"),

        // Class 08 - connection exception.
        _ when sqlState.StartsWith("08", StringComparison.Ordinal) => (503, UnavailableMessage, "DB_CONNECTION_FAILURE"),

        // Anything else keeps its SQLSTATE in the error code so it can still be looked up.
        _ => (500, GenericMessage, $"DB_{sqlState}")
    };

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
