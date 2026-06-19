namespace Bfs.Iop.Infrastructure.ApiClient;

public record SwaggerResponse(int StatusCode, IReadOnlyDictionary<string, IEnumerable<string>> Headers)
{ }


public record SwaggerResponse<TResult>(
    int StatusCode, 
    IReadOnlyDictionary<string, IEnumerable<string>> Headers, 
    TResult Result) : SwaggerResponse(StatusCode, Headers)
{ }