using STUN.Interfaces;

namespace STUN.Header.Factories
{
    internal class MessageTypeFactory : IBytesConvertable<MessageType>
    {
        const ushort MethodLeftMask   =  0b0000111110000000;
        const ushort MethodCenterMask =  0b0000000001110000;
        const ushort MethodRightMask  =  0b0000000000001111;

        const byte MethodLeftMask1   = 0b00111110;
        const byte MethodCenterMask1 = 0b11100000;
        const byte MethodRightMask1  = 0b00001111;

        const byte ClassLeftMask1    = 0b00000001;
        const byte ClassRightMask1   = 0b00010000;
        private static void PrintNBaseNumber(string msg, int num, int _base = 2)
        {
            Console.Write(msg + ": ");
            Console.WriteLine(Convert.ToString(num, _base));
        }
        public void CopyToBytes(MessageType source,byte[] destination)
        {
            ushort method = (ushort)source.Method;
            int m_left = (method & MethodLeftMask) << 2;
            int m_center = (method & MethodCenterMask) << 1;
            int m_right = method & MethodRightMask;

            byte _class = (byte)source.Class;
            int c_left = (_class & 0b10) << 7;
            int c_right = (_class & 0b01) << 4;

            ushort bin_method = (ushort)(m_left | m_center | m_right);
            ushort bin_class = (ushort)(c_left | c_right);

            ushort message_type = (ushort)(bin_method | bin_class);
            byte msg_type_right = (byte)message_type;
            byte msg_type_left = (byte)(message_type >> 8);
            destination[0] = msg_type_left;
            destination[1] = msg_type_right;
        }
        private static Method ParseMethod(byte[] source)
        {
            int m_left = (source[0] & MethodLeftMask1) << 7;
            int m_center = source[1] & MethodCenterMask & MethodCenterMask1 >> 1;
            int m_right = source[1] & MethodRightMask1;

            ushort method_bin = (ushort)(m_left | m_center | m_right);
            return (Method)method_bin;
        }
        private static STUNClass ParseClass(byte[] source)
        {
            int c_left = (source[0]&ClassLeftMask1)<<1;
            int c_right = (source[1]&ClassRightMask1)>>4;
            byte class_bin = (byte)(c_left|c_right);
            return (STUNClass)class_bin;
        }
        public MessageType ParseBytes(byte[] source)
        {
            Method method = ParseMethod(source);
            STUNClass _class = ParseClass(source);
            return new(_class, method);
        }
    }
}
