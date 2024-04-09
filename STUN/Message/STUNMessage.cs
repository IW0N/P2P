using STUN.Attributes.Base;
using STUN.Header;
using STUN.Interfaces;
using System.Net;

namespace STUN.Message
{
    public class STUNMessage
    {
        static readonly MessageFactory MsgFactory = new();
        public MessageHeader Header { get; }
        private List<STUNAttribute> Attributes { get; } = new();
        public STUNMessage(STUNClass requestType, Method method = Method.Binding)
        {
            Header = new()
            {
                TransactionID = TransactionID.GetRandom(),
                MsgType = new MessageType(requestType, method)
            };
        }
        public STUNMessage(TransactionID id, STUNClass actionType, Method method = Method.Binding)
        {
            Header = new()
            {
                TransactionID = id,
                MsgType = new MessageType(actionType, method)
            };
        }
        public STUNMessage(MessageHeader header,IEnumerable<STUNAttribute> attrs) {
            Header = header;
            Attributes.AddRange(attrs);
        }
        public void AddAttribute(STUNAttribute attr)
        {
            bool hasAttr = Attributes.FirstOrDefault(a=>a.Type==attr.Type)!=null;
            if (!hasAttr)
            {
                Attributes.Add(attr);
                Header.MessageLength += attr.TotalLength;
            }
        }
        public void RemoveAttribute(AttributeType type)
        {
            var attr=Attributes.FirstOrDefault(a => a.Type == type);
            if (attr != null)
            {
                Attributes.Remove(attr);
                Header.MessageLength -= attr.TotalLength;
            }
        }
        public void AddAttributes(IEnumerable<STUNAttribute> attrs)
        {
            foreach (STUNAttribute attr in attrs)
            {
                AddAttribute(attr);
            }
        }
        public void RemoveAttributes(IEnumerable<AttributeType> types)
        {
            foreach (AttributeType type in types)
            {
                RemoveAttribute(type);
            }
        }
        public STUNAttribute? GetAttribute(AttributeType type)
        {
            try
            {
                var attr = Attributes.First(a => a.Type == type);
                return attr;
            }
            catch
            {
                return null;
            }
        }
        internal byte[] ToBytes()
        {
            int total_length = Header.MessageLength+MessageHeader.header_size;
            byte[] msg = new byte[total_length];
            MsgFactory.CopyToBytes(this, msg);
            return msg;
        }
        internal static STUNMessage FromBytes(byte[] source)
        {
            return MsgFactory.ParseBytes(source);
        }
        
    }
}
