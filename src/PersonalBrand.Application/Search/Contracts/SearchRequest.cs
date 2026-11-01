namespace PersonalBrand.Application.Search.Contracts;

/// <summary>
/// Query parameters for public site search.
/// </summary>
public sealed class SearchRequest
{
    public string Query { get; set; } = string.Empty;

    public string? ContentType { get; set; }

    public int Limit { get; set; } = 20;
}
