using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.IopPersons;

/// <summary>
/// Initializes a <see cref="SearchIopPersonsByQueryCommand"/> instance.
/// </summary>
/// <param name="Query"> The Iop Persons query string </param>
/// <param name="Page"></param>
/// <param name="PageSize"></param>
public record SearchIopPersonsByQueryCommand(string Query, int? Page, int? PageSize) : IRequest<PagedResult<IopPersonModel>>;