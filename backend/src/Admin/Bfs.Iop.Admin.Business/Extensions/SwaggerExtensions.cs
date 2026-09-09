using Bfs.Iop.Infrastructure.ApiClient;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Extensions;

public static class SwaggerExtensions
{
    public static int TryGetSwaggerHeaderIntValue<T>(this SwaggerResponse<T> swaggerResponse, string headerKey, int defaultValue = default)
    {
        var headerValues = swaggerResponse.Headers.SingleOrDefault(x => x.Key.Equals(headerKey, System.StringComparison.InvariantCultureIgnoreCase)).Value;

        if (headerValues is not null && headerValues.Any())
        {
            return int.TryParse(headerValues.First(), out int value) 
                ? value 
                : defaultValue;
        }

        return defaultValue;
    }
}