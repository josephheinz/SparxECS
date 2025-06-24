namespace SparxEcs.Tests;

using SparxEcs;
using Xunit;

public class CoreTests
{
    public struct A
    {
        public int X;
    }

    [Fact]
    public void Test_AddEntity_AssignsUniqueID()
    {
        ECS ecs = new ECS();
        var id1 = ecs.AddEntity();
        var id2 = ecs.AddEntity();
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void Test_AddComponent_ShouldAddComponentToEntity()
    {
        ECS ecs = new ECS();
        var entity = ecs.AddEntity();
        ecs.RegisterComponent<A>();
        ecs.Add<A>(entity, new A { X = 10 });

        Assert.True(ecs.HasComponent<A>(entity));
        var comp = ecs.Get<A>(entity);
        Assert.Equal(10, comp.X);
    }

    [Fact]
    public void Test_AddEntity_UsesRecycledID_WhenAvailable()
    {
        ECS ecs = new ECS();
        var id1 = ecs.AddEntity();
        ecs.DeleteEntity(id1);
        var id2 = ecs.AddEntity();
        Assert.Equal(id1, id2);
    }

    [Fact]
    public void Test_DeleteEntity_RemovesComponents_And_RecyclesID()
    {
        ECS ecs = new ECS();
        var id = ecs.AddEntity();
        ecs.RegisterComponent<A>();
        ecs.Add<A>(id, new A { X = 5 });
        ecs.DeleteEntity(id);

        Assert.False(ecs.ValidateEntity(id));
        Assert.False(ecs.HasComponent<A>(id));
    }

    [Fact]
    public void Test_ValidateEntity_ReturnsCorrectState()
    {
        ECS ecs = new ECS();
        var id1 = ecs.AddEntity();
        var id2 = ecs.AddEntity();

        ecs.DeleteEntity(id2);

        Assert.False(ecs.ValidateEntity(id2));
        Assert.True(ecs.ValidateEntity(id1));
    }

    [Fact]
    public void Test_DeleteEntity_Twice_ThrowsOrNo()
    {
        ECS ecs = new ECS();
        var id = ecs.AddEntity();
        ecs.DeleteEntity(id);
        Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => ecs.DeleteEntity(id));
    }

    [Fact]
    public void Test_ValidateEntity_ReturnsFalseForUninitializedEntity()
    {
        ECS ecs = new ECS();
        Assert.False(ecs.ValidateEntity(100));
    }

}
