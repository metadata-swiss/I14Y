using Bfs.Iop.Admin.Models;
using Cerise;
using System;

namespace Bfs.Iop.Admin.Commands.DatasetInput;

public class CreateVersionCommand : IRequest<Guid>
{
    public CreateVersionCommand(DatasetCreateVersion model)
        => Model = model;

    public DatasetCreateVersion Model { get; }
}