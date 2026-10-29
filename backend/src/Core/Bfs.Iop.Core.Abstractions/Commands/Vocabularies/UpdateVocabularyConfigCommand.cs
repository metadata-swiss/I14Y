using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.Vocabularies;

public sealed record UpdateVocabularyConfigCommand(Guid Id, VocabularyConfigInputModel Model) : IRequest
{}
