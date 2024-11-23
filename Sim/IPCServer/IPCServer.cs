using IPCServer.Utility;
using MessagePack;
using System.Buffers;
using System.IO.Pipes;
using System.Numerics;

namespace IPCServer;

public class IPCServer
{
    const string renderToSimPipeName = "RenderToSim";
    const string simToRenderPipeName = "SimToRender";

    static NamedPipeServerStream toSim;
    static NamedPipeClientStream toRender;


    static StreamWriter toSimWriter;
    static StreamReader toSimReader;

    static StreamWriter toRenderWriter;
    static StreamReader toRenderReader;
    static byte[] writeMessageTypeBuff = new byte[4];

    static void Main(string[] args)
    {
        Console.WriteLine("Start IPC Server");

        toRender = new NamedPipeClientStream(simToRenderPipeName);
        toSim = new NamedPipeServerStream(renderToSimPipeName);

        toSim.WaitForConnection();
        toRender.Connect(10000);

        toRenderReader = new StreamReader(toRender);
        toRenderWriter = new StreamWriter(toRender);

        toSimReader = new StreamReader(toSim);
        toSimWriter = new StreamWriter(toSim);

        long frameCount = 0;
        var buffer = new byte[1024 * 1024];

        while (true)
        {
            bool ack = true;
            try
            {
                var messageTypeBuff = new byte[4];
                toSim.Read(messageTypeBuff);
                var messageType = (MessageType)BitConverter.ToInt32(messageTypeBuff, 0);

                switch (messageType)
                {
                    case MessageType.Init: BepuSim.Init(); break;
                    case MessageType.CreateSimulationInstance: BepuSim.CreateSimulationInstance(); break;
                    case MessageType.WriteUpdatedData:
                        {
                            var count = BepuSim.WriteUpdatedData();
                            ack = false;
                            WriteToRender(MessageType.Done, new IntDTO { Value = count });
                        }
                        break;
                    case MessageType.StepSimulation:
                        BepuSim.StepSimulation();
                        frameCount++;
                        ack = false;
                        WriteToRender(MessageType.StepSimulation, new DoubleDTO { Value = BepuSim.stepTime });
                        break;
                    case MessageType.ScreenLeftClick:
                        {
                            var count = toSim.Read(buffer);
                            var span = new ReadOnlySequence<byte>(buffer, 0, count);
                            var ray = MessagePackSerializer.Deserialize<RayDTO>(span);
                            BepuSim.ScreenLeftClick(ray.Origin, ray.Direction);
                        }
                        break;
                    case MessageType.ResetSimulation:
                        BepuSim.Reset();
                        break;
                    default:
                        ack = false;
                        WriteToRender(MessageType.Unknown);
                        break;
                }

            }
            catch (Exception e)
            {
                var eDto = new ExceptionDTO()
                {
                    Type = e.GetType().Name,
                    Message = e.Message,
                    StackTrace = e.StackTrace
                };

                ack = false;
                WriteToRender(MessageType.Error, eDto);
            }
            finally
            {
                if (ack)
                    WriteToRender(MessageType.Done);
            }
        }
    }

    static void WriteToRender(MessageType messageType, object? payload = null)
    {
        ByteConvert.MessageTypeToByteArray(messageType, writeMessageTypeBuff);
        toSim.Write(writeMessageTypeBuff);

        if (payload != null)
            toSim.Write(MessagePackSerializer.Serialize(payload));
    }



    static void SendFromSim(string payload)
    {
        toSimWriter.WriteLine(payload);
        toSimWriter.Flush();
    }

    static void SendToRender(string payload)
    {
        toRenderWriter.WriteLine(payload);
        toRenderWriter.Flush();
        toRenderReader.ReadLine();
    }
}

[MessagePackObject]
public class MessagePackDTO
{
    [Key(0)]
    public string Type { get; set; }
    [Key(1)]
    public byte[] Data { get; set; }
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
public class RayDTO
{
    [Key(0)]
    public Vector3 Origin { get; set; }
    [Key(1)]
    public Vector3 Direction { get; set; }
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