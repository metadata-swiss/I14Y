using MediatR;

namespace Bfs.Iop.Admin.Commands;

/// <summary>
/// The POST input command
/// </summary>
/// <typeparam name="TModel"></typeparam>
public class PostInputCommand<TModel> : IRequest<TModel>
{
    /// <summary>
    /// Initializes a <see cref="PostInputCommand{TModel}"/> instance.
    /// </summary>
    /// <param name="model"></param>
    public PostInputCommand(TModel model)
    {
        Model = model;
    }

    /// <summary>
    /// The model
    /// </summary>
    public TModel Model { get; }
}