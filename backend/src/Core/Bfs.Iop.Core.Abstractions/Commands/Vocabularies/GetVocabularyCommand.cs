using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Vocabularies;

public sealed record GetVocabularyCommand(string VocabularyIdentifier) : IRequest<VocabularyModel>
{ }
