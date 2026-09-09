using Bfs.Iop.Infrastructure.ApiClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.UnitTests.Extensions;

internal static class SwaggerResponseExtensions
{
    internal static Task<SwaggerResponse<T>> OkResponse<T>(this T model)
    {
        return Task.FromResult(new SwaggerResponse<T>(200, new Dictionary<string, IEnumerable<string>>(), model));
    }
}