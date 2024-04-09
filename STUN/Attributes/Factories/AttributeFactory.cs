using STUN.Attributes.Base;
using STUN.Interfaces;

namespace STUN.Attributes.Factories
{
    internal abstract class AttributeFactory : IBytesConvertable<STUNAttribute>
    {
        public ushort BeginPosition { get; set; }
        protected abstract void PackValueToBytes(object attribute_value, byte[] destination);
        protected abstract object ParseValue(byte[] source);
        protected static ushort ParseUshort(byte[] source,ushort begin_position)
        {
            ushort data = (ushort)(source[begin_position] << 8 | source[begin_position + 1]);
            return data;
        }
        protected static void CopyUshortToArr(ushort data, byte[] destination, ushort begin_position)
        {
            destination[begin_position] = (byte)(data>>8);
            destination[begin_position + 1] = (byte)data;
        }
        public void CopyToBytes(STUNAttribute source, byte[] destination)
        {
            ushort bin_type = (ushort)source.Type;
            CopyUshortToArr(bin_type,destination,BeginPosition);
            CopyUshortToArr(source.ValueLength, destination, (ushort)(BeginPosition + 2));
            PackValueToBytes(source.Value, destination);
        }

        public STUNAttribute ParseBytes(byte[] source)
        {
            ushort bin_type = ParseUshort(source,BeginPosition);
            AttributeType type = (AttributeType)bin_type;

            ushort length = ParseUshort(source,(ushort)(BeginPosition+2));
            BeginPosition += STUNAttribute.header_size;
            object value = ParseValue(source);
            return new STUNAttribute(type, length, value);
        }
    }
}
