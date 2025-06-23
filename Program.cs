using SparxEcs;
ECS ecs = new ECS();
ecs.RegisterComponent<string>();
EntityID entity = ecs.AddEntity();
ecs.Add<string>(entity, "asdf");
Console.WriteLine(ecs.Get<string>(entity));
