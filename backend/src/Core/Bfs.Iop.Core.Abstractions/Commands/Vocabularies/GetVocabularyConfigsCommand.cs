using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.Vocabularies;

public sealed record GetVocabularyConfigsCommand() : IRequest<IEnumerable<VocabularyConfigModel>>
{}
