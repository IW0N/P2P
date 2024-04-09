using STUN.Interfaces;

namespace STUN.Header.Factories
{
    internal class MessageHeaderFactory : IBytesConvertable<MessageHeader>
    {
        static readonly MessageTypeFactory msgTypeFactory = new();
        private void MoveMsgLengthToBytes(MessageHeader header, byte[] bytes)
        {
            ushort msg_len = header.MessageLength;
            const ushort mask_left = 0b11111111_00000000;
            byte b0 = (byte)(msg_len & mask_left);
            byte b1 = (byte)msg_len;
            bytes[2] = b0;
            bytes[3] = b1;
        }
        void MoveIntToBytes(int beginIndex, int value, byte[] bytes)
        {
            for (byte i = 0; i < 4; i++)
                bytes[beginIndex + i] = (byte)(value >> 24 - i * 8);
        }
        private void MoveMagicCookieToBytes(byte[] bytes)
        {
            int cookie = MessageHeader.MagicCookie;
            MoveIntToBytes(4, cookie, bytes);
        }
        private void MoveTransactionIdToBytes(MessageHeader source, byte[] bytes)
        {
            TransactionID transaction = source.TransactionID;
            var (a, b, c) = transaction;
            MoveIntToBytes(8, a, bytes);
            MoveIntToBytes(12, b, bytes);
            MoveIntToBytes(16, c, bytes);
        }

        public void CopyToBytes(MessageHeader source, byte[] destination)
        {
            
            msgTypeFactory.CopyToBytes(source.MsgType, destination);
            MoveMsgLengthToBytes(source, destination);
            MoveMagicCookieToBytes(destination);
            MoveTransactionIdToBytes(source, destination);
        }
        private int ParseInt(byte[] source, int begin)
        {
            int result = 0;

            for(int x = 0; x < 4; x++)
            {
                int index = begin+x;
                int pice = source[index]<<(24-x*8);
                result |= pice;
            }
            return result;
        }
        private ushort ParseLength(byte[] source)
        {
            int pice0 = source[2]<<8;
            int pice1 = source[3];
            return (ushort)(pice0 | pice1);
        }
        private int ParseMagicCookie(byte[] source)
        {
            int cookie = ParseInt(source,4);
            return cookie;
        }
        private TransactionID ParseTransactionID(byte[] source)
        {
            int first = ParseInt(source,8);
            int second = ParseInt(source,12);
            int third = ParseInt(source,16);
            return new TransactionID(first, second, third);
        }
        public MessageHeader ParseBytes(byte[] source)
        {
            MessageType type = msgTypeFactory.ParseBytes(source);
            ushort length = ParseLength(source);
            if (length != source.Length - MessageHeader.header_size)
                throw new InvalidDataException("Данные повреждены, неверная длина!");
            int cookie = ParseMagicCookie(source);
            if (cookie != MessageHeader.MagicCookie)
                throw new InvalidDataException("Данные повреждены, неправильный MagicCookie!");
            TransactionID id = ParseTransactionID(source);
            return new MessageHeader() { MsgType=type, MessageLength = length, TransactionID=id};
        }
    }
}
