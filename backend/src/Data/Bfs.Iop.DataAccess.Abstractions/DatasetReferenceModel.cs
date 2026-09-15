namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DatasetReferenceModel(Guid DatasetId, string Identifier, MultiLanguageModel? Title, MultiLanguageModel? PublisherName);
