namespace IPCServer.Utility
{
    public class ByteConvert
    {
        public static void MessageTypeToByteArray(MessageType messageType, byte[] array)
        {
            var value = (int)messageType;
            if (BitConverter.IsLittleEndian)
                unchecked
                {
                    array[0] = (byte)(value >> (8 * 4));
                    array[1] = (byte)(value >> (8 * 3));
                    array[2] = (byte)(value >> (8 * 2));
                    array[3] = (byte)(value >> (8 * 1));
                }
            else
                unchecked
                {
                    array[0] = (byte)(value >> (8 * 1));
                    array[1] = (byte)(value >> (8 * 2));
                    array[2] = (byte)(value >> (8 * 3));
                    array[3] = (byte)(value >> (8 * 4));
                }
        }
    }
}