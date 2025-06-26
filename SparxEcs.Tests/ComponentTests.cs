namespace SparxEcs.Tests;

using SparxEcs;
using Xunit;

public class ComponentTests
{
    public struct A
    {
        public int X;
    }

    public struct B
    {
        public int Y;
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

        var remove = ecs.Remove<A>(entity);
        Assert.True(remove);
        Assert.False(ecs.HasComponent<A>(entity));

        A emptyA = default(A);
        Assert.Equal(emptyA.X, ecs.Get<A>(entity).X);
    }

    [Fact]
    public void Test_GetComponent_ReturnsCorrectValue()
    {
        ECS ecs = new ECS();

        var entity = ecs.AddEntity();
        var comp = new A { X = 2 };

        ecs.RegisterComponent<A>();
        ecs.Add<A>(entity, comp);

        Assert.Equal(comp, ecs.Get<A>(entity));
    }

    [Fact]
    public void Test_HasComponent_WorksAsExpected()
    {
        ECS ecs = new ECS();

        var entity = ecs.AddEntity();
        var comp = new A { X = 2 };

        ecs.RegisterComponent<A>();
        ecs.Add<A>(entity, comp);

        Assert.True(ecs.HasComponent<A>(entity));
    }

    [Fact]
    public void Test_AddMultipleComponents_ToSameEntity()
    {
        ECS ecs = new ECS();

        var entity = ecs.AddEntity();
        var compA = new A { X = 2 };
        var compB = new B { Y = 3 };

        ecs.RegisterComponent<A>();
        ecs.RegisterComponent<B>();
        ecs.Add<A>(entity, compA);
        ecs.Add<B>(entity, compB);

        Assert.True(ecs.HasComponent<A>(entity));
        Assert.True(ecs.HasComponent<B>(entity));
    }
}

