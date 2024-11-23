using BepuPhysics.Collidables;
using BepuPhysics;
using BepuUtilities;
using IPCServer.Physics;
using System.Numerics;

namespace IPCServer.Scenes;

public class Simple : Scene
{
    BodyDescription bulletDescription;

    public override void CreateSimulation()
    {
        var narrow = new NarrowPhaseCallbacks();
        var integrator = new PoseIntegratorCallbacks(new Vector3(0, -10f, 0));
        var solver = new SolveDescription(8, 1);
        simulation = Simulation.Create(bufferPool, narrow, integrator, solver);
        simulation.Statics.Add(new StaticDescription(new Vector3(0, -0.5f, 0), simulation.Shapes.Add(new Box(500, 1, 500))));

        var boxShape = new Box(1, 1, 1);
        var bodyDesc = BodyDescription.CreateDynamic(new Vector3(), boxShape.ComputeInertia(1), simulation.Shapes.Add(boxShape), 0.01f);

        int spacing = 2;
        for (int x = 0; x < 20; x++)
            for (int y = 0; y < 10; y++)
                for (int z = 0; z < 20; z++)
                {
                    bodyDesc.Pose = (new Vector3(x * spacing, y * spacing + 10, z * spacing), QuaternionEx.CreateFromAxisAngle(Vector3.UnitY, 0));
                    simulation.Bodies.Add(bodyDesc);
                }

        var bulletShape = new Sphere(0.5f);
        bulletDescription = BodyDescription.CreateDynamic(new Vector3(), bulletShape.ComputeInertia(.1f), simulation.Shapes.Add(bulletShape), 0.01f);
    }

    public override void ScreenLeftClick(Vector3 position, Vector3 direction)
    {
        bulletDescription.Pose.Position = position;
        bulletDescription.Velocity.Linear = direction * 400;
        simulation.Bodies.Add(bulletDescription);
    }
}
