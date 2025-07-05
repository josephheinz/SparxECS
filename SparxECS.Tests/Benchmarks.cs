namespace SparxECS.Tests;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SparxECS;
using Xunit;

[MemoryDiagnoser]
public class ECSBenchmarks
{
    private ECS? ecs;

    [BenchmarkDotNet.Attributes.GlobalSetup]
    public void Setup()
    {
        ecs = new ECS();
    }

    [BenchmarkDotNet.Attributes.Benchmark]
    public void Benchmark_Add_1000_Entities()
    {
        ECS ecs = new ECS();
        for (int i = 0; i < 1000; i++)
        {
            ecs.AddEntity();
        }
    }
}

public class RunBenchmarks
{
    [Fact]
    public void RunECSBenchmarks()
    {
        var runBenchmarks = Environment.GetEnvironmentVariable("DOTNET_BENCHMARK");
        if (string.Equals(runBenchmarks, "true", StringComparison.OrdinalIgnoreCase))
        {
            BenchmarkRunner.Run<ECSBenchmarks>();
        }
        else
        {
            Console.WriteLine("Skipping benchmarks");
        }
    }
}
