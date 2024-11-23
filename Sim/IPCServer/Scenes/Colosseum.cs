using BepuPhysics.Collidables;
using BepuPhysics;
using BepuUtilities;
using IPCServer.Physics;
using System.Numerics;
using BepuPhysics.Constraints;

namespace IPCServer.Scenes;

public class Colosseum : Scene
{
    BodyDescription bulletDescription;

    public override void CreateSimulation()
    {
        var narrow = new DemoNarrowPhaseCallbacks(new SpringSettings(30, 1));
        var integrator = new DemoPoseIntegratorCallbacks(new Vector3(0, -10f, 0));
        var solver = new SolveDescription(8, 1);
        simulation = Simulation.Create(bufferPool, narrow, integrator, solver);

        var boxShape = new Box(1, 1, 1);
        var bodyDesc = BodyDescription.CreateDynamic(new Vector3(), boxShape.ComputeInertia(1), simulation.Shapes.Add(boxShape), 0.01f);

        var ringBoxShape = new Box(0.5f, 1, 3);
        var boxDescription = BodyDescription.CreateDynamic(new Vector3(), ringBoxShape.ComputeInertia(1), simulation.Shapes.Add(ringBoxShape), 0.01f);

        var layerPosition = new Vector3();
        const int layerCount = 6;
        var innerRadius = 15f;
        var heightPerPlatform = 3;
        var platformsPerLayer = 1;
        var ringSpacing = 0.5f;
        for (int layerIndex = 0; layerIndex < layerCount; ++layerIndex)
        {
            var ringCount = layerCount - layerIndex;
            for (int ringIndex = 0; ringIndex < ringCount; ++ringIndex)
            {
                CreateRing(simulation, layerPosition, ringBoxShape, boxDescription, innerRadius + ringIndex * (ringBoxShape.Length + ringSpacing) + layerIndex * (ringBoxShape.Length - ringBoxShape.Width), heightPerPlatform, platformsPerLayer);
            }
            layerPosition.Y += platformsPerLayer * (ringBoxShape.Height * heightPerPlatform + ringBoxShape.Width);
        }

        simulation.Statics.Add(new StaticDescription(new Vector3(0, -0.5f, 0), simulation.Shapes.Add(new Box(500, 1, 500))));

        var bulletShape = new Sphere(0.5f);
        bulletDescription = BodyDescription.CreateDynamic(new Vector3(), bulletShape.ComputeInertia(.1f), simulation.Shapes.Add(bulletShape), 0.01f);
    }

    public override void ScreenLeftClick(Vector3 position, Vector3 direction)
    {
        bulletDescription.Pose.Position = position;
        bulletDescription.Velocity.Linear = direction * 400;
        simulation.Bodies.Add(bulletDescription);
    }

    public static void CreateRingWall(Simulation simulation, Vector3 position, Box ringBoxShape, BodyDescription bodyDescription, int height, float radius)
    {
        var circumference = MathF.PI * 2 * radius;
        var boxCountPerRing = (int)(0.9f * circumference / ringBoxShape.Length);
        float increment = MathHelper.TwoPi / boxCountPerRing;
        for (int ringIndex = 0; ringIndex < height; ringIndex++)
        {
            for (int i = 0; i < boxCountPerRing; i++)
            {
                var angle = ((ringIndex & 1) == 0 ? i + 0.5f : i) * increment;
                bodyDescription.Pose = (position + new Vector3(-MathF.Cos(angle) * radius, (ringIndex + 0.5f) * ringBoxShape.Height, MathF.Sin(angle) * radius), QuaternionEx.CreateFromAxisAngle(Vector3.UnitY, angle));
                simulation.Bodies.Add(bodyDescription);
            }
        }
    }

    public static void CreateRingPlatform(Simulation simulation, Vector3 position, Box ringBoxShape, BodyDescription bodyDescription, float radius)
    {
        var innerCircumference = MathF.PI * 2 * (radius - ringBoxShape.HalfLength);
        var boxCount = (int)(0.95f * innerCircumference / ringBoxShape.Height);
        float increment = MathHelper.TwoPi / boxCount;
        for (int i = 0; i < boxCount; i++)
        {
            var angle = i * increment;
            bodyDescription.Pose = (position + new Vector3(-MathF.Cos(angle) * radius, ringBoxShape.HalfWidth, MathF.Sin(angle) * radius),
                QuaternionEx.Concatenate(QuaternionEx.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI * 0.5f), QuaternionEx.CreateFromAxisAngle(Vector3.UnitY, angle + MathF.PI * 0.5f)));
            simulation.Bodies.Add(bodyDescription);
        }
    }

    public static Vector3 CreateRing(Simulation simulation, Vector3 position, Box ringBoxShape, BodyDescription bodyDescription, float radius, int heightPerPlatformLevel, int platformLevels)
    {
        for (int platformIndex = 0; platformIndex < platformLevels; ++platformIndex)
        {
            var wallOffset = ringBoxShape.HalfLength - ringBoxShape.HalfWidth;
            CreateRingWall(simulation, position, ringBoxShape, bodyDescription, heightPerPlatformLevel, radius + wallOffset);
            CreateRingWall(simulation, position, ringBoxShape, bodyDescription, heightPerPlatformLevel, radius - wallOffset);
            CreateRingPlatform(simulation, position + new Vector3(0, heightPerPlatformLevel * ringBoxShape.Height, 0), ringBoxShape, bodyDescription, radius);
            position.Y += heightPerPlatformLevel * ringBoxShape.Height + ringBoxShape.Width;
        }
        return position;
    }
}
