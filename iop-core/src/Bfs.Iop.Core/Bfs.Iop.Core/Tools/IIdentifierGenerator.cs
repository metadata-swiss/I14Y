using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;

namespace Bfs.Iop.Core.Tools;

internal interface IIdentifierGenerator
{
    string GenerateIdentifier<T>(MultiLanguageModel model) where T : EntityBase;
}
