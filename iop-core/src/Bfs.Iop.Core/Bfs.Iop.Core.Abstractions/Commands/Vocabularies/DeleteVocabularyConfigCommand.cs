using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Vocabularies;

public sealed record DeleteVocabularyConfigCommand(Guid Id) : IRequest
{}
