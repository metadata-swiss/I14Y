using Bfs.Iop.Infrastructure.ApiClient;

namespace Bfs.Iop.Partner.Business.Extensions;

public static class SwaggerResponseExtensions
{
    public static string TryGetSwaggerHeaderValue<T>(this SwaggerResponse<T> swaggerResponse, string headerKey, string defaultValue = "")
    {
        var headerValues = swaggerResponse.Headers.SingleOrDefault(x => x.Key.Equals(headerKey, StringComparison.InvariantCultureIgnoreCase)).Value;

        return headerValues is not null && headerValues.Any()
            ? headerValues.First()
            : defaultValue;
    }
}
