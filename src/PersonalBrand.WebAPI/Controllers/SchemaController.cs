using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Schema;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Public schema.org JSON-LD endpoint.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class SchemaController : ControllerBase
{
    private readonly ISchemaService _schemaService;

    public SchemaController(ISchemaService schemaService)
    {
        _schemaService = schemaService;
    }

    /// <summary>
    /// Returns schema.org JSON-LD for the published site and profiles.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [Produces("application/ld+json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var json = await _schemaService.GetPublicJsonLdAsync(cancellationToken);
        return Content(json, "application/ld+json", System.Text.Encoding.UTF8);
    }
}
