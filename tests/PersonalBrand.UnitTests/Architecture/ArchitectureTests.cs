using System.Reflection;

using NetArchTest.Rules;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Domain.Common;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.UnitTests.Architecture;

public sealed class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(Entity<>).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(IDateTimeProvider).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(ApplicationDbContext).Assembly;
    private static readonly Assembly WebApiAssembly = typeof(Program).Assembly;

    [Fact]
    public void Domain_Should_Not_Depend_On_Outer_Layers()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                ApplicationAssembly.GetName().Name!,
                InfrastructureAssembly.GetName().Name!,
                WebApiAssembly.GetName().Name!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Or_WebApi()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                InfrastructureAssembly.GetName().Name!,
                WebApiAssembly.GetName().Name!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_WebApi()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(WebApiAssembly.GetName().Name!)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    private static string FormatFailures(TestResult result)
    {
        if (result.FailingTypes is null || result.FailingTypes.Count == 0)
        {
            return "Architecture rule failed.";
        }

        return "Architecture rule failed for: "
            + string.Join(", ", result.FailingTypes.Select(type => type.FullName));
    }
}
