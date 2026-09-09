using Bfs.Iop.Admin.Models;
using MediatR;
using System;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Commands.DcatCatalog;

public class GetThemesByDcatCatalogIdCommand : IRequest<IEnumerable<DcatVocabularyEntry>>
{
    public GetThemesByDcatCatalogIdCommand(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }
}