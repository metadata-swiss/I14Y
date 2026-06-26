using Bfs.Iop.Core.Common.Exceptions;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.MediatR
{
#pragma warning disable S2436 // Types and methods should not have too many generic parameters

    /// <summary>
    /// Handles the command handler exceptions.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TException"></typeparam>
    public class CommandHandlersExceptionHandler<TRequest, TResponse, TException> : IRequestExceptionHandler<TRequest, TResponse, TException> where TRequest : notnull, IRequest<TResponse> where TException : Exception
#pragma warning restore S2436 // Types and methods should not have too many generic parameters
    {
        private readonly ILogger<CommandHandlersExceptionHandler<TRequest, TResponse, TException>> _logger;

        /// <summary>
        /// Initializes a <see cref="CommandHandlersExceptionHandler{TRequest, TResponse, TException}"/> instance.
        /// </summary>
        /// <param name="logger"></param>
        public CommandHandlersExceptionHandler(ILogger<CommandHandlersExceptionHandler<TRequest, TResponse, TException>> logger) => _logger = logger;

        /// <summary>
        /// Handles the command exception.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="exception"></param>
        /// <param name="state"></param>
        /// <param name="cancellationToken"></param>
        public Task Handle(TRequest request, TException exception, RequestExceptionHandlerState<TResponse> state, CancellationToken cancellationToken)
        {
            var level = exception is NotFoundException ? LogLevel.Information : LogLevel.Warning;

            _logger.Log(level, exception, "An error occured while processing the request of type '{typeName}'.", typeof(TRequest).Name);
            return Task.CompletedTask;
        }
    }
}