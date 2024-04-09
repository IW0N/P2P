using STUN.Attributes.Values;
using STUN.Header;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace STUN.Attributes.Factories
{
    internal class MappedAddressFactory : AttributeFactory
    {
        protected override void PackValueToBytes(object source, byte[] destination)
        {
            throw new NotImplementedException("Функция не поддерживается");
        }
        private ushort ParsePort(byte[] source)
        {
            int b0 = source[BeginPosition+2]<<8;
            byte b1 = source[BeginPosition+3];
            return (ushort)(b0 | b1);
        }
        private IPAddress ParseIPAddress(byte[] source,STUNAddressFamily addressFamily)
        {
            return addressFamily switch
            {
                STUNAddressFamily.IPv4 => ParseIPv4(source),
                STUNAddressFamily.IPv6 => ParseIPv6(source),
                _ => throw new NotImplementedException($"Для нового типа адреса {addressFamily} нет парсера!"),
            };

        }
        private IPAddress ParseIPv4(byte[] source)
        {
            int first = source[BeginPosition+4]<<24;
            int second = source[BeginPosition+5]<<16;
            int third = source[BeginPosition+6]<<8;
            int fourth = source[BeginPosition+7];
            uint ipv4=(uint)(first | second | third | fourth);
            return new IPAddress(ipv4);
        }
        private IPAddress ParseIPv6(byte[] source)
        {
            long data = 0,address_pice;
            for (int x=0;x<16;x++)
            {
                int index = BeginPosition+4+x;
                int shift = 128-(x+1)*8;
                address_pice = ((long)source[index])<<shift;
                data |= address_pice;
            }
            return new IPAddress(data);
        }
        protected override object ParseValue(byte[] source)
        {
            byte family = source[BeginPosition+1];
            STUNAddressFamily address_type = (STUNAddressFamily)family;
            ushort port = ParsePort(source);
            IPAddress address = ParseIPAddress(source, address_type);
            return new MappedAddress(address_type,port,address);
        }
    }
}
