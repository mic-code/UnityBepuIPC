using IPCServer.Scenes;
using IPCServer;
using IPCServer.Utility;

namespace Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Simple()
        {
            var simpleScene = new Simple();
            simpleScene.CreateSimulation();

            simpleScene.UpdateSimulation();

            Assert.AreEqual(1, 1);
        }

        [Test]
        public void MessageTypeToByte()
        {
            var messageType = MessageType.Init;
            var buffer = new byte[4];
            ByteConvert.MessageTypeToByteArray(messageType, buffer);
            var messageType2 = (MessageType)BitConverter.ToInt32(buffer, 0);
            Assert.AreEqual(messageType, messageType2);
        }

        [Test]
        public void MessageTypeCast()
        {
            var messageType = MessageType.Init;
            var i = (int)MessageType.Init;

            Assert.AreEqual(messageType, (MessageType)i);
        }
    }
}