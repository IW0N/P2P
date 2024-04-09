using System.Net;

namespace STUN.Attributes.Values
{
    public class MappedAddress
    {
        public STUNAddressFamily Family { get; }
        public ushort Port { get; }
        public IPAddress Address { get; }
        public MappedAddress(STUNAddressFamily family, ushort port, IPAddress globalAddress)
        {
            Family = family;
            Port = port;
            Address = globalAddress;
        }
        public static explicit operator IPEndPoint(MappedAddress address)
        {
            return new IPEndPoint(address.Address, address.Port);
        }
    }
    public enum STUNAddressFamily
    {
        IPv4=1,
        IPv6
    }
}
