using Bfs.Iop.Admin.Models;
using MediatR;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Commands.DatasetQualityInformation;

public class GetAllCommand : IRequest<IEnumerable<DatasetQualityQuestion>>
{
}