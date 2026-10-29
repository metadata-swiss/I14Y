using Bfs.Iop.Core.Abstractions.Models;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;

public sealed record ExportDcatCatalogCommand(Guid Id, RdfExportFormat Format) : IRequest<string>
{ }
