namespace PersonalBrand.Application.Schema;

/// <summary>
/// Public schema.org JSON-LD use cases.
/// </summary>
public interface ISchemaService
{
    Task<string> GetPublicJsonLdAsync(CancellationToken cancellationToken = default);
}
