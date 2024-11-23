using BepuPhysics;
using BepuUtilities.Memory;
using IPCServer.Scenes;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Numerics;

namespace IPCServer;

public class BepuSim
{
    static BodyHandle bodyHandle;
    static MemoryMappedFile mmf;
    static Scene scene;
    const string mapName = "SimData";
    static Stopwatch stepSW;
    public static double stepTime;

    public static void Init()
    {
        mmf = MemoryMappedFile.CreateNew(mapName, 1024 * 1024);
        stepSW = new Stopwatch();
    }

    public static void CreateSimulationInstance()
    {
        scene = new Colosseum();
        scene.CreateSimulation();
    }

    public static void DestroySimulation()
    {
        scene.Dispose();
    }

    public static void StepSimulation()
    {
        stepSW.Restart();
        scene.UpdateSimulation();
        stepTime = (double)stepSW.ElapsedTicks / TimeSpan.TicksPerMillisecond;
    }

    public static void ScreenLeftClick(Vector3 position, Vector3 direction)
    {
        scene.ScreenLeftClick(position, direction);
    }

    public static Buffer<BodyDynamics> GetActiveStates()
    {
        return scene.simulation.Bodies.ActiveSet.DynamicsState;
    }

    public static int WriteUpdatedData()
    {
        const int size = 64;
        var a = mmf.CreateViewAccessor();
        var count = 0;

        for (int x = 0; x < scene.simulation.Bodies.Sets.Length; x++)
        {
            var set = scene.simulation.Bodies.Sets[x];
            if (set.Allocated)
            {
                var state = set.DynamicsState;
                for (int y = 0; y < set.Count; y++)
                {
                    var s = state[y];
                    a.Write(count * size, ref s.Motion);
                    count++;
                }
            }
        }
        return count;
    }

    internal static void Reset()
    {
        DestroySimulation();
        CreateSimulationInstance();
    }
}
