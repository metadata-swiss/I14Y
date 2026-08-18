using Bfs.Iop.Core.Data.Exceptions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace Bfs.Iop.Core.Data.Interceptors;

/// <summary>
/// Replaces database failures raised while executing a command with a
/// <see cref="Common.Exceptions.DatabaseException"/>.
/// <para>
/// This covers the read path, which <see cref="IopDbContext.SaveChangesAsync(CancellationToken)"/>
/// cannot see: query timeouts, dropped connections and an unreachable server. Failures raised while
/// saving pass through here too, but Entity Framework wraps them into a <c>DbUpdateException</c>
/// afterwards, so <see cref="IopDbContext"/> unwraps the translation again.
/// </para>
/// </summary>
internal sealed class DatabaseExceptionInterceptor : DbCommandInterceptor
{
    public static readonly DatabaseExceptionInterceptor Instance = new();

    private DatabaseExceptionInterceptor()
    { }

    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData) =>
        ThrowTranslated(eventData, CancellationToken.None);

    public override Task CommandFailedAsync(
        DbCommand command,
        CommandErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        ThrowTranslated(eventData, cancellationToken);
        return Task.CompletedTask;
    }

    private static void ThrowTranslated(CommandErrorEventData eventData, CancellationToken cancellationToken)
    {
        // A caller that went away aborts the command as well. That is not a database failure and
        // must keep its original exception, otherwise it would be reported as a query timeout.
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (!DatabaseExceptionTranslator.IsDatabaseFailure(eventData.Exception))
        {
            return;
        }

        // Entity Framework has already logged the original exception with its full details at this
        // point, so throwing the translation here loses nothing.
        throw DatabaseExceptionTranslator.Translate(eventData.Exception);
    }
}
