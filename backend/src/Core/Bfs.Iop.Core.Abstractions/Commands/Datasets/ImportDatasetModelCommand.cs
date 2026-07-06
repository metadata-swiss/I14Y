using MediatR;
using Microsoft.AspNetCore.Http;

namespace Bfs.Iop.Core.Abstractions.Commands.Datasets;

public sealed record ImportDatasetModelCommand(Guid DatasetId, IFormFile ImportFile) : IRequest;