using System.Net;
using System.Net.Sockets;
using STUN.Header;
using STUN.Message;
using STUN.Attributes.Base;
using STUN.Attributes.Values;

namespace STUN;
public class STUNClient:IDisposable
{
    public string Domain { get; set; }
    public ushort Port { get; set; }
    public IPAddress IP { get; set; }
    private ServerAddress _server_address;
    public ServerAddress ServerTransportAddress 
    { 
        get => _server_address; 
        set {
            Domain = value.Domain;
            IP = value.Address;
            Port = value.Port;
            _server_address = value;
        } 
    }
    public ReusableUdpClient Udp { get; set; }=new();
    public STUNClient(string domain,ushort port=3478)
    {
        Domain = domain;
        Port = port;
        _server_address = new(Domain, Port);
    }
    public STUNClient(IPAddress address, ushort port=3478)
    {
        IP = address;
        Port = port;
        _server_address = new(IP, Port);
    }
    public STUNClient(ServerAddress address)
    {
        Domain = address.Domain;
        IP = address.Address;
        Port = address.Port;
        _server_address = address;
    }
    public STUNClient()
    {

    }
    private async Task<STUNMessage> SendMessage(STUNMessage message)
    {
        IPEndPoint? source = null;
        byte[] bytes = message.ToBytes();
        //using CancellationTokenSource src = new(1000);
        SendData(ServerTransportAddress, bytes);
        
        bytes = Udp.Receive(ref source);
        STUNMessage answer = STUNMessage.FromBytes(bytes);
        return answer;
    }
    public async Task<IPEndPoint> GetForeignIP()
    {
        STUNMessage msg = new(STUNClass.Request);
        msg = await SendMessage(msg);
        var type=msg.Header.MsgType;
        if (type.Class == STUNClass.SuccessResponse)
        {
            var attr=msg.GetAttribute(AttributeType.MappedAddress);
            if (attr != null)
            {
                MappedAddress address = (MappedAddress)attr.Value;
                return new IPEndPoint(address.Address,address.Port);
            }
            else
                throw new ArgumentException("Can not find \"Mapped-Address\" attribute!");
        }
        else if (type.Class == STUNClass.ErrorResponse)
            throw new ArgumentException("Error response!");
        else
            throw new ArgumentException("Incorrect answer format!");
    }
    public async Task SendIndication()
    {
        STUNMessage msg = new(STUNClass.Indication);
        byte[] bytes = msg.ToBytes();
        SendData(ServerTransportAddress, bytes);
    }

  
    private void SendData(ServerAddress address, byte[] data,CancellationTokenSource? tokenSrc=null)
    {
        //CancellationToken token = (tokenSrc?.Token)??default;
        if (address.Domain != null)
            Udp.Send(data,address.Domain,address.Port);
        else if (address != null)
        {
            IPEndPoint endpoint = new(address.Address, address.Port);
            (Udp as UdpClient).Send(data,endpoint);
        }
        else
            throw new Exception("Куда таки отправлять stun-запрос?");
    }
    public async Task<bool> CanConnectWith(ServerAddress address)
    {
        STUNMessage msg = new(Header.STUNClass.Request);
        byte[] bytes = msg.ToBytes();
        IPEndPoint? source = null;
        try
        {
            using (CancellationTokenSource src = new(500))
                SendData(address, bytes, src);
            var data = Udp.Receive(500, ref source);
            return data.Length > 0;
        }
        catch
        {
            return false;
        }
    }
    public void Dispose()
    {
        Udp.Close();
        Udp.Dispose();
    }
}

