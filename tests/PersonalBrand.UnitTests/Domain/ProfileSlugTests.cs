using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Profiles;

namespace PersonalBrand.UnitTests.Domain;

public sealed class ProfileSlugTests
{
    [Theory]
    [InlineData("Yahya KHorami", "yahya-khorami")]
    [InlineData("  Hello   World  ", "hello-world")]
    [InlineData("C# Developer!", "c-developer")]
    public void FromDisplayName_Should_Create_Normalized_Slug(string displayName, string expected)
    {
        var slug = ProfileSlug.FromDisplayName(displayName);
        Assert.Equal(expected, slug);
    }

    [Fact]
    public void Normalize_Should_Reject_Invalid_Slug()
    {
        Assert.Throws<DomainException>(() => ProfileSlug.Normalize("-"));
    }
}
