using SparxEcs;
ECS ecs = new ECS();
ecs.RegisterComponent<string>();
EntityID entity = ecs.AddEntity();
