using Bfs.Iop.Core.Abstractions.Models;
using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.DatasetInput;

public sealed record GetDcatDatasetInputModelCommand(Guid Id) : IRequest<DcatDatasetInputModel>
{ }
