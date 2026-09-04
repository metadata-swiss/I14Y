using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Entities;

namespace Bfs.Iop.DataAccess.Relational.Tools;

internal interface IIdentifierGenerator
{
    string GenerateIdentifier<T>(MultiLanguageModel model) where T : EntityBase;
}
