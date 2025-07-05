namespace SparxECS.Tests;

using SparxECS;
using Xunit;

public class MaskAndQueryTests
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
    public void Test_SetComponentMask_SetsCorrectBits()
    {
        var mask = new ComponentMask(64);
        mask.Set(2, 1);
        mask.Set(9, 1);
        mask.Set(23, 1);
        mask.Set(52, 1);

        Assert.True(mask.Has(2));
        Assert.True(mask.Has(9));
        Assert.True(mask.Has(23));
        Assert.True(mask.Has(52));
    }

    [Fact]
    public void Test_Query_OneComponent_ReturnsCorrect_Entities()
    {
        ECS ecs = new ECS();
        ecs.RegisterComponent<A>();
        A[] ids = new A[5] { new A { X = 0 }, new A { X = 2 }, new A { X = 4 }, new A { X = 6 }, new A { X = 8 } };
        A[] outputIds = new A[5];
        for (int i = 0; i < 10; i++)
        {
            var entity = ecs.AddEntity();
            if (i % 2 == 0)
                ecs.Add<A>(entity, new A { X = i });
        }

        int x = 0;
        foreach (var entity in ecs.Query<A>())
        {
            outputIds[x] = entity;
            x++;
        }

        Assert.Equal(ids, outputIds);
    }

    [Fact]
    public void Test_Query_MultipleComponents_ReturnsCorrect_Entities()
    {
        ECS ecs = new ECS();
        ecs.RegisterComponent<A>();
        ecs.RegisterComponent<B>();
        (A, B)[] ids = new (A, B)[5] {
          (new A { X = 0 }, new B { Y = 0 }),
          (new A { X = 2 }, new B { Y = 2 }),
          (new A { X = 4 }, new B { Y = 4 }),
          (new A { X = 6 }, new B { Y = 6 }),
          (new A { X = 8 }, new B { Y = 8 })
        };
        (A, B)[] outputIds = new (A, B)[5];
        for (int i = 0; i < 10; i++)
        {
            var entity = ecs.AddEntity();
            if (i % 2 == 0)
            {
                ecs.Add<A>(entity, new A { X = i });
                ecs.Add<B>(entity, new B { Y = i });
            }
        }

        int x = 0;
        foreach (var (eA, eB) in ecs.Query<A, B>())
        {
            outputIds[x] = (eA, eB);
            x++;
        }

        Assert.Equal(ids, outputIds);
    }

    [Fact]
    public void Test_Query_EmptyPool_Returns_Nothing()
    {
        ECS ecs = new ECS();
        ecs.RegisterComponent<A>();
        A[] ids = new A[5];
        A[] outputIds = new A[5];
        for (int i = 0; i < 10; i++)
        {
            var entity = ecs.AddEntity();
        }

        int x = 0;
        foreach (var entity in ecs.Query<A>())
        {
            outputIds[x] = entity;
            x++;
        }

        Assert.Equal(ids, outputIds);
    }

    [Fact]
    public void Test_Query_EntityRemoved_SkipsEntity()
    {
        ECS ecs = new ECS();
        ecs.RegisterComponent<A>();
        A[] outputIds = new A[5];
        for (int i = 0; i < 10; i++)
        {
            var entity = ecs.AddEntity();
            if (i % 2 == 0)
                ecs.Add<A>(entity, new A { X = i });
            if (i % 6 == 0)
            {
                ecs.Remove<A>(entity);
            }
        }

        int x = 0;
        foreach (var entity in ecs.Query<A>())
        {
            outputIds[x] = entity;
            x++;
        }

        Assert.DoesNotContain(new A { X = 6 }, outputIds);
    }

    [Fact]
    public void Test_Query_WithFilter_IgnoresOutOfViewEntities()
    {
        ECS ecs = new ECS();
        ecs.RegisterComponent<A>();
        A[] outputIds = new A[2];
        for (int i = 0; i < 10; i++)
        {
            var entity = ecs.AddEntity();
            if (i % 2 == 0)
                ecs.Add<A>(entity, new A { X = i });
        }

        int x = 0;
        foreach (var entity in ecs.Query<A>(id => (ecs.Get<A>(id).X > 2 && ecs.Get<A>(id).X < 8)))
        {
            Console.WriteLine(entity.X);
            outputIds[x] = entity;
            x++;
        }

        Assert.Equal(new A[] { new A { X = 4 }, new A { X = 6 } }, outputIds);
    }
}
