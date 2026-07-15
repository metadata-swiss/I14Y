using Swashbuckle.AspNetCore.Filters;

namespace Bfs.Iop.Partner.Api.Swagger.Examples;

public class DcatCatalogRecordInputModelExamplesProvider : IExamplesProvider<object>
{
    public object GetExamples()
    {
        return "{\r\n  \"data\": [\r\n    {\r\n      \"primaryTopic\": {\r\n        \"resourceId\": \"3fa85f64-5717-4562-b3fc-2c963f66afa6\",\r\n        \"resourceType\": \"Dataset\"\r\n      },\r\n      \"themes\": [\r\n        {\r\n          \"code\": \"string\",\r\n          \"themeTaxonomy\": \"string\"\r\n        }\r\n      ]\r\n    }\r\n  ]\r\n}";
    }
}
