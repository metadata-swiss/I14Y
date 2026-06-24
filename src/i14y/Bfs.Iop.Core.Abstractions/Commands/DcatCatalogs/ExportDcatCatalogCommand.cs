using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;

public sealed record ExportDcatCatalogCommand(Guid Id, CatalogExportFormat Format) : IRequest<string>
{ }
