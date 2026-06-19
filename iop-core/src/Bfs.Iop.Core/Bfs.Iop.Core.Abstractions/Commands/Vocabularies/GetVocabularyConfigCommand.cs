using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Vocabularies;

public sealed record GetVocabularyConfigCommand(Guid Id) : IRequest<VocabularyConfigModel>
{}
