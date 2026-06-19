using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace Bfs.Iop.Core.Common.Api.Extensions;

public static class HttpContextExtensions
{
    public const string PageHeaderKey = "x-paging-page";
    public const string PageSizeHeaderKey = "x-paging-pagesize";
    public const string TotalPagesHeaderKey = "x-paging-totalpages";
    public const string TotalRowsHeaderKey = "x-paging-totalrows";
    public const string EmptyResult = "0";

    public static void AddPagingHeaders(this HttpContext context, int page, int pageSize, int totalHits)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(page, 0, nameof(page));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, -1, nameof(pageSize));
        ArgumentOutOfRangeException.ThrowIfLessThan(totalHits, -1, nameof(totalHits));

        context.Response.Headers[PageHeaderKey] = page.ToString(CultureInfo.InvariantCulture);
        context.Response.Headers[PageSizeHeaderKey] = pageSize.ToString(CultureInfo.InvariantCulture);

        if (totalHits > 0)
        {
            context.Response.Headers[TotalPagesHeaderKey] = Math.Ceiling((double)totalHits / pageSize).ToString(CultureInfo.InvariantCulture);
            context.Response.Headers[TotalRowsHeaderKey] = totalHits.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            context.Response.Headers[TotalPagesHeaderKey] = EmptyResult;
            context.Response.Headers[TotalRowsHeaderKey] = EmptyResult;
        }
       
    }
}