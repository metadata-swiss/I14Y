using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands;

/// <summary>
/// The DELETE input command
/// </summary>
/// <typeparam name="TModel"></typeparam>
public class DeleteInputCommand<TModel> : IRequest
{
    /// <summary>
    /// Initializes a <see cref="DeleteInputCommand{TModel}"/> instance.
    /// </summary>
    /// <param name="id"></param>
    public DeleteInputCommand(Guid id)
    {
        Id = id;
    }

    /// <summary>
    /// The id
    /// </summary>
    public Guid Id { get; }
}