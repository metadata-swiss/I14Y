namespace Bfs.Iop.Core.Common.Exceptions;

/// <summary>
/// Represents a failure that originated in the database.
/// <para>
/// The <see cref="Exception.Message"/> is deliberately generic and safe to return to the caller:
/// it never contains table, column, constraint or value information. Everything needed to diagnose
/// the failure stays in the <see cref="Exception.InnerException"/>, which is logged but never
/// serialized into a response. Use <see cref="ErrorCode"/> to correlate a report with the logs.
/// </para>
/// </summary>
[Serializable]
public sealed class DatabaseException : Exception
{
    public DatabaseException(string message, string errorCode, int statusCode, string? sqlState, Exception inner)
        : base(message, inner)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        SqlState = sqlState;
    }

    /// <summary>
    /// Stable, non-localized identifier of the failure kind, for example <c>DB_UNIQUE_VIOLATION</c>.
    /// Safe to return to the caller and meant to be quoted in bug reports.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// The HTTP status code this failure translates to.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// The PostgreSQL SQLSTATE the failure was classified from, or <c>null</c> when the failure did
    /// not come from the database server itself (a connection or concurrency problem, for instance).
    /// </summary>
    public string? SqlState { get; }
}
