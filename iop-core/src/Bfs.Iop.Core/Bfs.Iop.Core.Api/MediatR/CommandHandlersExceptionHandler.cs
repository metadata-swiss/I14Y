using Bfs.Iop.Core.Common.Exceptions;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.MediatR;

#pragma warning disable S2436 // Types and methods should not have too many generic parameters

public class CommandHandlersExceptionHandler<TRequest, TResponse, TException> : IRequestExceptionHandler<TRequest, TResponse, TException> where TRequest : notnull, IRequest<TResponse> where TException : Exception
#pragma warning restore S2436 // Types and methods should not have too many generic parameters
{
    private readonly ILogger<CommandHandlersExceptionHandler<TRequest, TResponse, TException>> _logger;

    public CommandHandlersExceptionHandler(ILogger<CommandHandlersExceptionHandler<TRequest, TResponse, TException>> logger) => _logger = logger;

    public Task Handle(TRequest request, TException exception, RequestExceptionHandlerState<TResponse> state, CancellationToken cancellationToken)
    {
        var level = exception is NotFoundException ? LogLevel.Information : LogLevel.Warning;

        _logger.Log(level, exception, "An error occurred while processing the request of type '{requestTypeName}'.", typeof(TRequest).Name);
        return Task.CompletedTask;
    }
}