using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace STUN;
public class ServerAddress
{
    private string? _domain;
    private IPAddress? _address;
    public string? Domain { get=>_domain; 
        set 
        {
            _domain = value;
            _address = null;
        } 
    }
    public IPAddress? Address { get=>_address; set 
        {
            _domain = null;
            _address = value;
        } 
    }
    public ushort Port { get; set; }
    public string Host { get => Domain ?? Address.ToString(); }
    public ServerAddress(string domain,ushort port)
    {
        Domain = domain;
        Port = port;
    }
    public ServerAddress(IPAddress address,ushort port)
    {
        Address = address;
        Port = port;
    }
    public ServerAddress(IPEndPoint endpoint)
    {
        Address = endpoint.Address;
        Port = (ushort)endpoint.Port;
    }
    public override string ToString()
    {
        return $"{Domain ?? Address.ToString()}:{Port}";
    }
}



