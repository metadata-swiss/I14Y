namespace Bfs.Iop.Common.Serialization.Json;

public sealed record DataWrapper<T>(T Data) where T : class
{ }
