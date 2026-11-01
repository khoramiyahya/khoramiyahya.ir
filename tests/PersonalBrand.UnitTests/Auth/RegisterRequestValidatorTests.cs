using PersonalBrand.Application.Auth.Contracts;
using PersonalBrand.Application.Auth.Validators;

namespace PersonalBrand.UnitTests.Auth;

public sealed class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public async Task Valid_Request_Should_Pass()
    {
        var request = new RegisterRequest
        {
            Email = "user@example.com",
            Password = "StrongPass1!",
            ConfirmPassword = "StrongPass1!",
            DisplayName = "Yahya"
        };

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Mismatched_Passwords_Should_Fail()
    {
        var request = new RegisterRequest
        {
            Email = "user@example.com",
            Password = "StrongPass1!",
            ConfirmPassword = "DifferentPass1!",
            DisplayName = "Yahya"
        };

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterRequest.ConfirmPassword));
    }
}
