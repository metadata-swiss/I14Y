using Bfs.Iop.Admin.Models;
using MediatR;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Commands.IopPerson;

public class SearchIopPersonsByQueryCommand : IRequest<IEnumerable<ActiveDirectoryUser>>
{
    /// <summary>
    /// Initializes a <see cref="SearchIopPersonsByQueryCommand"/> instance.
    /// </summary>
    /// <param name="query">The Iop Person query string</param>
    public SearchIopPersonsByQueryCommand(string query)
    {
        Query = query;
    }

    /// <summary>
    /// The Iop Persons query string
    /// </summary>
    public string Query { get; }
}