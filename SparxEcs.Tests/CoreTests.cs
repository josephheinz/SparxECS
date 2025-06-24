namespace SparxEcs.Tests;

using Xunit;
using SparxEcs;

public class CoreTests
{
    private ECS ecs;

    public CoreTests()
    {
        ecs = new ECS();
    }

    public struct A
    {
        public int X;
    }

    [Fact]
    public void Test_AddEntity_AssignsUniqueID()
    {
        var id1 = ecs.AddEntity();
        var id2 = ecs.AddEntity();
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void Test_AddComponent_ShouldAddComponentToEntity()
    {
        var entity = ecs.AddEntity();
        ecs.RegisterComponent<A>();
        ecs.Add<A>(entity, new A { X = 10 });

        Assert.True(ecs.HasComponent<A>(entity));
        var comp = ecs.Get<A>(entity);
        Assert.NotNull(comp);
        Assert.Equal(10, comp.X);
    }
}
