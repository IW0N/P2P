namespace STUN.Header
{
    public class MessageHeader
    {
        public const byte header_size=20;
        public ushort max_size = 576;
        public const ushort MTU = ushort.MaxValue;
        public MessageType MsgType { get; set; }
        private ushort _msg_len = 0;
        public ushort MessageLength
        {
            get => _msg_len; 
            set
            {
                const string fieldName = nameof(MessageLength);
                if (value > max_size)
                    throw new ArgumentOutOfRangeException(nameof(MessageLength),
                        $"{fieldName} can not be bigger than {max_size} bytes!");
                else
                    _msg_len = value;
            }
        }
        public const int MagicCookie = 0x2112A442;
        public TransactionID TransactionID { get; set; }
        

    }
}
