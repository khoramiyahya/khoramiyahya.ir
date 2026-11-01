using PersonalBrand.Domain.Common;

namespace PersonalBrand.UnitTests.Domain;

public sealed class EntityEqualityTests
{
    [Fact]
    public void Entities_With_Same_Id_And_Type_Should_Be_Equal()
    {
        var left = new SampleEntity(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var right = new SampleEntity(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        Assert.Equal(left, right);
        Assert.True(left == right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void Entities_With_Different_Ids_Should_Not_Be_Equal()
    {
        var left = new SampleEntity(Guid.NewGuid());
        var right = new SampleEntity(Guid.NewGuid());

        Assert.NotEqual(left, right);
        Assert.True(left != right);
    }

    private sealed class SampleEntity : Entity<Guid>
    {
        public SampleEntity(Guid id)
        {
            Id = id;
        }
    }
}
