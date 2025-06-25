namespace SparxEcs.Tests;

using SparxEcs;
using Xunit;

public class ComponentTests
{
    public struct A
    {
        public int X;
    }

    [Fact]
    public void Test_RegisterComponent_AssignsComponentID()
    {
        ECS ecs = new ECS();
        ecs.RegisterComponent<A>();

        Assert.True(ecs.ValidateComponent<A>());
    }

    [Fact]
    public void Test_AddComponent_SetsCorrectValue()
    {
        ECS ecs = new ECS();
        A comp = new A { X = 3 };

        ecs.RegisterComponent<A>();
        var entity = ecs.AddEntity();
        ecs.Add<A>(entity, comp);

        var outComp = ecs.Get<A>(entity);

        Assert.Equal(comp, outComp);
    }

    [Fact]
    public void Test_RemoveComponent_RemovesCorrectly()
    {
        ECS ecs = new ECS();
        A comp = new A { X = 3 };

        ecs.RegisterComponent<A>();
        var entity = ecs.AddEntity();
        ecs.Add<A>(entity, comp);
        
        Assert.True(ecs.HasComponent<A>(entity));

        ecs.Remove<A>(entity);

        Assert.False(ecs.HasComponent<A>(entity));

        A? emptyA = default(A);
        Assert.Equal(emptyA, ecs.Get<A>(entity));
    }
}

