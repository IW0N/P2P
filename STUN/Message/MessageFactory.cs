using STUN.Attributes.Base;
using STUN.Attributes.Factories;
using STUN.Header;
using STUN.Header.Factories;
using STUN.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STUN.Message
{
    internal class MessageFactory : IBytesConvertable<STUNMessage>
    {
        readonly MessageHeaderFactory headerFatory = new();
        readonly Dictionary<AttributeType, AttributeFactory> factories = new() 
        {
            { AttributeType.MappedAddress, new MappedAddressFactory() }
        };
        public void CopyToBytes(STUNMessage source, byte[] destination)
        {
            headerFatory.CopyToBytes(source.Header, destination);
            
        }
        private AttributeType ParseType(byte[] source, int begin)
        {
            int first = source[begin]<<8;
            int second = source[begin+1];
            ushort bin_type = (ushort)(first|second);
            return (AttributeType)bin_type;
        }
        private ushort ParseAttributeLength(byte[] source, int begin)
        {
            int first = source[begin+2] << 8;
            int second = source[begin + 3];
            ushort length = (ushort)(first | second);
            return length;
        }
        private Dictionary<AttributeType, ushort> GetAttributeBegins(byte[] source,MessageHeader header)
        {
            byte shift = MessageHeader.header_size;
            ushort length = header.MessageLength;
            ushort delta;
            Dictionary<AttributeType, ushort> dict = new();
            const ushort attribute_header_length = STUNAttribute.header_size;
            for (ushort x=0;x<length;x+=delta)
            {
                int index = x+shift;
                
                AttributeType type = ParseType(source,index);
                ushort attr_value_length = ParseAttributeLength(source,index);
                if (factories.ContainsKey(type))
                {
                    dict.Add(type, (ushort)index);
                }
                delta = (ushort)(attr_value_length+attribute_header_length);
            }
            return dict;
        }
        private List<STUNAttribute> ParseAttributes(byte[] source,MessageHeader header)
        {
            var attr_factories = GetAttributeBegins(source, header);
            List<STUNAttribute> attrs = new();
            foreach (var pair in attr_factories)
            {
                AttributeType _type = pair.Key;
                ushort begin = pair.Value;
                AttributeFactory factory = factories[_type];
                factory.BeginPosition = begin;
                STUNAttribute attr = factory.ParseBytes(source);
                attrs.Add(attr);
            }
            return attrs;
        }
        public STUNMessage ParseBytes(byte[] source)
        {
            MessageHeader header=headerFatory.ParseBytes(source);
            List<STUNAttribute> attrs = ParseAttributes(source, header);
            return new STUNMessage(header, attrs);
        }
    }
}
