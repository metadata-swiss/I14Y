using Bfs.Iop.Core.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Net;

namespace Bfs.Iop.Core.Data.Exceptions;

/// <summary>
/// Translates a database failure into a <see cref="DatabaseException"/> whose message is safe to
/// return to the caller.
/// <para>
/// The messages produced here are intentionally generic. A <see cref="PostgresException"/> exposes
/// the failing constraint, column and sometimes the rejected value in its <c>Message</c> and
/// <c>Detail</c>; none of that is copied into the translation. The original exception is kept as
/// inner exception so the full information remains available in the logs.
/// </para>
/// </summary>
internal static class DatabaseExceptionTranslator
{
    private const string GenericMessage = "The request could not be completed because of a database error.";
    private const string UnavailableMessage = "The database is temporarily unavailable. Please try again later.";

    /// <summary>
    /// Translates <paramref name="exception"/>, classifying it by SQLSTATE when it originated in the
    /// database server. Failures that cannot be classified become a generic
    /// <see cref="HttpStatusCode.InternalServerError"/>.
    /// </summary>
    public static DatabaseException Translate(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception, nameof(exception));

        // The command interceptor translates failures on the read path. When such a failure happens
        // while saving, Entity Framework wraps the translation into a DbUpdateException, so the
        // already translated exception must be reused instead of being classified a second time.
        if (FindInnerException<DatabaseException>(exception) is { } alreadyTranslated)
        {
            return alreadyTranslated;
        }

        // Raised when a row was changed or deleted by someone else between loading and saving.
        // There is no PostgresException in this case, Entity Framework detects it from the row count.
        if (exception is DbUpdateConcurrencyException)
        {
            return Create(
                "The record was modified by another user in the meantime. Please reload and try again.",
                "DB_CONCURRENCY_CONFLICT",
                HttpStatusCode.Conflict,
                sqlState: null,
                exception);
        }

        // PostgresException derives from NpgsqlException and must therefore be tested first.
        if (FindInnerException<PostgresException>(exception) is { } postgresException)
        {
            return FromSqlState(postgresException.SqlState, exception);
        }

        // No SQLSTATE means the server was never reached or the connection dropped mid-command.
        if (FindInnerException<NpgsqlException>(exception) is not null)
        {
            return Create(UnavailableMessage, "DB_UNAVAILABLE", HttpStatusCode.ServiceUnavailable, sqlState: null, exception);
        }

        return Create(GenericMessage, "DB_ERROR", HttpStatusCode.InternalServerError, sqlState: null, exception);
    }

    /// <summary>
    /// Indicates whether <paramref name="exception"/> is a database failure worth translating.
    /// </summary>
    public static bool IsDatabaseFailure(Exception exception) =>
        exception is DbUpdateException || FindInnerException<NpgsqlException>(exception) is not null;

    private static DatabaseException FromSqlState(string sqlState, Exception inner) => sqlState switch
    {
        // Class 23 - integrity constraint violation. The caller sent data the schema rejects.
        "23505" => Create("A record with the same unique value already exists.", "DB_UNIQUE_VIOLATION", HttpStatusCode.Conflict, sqlState, inner),
        "23503" => Create("The record is referenced by other resources, or references a resource that does not exist.", "DB_FOREIGN_KEY_VIOLATION", HttpStatusCode.Conflict, sqlState, inner),
        "23001" => Create("The record is referenced by other resources.", "DB_RESTRICT_VIOLATION", HttpStatusCode.Conflict, sqlState, inner),
        "23502" => Create("A required field is missing.", "DB_NOT_NULL_VIOLATION", HttpStatusCode.BadRequest, sqlState, inner),
        "23514" => Create("A field contains a value that is not allowed.", "DB_CHECK_VIOLATION", HttpStatusCode.BadRequest, sqlState, inner),
        "23P01" => Create("The record conflicts with an existing one.", "DB_EXCLUSION_VIOLATION", HttpStatusCode.Conflict, sqlState, inner),

        // Class 22 - data exception. The value itself is malformed or out of range.
        "22001" => Create("A field exceeds the maximum allowed length.", "DB_VALUE_TOO_LONG", HttpStatusCode.BadRequest, sqlState, inner),
        "22003" => Create("A numeric field is out of the allowed range.", "DB_NUMERIC_OUT_OF_RANGE", HttpStatusCode.BadRequest, sqlState, inner),
        "22007" or "22008" => Create("A date or time field has an invalid value.", "DB_INVALID_DATETIME", HttpStatusCode.BadRequest, sqlState, inner),
        "22P02" => Create("A field has a value that could not be interpreted.", "DB_INVALID_VALUE_FORMAT", HttpStatusCode.BadRequest, sqlState, inner),

        // Class 40 - transaction rollback. Retrying the request usually succeeds.
        "40001" => Create("The request conflicted with another request. Please try again.", "DB_SERIALIZATION_FAILURE", HttpStatusCode.Conflict, sqlState, inner),
        "40P01" => Create("The request conflicted with another request. Please try again.", "DB_DEADLOCK_DETECTED", HttpStatusCode.Conflict, sqlState, inner),

        // Class 57 - operator intervention.
        "57014" => Create("The database request took too long and was cancelled.", "DB_QUERY_TIMEOUT", HttpStatusCode.GatewayTimeout, sqlState, inner),
        "57P01" or "57P02" or "57P03" => Create(UnavailableMessage, "DB_UNAVAILABLE", HttpStatusCode.ServiceUnavailable, sqlState, inner),

        // Class 53 - insufficient resources.
        "53100" => Create(UnavailableMessage, "DB_DISK_FULL", HttpStatusCode.ServiceUnavailable, sqlState, inner),
        "53200" => Create(UnavailableMessage, "DB_OUT_OF_MEMORY", HttpStatusCode.ServiceUnavailable, sqlState, inner),
        "53300" => Create(UnavailableMessage, "DB_TOO_MANY_CONNECTIONS", HttpStatusCode.ServiceUnavailable, sqlState, inner),

        // Class 42 - the schema does not match what the application expects, or the application is
        // missing a grant. Both are deployment problems, so the caller only gets the error code.
        "42501" => Create(GenericMessage, "DB_PERMISSION_DENIED", HttpStatusCode.InternalServerError, sqlState, inner),
        "42P01" or "42703" or "42P02" or "42883" => Create(GenericMessage, "DB_SCHEMA_MISMATCH", HttpStatusCode.InternalServerError, sqlState, inner),

        _ => sqlState.StartsWith("08", StringComparison.Ordinal)

            // Class 08 - connection exception.
            ? Create(UnavailableMessage, "DB_CONNECTION_FAILURE", HttpStatusCode.ServiceUnavailable, sqlState, inner)

            // Anything else keeps its SQLSTATE in the error code so it can still be looked up.
            : Create(GenericMessage, $"DB_{sqlState}", HttpStatusCode.InternalServerError, sqlState, inner)
    };

    private static DatabaseException Create(string message, string errorCode, HttpStatusCode statusCode, string? sqlState, Exception inner) =>
        new(message, errorCode, (int)statusCode, sqlState, inner);

    private static T? FindInnerException<T>(Exception exception) where T : Exception
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
