using BepuPhysics;
using BepuUtilities;
using BepuUtilities.Memory;
using System.Numerics;

namespace IPCServer.Scenes;

public abstract class Scene : IDisposable
{
    public const float TimestepDuration = 1 / 60f;

    public Simulation simulation;
    public BufferPool bufferPool;
    ThreadDispatcher threadDispatcher;
    public abstract void CreateSimulation();

    public Scene()
    {
        bufferPool = new BufferPool();
        var targetThreadCount = int.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
        threadDispatcher = new ThreadDispatcher(targetThreadCount);
    }

    public void Dispose()
    {
        simulation.Dispose();
        bufferPool.Clear();
    }

    public virtual void UpdateSimulation()
    {
        simulation.Timestep(TimestepDuration, threadDispatcher);
    }

    public virtual void ScreenLeftClick(Vector3 position, Vector3 direction) { }
}

public class TestClass
{
    public int Hi(int lol)
    {
        return lol + 1;
    }
}