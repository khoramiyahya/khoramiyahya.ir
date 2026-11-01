namespace PersonalBrand.Application.Robots.Contracts;

/// <summary>
/// Request payload for creating a robots.txt rule.
/// </summary>
public sealed class CreateRobotsRuleRequest
{
    public string UserAgent { get; set; } = "*";

    public string Directive { get; set; } = "Allow";

    public string Path { get; set; } = "/";

    public int DisplayOrder { get; set; }

    public bool IsPublished { get; set; }
}
