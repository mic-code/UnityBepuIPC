using System;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using Overimagined.Common;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Threading.Tasks;
using System.Buffers;
using System.Runtime.InteropServices;
using MessagePack;
using IPCServer.Utility;

public class SimulationManager : SingletonMono<SimulationManager>
{
    public bool debug;

    const string renderToSimPipeName = "RenderToSim";
    const string simToRenderPipeName = "SimToRender";
    const string mapName = "SimData";

    MemoryMappedFile mmf;

    Process process;
    NamedPipeClientStream toSim;
    NamedPipeServerStream toRender;

    StreamWriter toSimWriter;
    StreamReader toSimReader;

    StreamWriter toRenderWriter;
    StreamReader toRenderReader;

    CancellationTokenSource simToRenderListenTokenSrc;

    byte[] dtoBuffer = new byte[1024];

    bool isFinished;

    void Start()
    {

        StartProcess();


        toRender = new NamedPipeServerStream(simToRenderPipeName);
        toSim = new NamedPipeClientStream(renderToSimPipeName);

        toSim.Connect(10000);
        toRender.WaitForConnection();

        if (!toSim.IsConnected)
            return;

        Debug.Log("Connected to server");


        WriteToSim(MessageType.Init);
        WriteToSim(MessageType.CreateSimulationInstance);

        mmf = MemoryMappedFile.OpenExisting(mapName);

        ListenSimToRender();

        _ = DebugInfo.Instance;
        _ = BodyRenderer.Instance;
    }

    void OnDestroy()
    {
        isFinished = true;
        simToRenderListenTokenSrc?.Cancel();

        process?.Kill();
        toSim?.Dispose();
        toRender?.Dispose();
        mmf?.Dispose();
    }

    Task updateTask;
    double simTime = 0;
    double simTimeInternal = 0;


    void Update()
    {
        updateTask?.Wait();
        DebugInfo.Set("simR", simTime);
        var sw = Stopwatch.StartNew();
        WriteData();
        DebugInfo.Set("data", (double)sw.ElapsedTicks / TimeSpan.TicksPerMillisecond);
    }

    void LateUpdate()
    {
        updateTask = Task.Factory.StartNew(() =>
        {
            var sw2 = Stopwatch.StartNew();
            WriteToSim(MessageType.StepSimulation);
            simTime = (double)sw2.ElapsedTicks / TimeSpan.TicksPerMillisecond;
            toSim.Read(dtoBuffer, 0, dtoBuffer.Length);
            var dto = MessagePackSerializer.Deserialize<DoubleDTO>(dtoBuffer);
            simTimeInternal = dto.Value;
        });
    }

    void StartProcess()
    {
        var mode = debug ? "Debug" : "Release";
        var path = $"{Application.dataPath}/bin/{mode}/net8.0/IPCServer.exe";

#if UNITY_EDITOR
        path = GetSimDir(mode) + "IPCServer.exe";
#endif

        var fullPath = Path.GetFullPath(path);
        process = ProcessUtility.StartProcess(fullPath, null, null, false);
    }

    public static string GetSimDir(string mode)
    {
        return $"{Application.dataPath}/../Sim/IPCServer/bin/{mode}/net8.0/";
    }

    void ThrowTest()
    {
        toSimWriter.WriteLine(SimMethod.Throw);
        toSimWriter.Flush();
        ReadToSimPipe();
    }

    void WriteData()
    {
        var result = WriteToSim(MessageType.WriteUpdatedData);
        if (result == MessageType.Done)
        {
            toSim.Read(dtoBuffer, 0, dtoBuffer.Length);
            var dto = MessagePackSerializer.Deserialize<IntDTO>(dtoBuffer);
            ReadData(dto.Value);
        }
    }


    void ReadData(int size)
    {
        var a = mmf.CreateViewAccessor();
        var array = new MotionState[size];
        a.ReadArray(0, array, 0, array.Length);
        BodyRenderer.Instance.RenderActive(array);
        DebugInfo.Set("count", array.Length);
    }

    void ListenSimToRender()
    {
        simToRenderListenTokenSrc = new CancellationTokenSource();

        Task.Factory.StartNew(() =>
        {
            var buffer = new byte[1024 * 1024];
            while (!isFinished)
            {
                var count = toRender.Read(buffer);
                var span = new ReadOnlySequence<byte>(buffer, 0, count);

                if (count == 0)
                    continue;

                try
                {
                    var dto = MessagePackSerializer.Deserialize<ExceptionDTO>(span);
                    //Debug.Log(MessagePackSerializer.ConvertToJson(span));

                    if (dto is ExceptionDTO e)
                    {
                        Debug.LogError(e.Message + "\n" + e.StackTrace);
                    }

                    toRenderWriter.WriteLine("");
                    toRenderWriter.Flush();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }, simToRenderListenTokenSrc.Token);
    }

    void ReadToSimPipe()
    {
        var response = toSimReader.ReadLine();
        //Debug.Log(response);
    }

    byte[] writeMessageTypeBuff = new byte[4];
    byte[] readMessageTypeBuff = new byte[4];

    MessageType WriteToSim(MessageType messageType, object payload = null)
    {
        ByteConvert.MessageTypeToByteArray(messageType, writeMessageTypeBuff);
        toSim.Write(writeMessageTypeBuff);

        if (payload != null)
            toSim.Write(MessagePackSerializer.Serialize(payload));
        toSim.Flush();

        return ReadToSim();
    }

    MessageType ReadToSim()
    {
        toSim.Read(readMessageTypeBuff);
        var result = (MessageType)BitConverter.ToInt32(readMessageTypeBuff, 0);

        if (result == MessageType.Error)
        {
            var e = ReadExcepton();
            Debug.LogError(e.Message + "\n" + e.StackTrace);
        }

        return result;
    }

    ExceptionDTO ReadExcepton()
    {
        toSim.Read(dtoBuffer);
        return MessagePackSerializer.Deserialize<ExceptionDTO>(dtoBuffer);
    }

    public void ShootBall(Ray ray)
    {
        WriteToSim(MessageType.ScreenLeftClick, new RayDTO { Origin = ray.origin.ToNVector(), Direction = ray.direction.ToNVector() });
    }

    public void Reset()
    {
        WriteToSim(MessageType.ResetSimulation);
    }
}

static class SimMethod
{
    public const string Init = nameof(Init);
    public const string CreateSimulationInstance = nameof(CreateSimulationInstance);
    public const string StepSimulation = nameof(StepSimulation);
    public const string WriteUpdatedData = nameof(WriteUpdatedData);
    public const string GetBodyPos = nameof(GetBodyPos);
    public const string Throw = nameof(Throw);
    public const string TestDTO = nameof(TestDTO);
}

public enum MessageType
{
    Done,
    Error,
    Unknown,
    Init,
    CreateSimulationInstance,
    StepSimulation,
    WriteUpdatedData,
    ScreenLeftClick,
    ResetSimulation
}

[StructLayout(LayoutKind.Sequential, Size = 64, Pack = 1)]
public struct MotionState
{
    public RigidPose Pose;
    public BodyVelocity Velocity;
}

[StructLayout(LayoutKind.Sequential, Size = 32, Pack = 1)]
public struct RigidPose
{
    public Quaternion Orientation;
    public Vector3 Position;
}

[StructLayout(LayoutKind.Explicit, Size = 32)]
public struct BodyVelocity
{
    [FieldOffset(0)]
    public Vector3 Linear;
    [FieldOffset(16)]
    public Vector3 Angular;
}

[MessagePackObject]
public class ExceptionDTO
{
    [Key(0)]
    public string Type { get; set; }
    [Key(1)]
    public string Message { get; set; }
    [Key(2)]
    public string StackTrace { get; set; }
}

[MessagePackObject]
public class IntDTO
{
    [Key(0)]
    public int Value { get; set; }
}

[MessagePackObject]
public class DoubleDTO
{
    [Key(0)]
    public double Value { get; set; }
}

[MessagePackObject]
public class RayDTO
{
    [Key(0)]
    public System.Numerics.Vector3 Origin { get; set; }
    [Key(1)]
    public System.Numerics.Vector3 Direction { get; set; }
}