using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Bfs.Iop.Core.Api.Controllers;

[ApiController]
[Route("_debug")]
[ApiExplorerSettings(IgnoreApi = true)]
public class DebugConfigController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public DebugConfigController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("config/all")]
    public IActionResult All()
    {
        if (!IsAuthorized(Request)) return NotFound();

        var root = (IConfigurationRoot)_configuration;
        var keys = GetAllKeys(_configuration).ToList();
        var result = new List<object>(keys.Count);

        foreach (var key in keys)
        {
            string? effective = null;
            var providers = new List<object>();

            foreach (var p in root.Providers)
            {
                if (p.TryGet(key, out var value))
                {
                    effective = value ?? effective;
                    providers.Add(new { provider = p.GetType().Name, value });
                }
            }

            result.Add(new { key, effectiveValue = effective, providers });
        }

        return new JsonResult(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [HttpGet("config/effective")]
    public IActionResult Effective()
    {
        if (!IsAuthorized(Request)) return NotFound();

        var dict = new SortedDictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in _configuration.AsEnumerable(makePathsRelative: false))
        {
            dict[kv.Key] = kv.Value;
        }

        return new JsonResult(dict, new JsonSerializerOptions { WriteIndented = true });
    }

    private static bool IsAuthorized(HttpRequest request)
    {
        var expected = Environment.GetEnvironmentVariable("DEBUG_SECRET");
        if (string.IsNullOrWhiteSpace(expected))
            return false;

        return request.Query.TryGetValue("secret", out var secret) && secret == expected;
    }

    private static IEnumerable<string> GetAllKeys(IConfiguration configuration)
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        void Walk(IEnumerable<IConfigurationSection> sections, string? parent = null)
        {
            foreach (var s in sections)
            {
                var full = string.IsNullOrEmpty(parent) ? s.Key : $"{parent}:{s.Key}";
                keys.Add(full);
                Walk(s.GetChildren(), full);
            }
        }
        Walk(configuration.GetChildren());
        return keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase);
    }
}