using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands;

/// <summary>
/// The GET input command
/// </summary>
/// <typeparam name="TModel"></typeparam>
public class GetInputCommand<TModel> : IRequest<TModel>
{
    /// <summary>
    /// Initializes a <see cref="GetInputCommand{TModel}"/> instance.
    /// </summary>
    /// <param name="id"></param>
    public GetInputCommand(Guid id)
    {
        Id = id;
    }

    /// <summary>
    /// The id
    /// </summary>
    public Guid Id { get; }
}