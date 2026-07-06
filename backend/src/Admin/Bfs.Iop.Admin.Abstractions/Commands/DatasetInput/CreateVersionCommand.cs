using Bfs.Iop.Admin.Models;
using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.DatasetInput;

public class CreateVersionCommand : IRequest<Guid>
{
    public CreateVersionCommand(DatasetCreateVersion model)
        => Model = model;

    public DatasetCreateVersion Model { get; }
}