namespace Bfs.Iop.Infrastructure.ApiClient;

/// <summary>
/// This exception is thrown for any unexpected result from a web api call.
/// </summary>
[Serializable]
public partial class ApiException : Exception
{
    public ApiException(string message, int statusCode, string response, IReadOnlyDictionary<string, IEnumerable<string>> headers, Exception innerException)
        : base(message + "\n\nStatus: " + statusCode + "\nResponse: \n" + (response == null ? "(null)" : response[..Math.Min(response.Length, 512)]), innerException)
    {
        StatusCode = statusCode;
        Response = response!;
        Headers = headers;
    }

    /// <summary>
    /// Response headers
    /// </summary>
    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; private set; }

    /// <summary>
    /// Response body text
    /// </summary>
    public string Response { get; private set; }

    /// <summary>
    /// Response status code
    /// </summary>
    public int StatusCode { get; private set; }

    public override string ToString() => string.Format("HTTP Response: \n\n{0}\n\n{1}", Response, base.ToString());
}

/// <summary>
/// <inheritdoc cref="ApiException"/>
/// This exception contains a deserialized result object if the failed api call response provided a body.
/// </summary>
/// <typeparam name="TResult"></typeparam>
[Serializable]
public partial class ApiException<TResult> : ApiException
{
    public ApiException(string message, int statusCode, string response, IReadOnlyDictionary<string, IEnumerable<string>> headers, TResult result, Exception innerException)
        : base(message, statusCode, response, headers, innerException)
        => Result = result;

    /// <summary>
    /// Deserialized response body
    /// </summary>
    public TResult Result { get; private set; }
}
