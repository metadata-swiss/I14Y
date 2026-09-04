using Bfs.Iop.Admin.Api.Attributes;
using Bfs.Iop.DataAccess.Abstractions.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Bfs.Iop.Admin.Api.Controllers.Partner;

[ApiController]
[Route("console/partner-admin")]
[SwaggerVersion("partner")]
public class PartnerOpenApiFileController : ControllerBase
{
    private const string UploadPath = "console/partner/v1";
    private readonly IWebHostEnvironment _environment;
    private readonly string _secret;

    public PartnerOpenApiFileController(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _secret = configuration.GetValue<string>("PartnerApi:Secret");
    }

    /// <summary>
    /// Upload OpenApi specification file to the designated folder.
    /// </summary>
    /// <param name="file">The OpenApi specification file.</param>
    /// <param name="secret">The secret for allowing the operation</param>
    /// <returns>Feedback on the action.</returns>
    [EnableCors("AllowBIT")]
    [HttpPost("v1/upload")]
    [Authorize] //TODO: specific permission missing
    public IActionResult UploadFileV1(IFormFile file, string secret)
    {
        if (string.IsNullOrWhiteSpace(_secret) || !_secret.Equals(secret))
        {
            throw new ForbiddenException($"Synchronisation API Rest method requires a valid secret key");
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest("No file was given.");
        }

        if (!file.FileName.StartsWith("openapi-"))
        {
            return BadRequest("Incorrect file name or format! File name needs to be in this form: openapi-{ENVIRONMENT}.json. Where ENVIRONMENT is one of the following: QA, DEV, TST, REF, ABN or PRD.");
        }

        var path = Path.Combine(_environment.WebRootPath, UploadPath, file.FileName);

        using var stream = new FileStream(path, FileMode.Create);
        file.CopyTo(stream);

        return Ok($"File {file.FileName} successfully uploaded");
    }
}