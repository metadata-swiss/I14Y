using Bfs.Iop.DataAccess.Abstractions;
using Cerise;
using System;

namespace Bfs.Iop.Admin.Commands.DatasetInput;

public sealed record GetDcatDatasetInputModelCommand(Guid Id) : IRequest<DcatDatasetInputModel>
{ }
