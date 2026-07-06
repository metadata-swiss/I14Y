using MediatR;

namespace Bfs.Iop.Admin.Commands;

/// <summary>
/// The PUT input command
/// </summary>
/// <typeparam name="TModel"></typeparam>
public class PutInputCommand<TModel> : IRequest
{
    /// <summary>
    /// Initializes a <see cref="PutInputCommand{TModel}"/> instance.
    /// </summary>
    /// <param name="model"></param>
    public PutInputCommand(TModel model)
    {
        Model = model;
    }

    /// <summary>
    /// The model
    /// </summary>
    public TModel Model { get; }
}