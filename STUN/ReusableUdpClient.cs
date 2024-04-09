using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace STUN;
//interuptable udp cllient
public class ReusableUdpClient:UdpClient
{
    readonly UdpClient timer;
    IPEndPoint timer_endpoint;
    CancellationTokenSource timer_starter;
    CancellationTokenSource interupter=new();
    CancellationTokenSource timer_stopper = new();
    public readonly IPEndPoint self_local_endpoint;
    
    bool received_successfully = false;
    readonly ushort timer_port;
    const byte interupt_byte = 42;
 
    public ReusableUdpClient()
    {
        /*использую 1025, а не 1024 в качестве нижней границы,
         т.к если выпадет 1024 для порта таймера,
         то нижней границей портов для получения порта основного клиента
         будет 1023, а меншье 1024 порта непривилигированные процессы использовать не могут
         тоже самое справедливо и для верхней границы*/
        timer_port = (ushort)RandomNumberGenerator.GetInt32(1025,ushort.MaxValue-1);
        timer = new UdpClient(timer_port);
        timer_starter = new();
        timer_endpoint = GetLocalEndpoint(timer_port);
        self_local_endpoint = GetSelfLocalEndpoint();
        Client.Bind(self_local_endpoint);
        LaunchTimer();
    }
    static IPEndPoint GetLocalEndpoint(int port) => new(IPAddress.Parse("127.0.0.1"), port);
    IPEndPoint GetSelfLocalEndpoint()
    {
        /*
         * This code is necessary to generate port, depended from timer_port 
         * and its random would even probability distribution
         */
        int bottom_max = timer_port-1;
        int top_min = timer_port+1;
        double bottom_probability = (double)bottom_max/(ushort.MaxValue-1024);
        double random = new Random((int)DateTime.Now.Ticks).NextDouble();
        bool isBottom = random<bottom_probability;
        var (bot, top) = isBottom?(1024,bottom_max):(top_min,ushort.MaxValue);
        ushort port = (ushort)RandomNumberGenerator.GetInt32(bot,top);
        //generating ip endpoint
        return new IPEndPoint(IPAddress.Any,port);
    }
    void ListenTimer(int timeout)
    {
        interupter = new CancellationTokenSource(timeout);
        timer_starter.Cancel();
    }
    void LaunchTimer()
    {
        IPEndPoint broadcastIP = new(IPAddress.Broadcast,self_local_endpoint.Port);
        Task.Run(() =>
        {
            while (true)
            {
                timer_starter.Token.WaitHandle.WaitOne();
                interupter.Token.WaitHandle.WaitOne();
                if (!received_successfully)
                {
                    timer.Send(new byte[] { interupt_byte }, broadcastIP);
                    
                }
                timer_starter.Dispose();
                interupter.Dispose();
                timer_starter = new CancellationTokenSource();
                received_successfully = false;
            }
        }, timer_stopper.Token);
    }
    public new int Send(ReadOnlySpan<byte> data,IPEndPoint destination)
    {
        byte[] sending = new byte[data.Length+1];
        for (int i = 0; i < data.Length; i++)
            sending[i + 1] = data[i];
        return base.Send(sending, destination);
    }
    private void StopInterupterSuccefully()
    {
        if (!interupter.IsCancellationRequested)
        {
            received_successfully = true;
            interupter.Cancel();
        }
    }
    public byte[] Receive(int timeout,ref IPEndPoint? source)
    {
        ListenTimer(timeout);
        byte[] bytes=Receive(ref source);
        StopInterupterSuccefully();
        //Console.WriteLine($"Источник: {source}; Получено {bytes.Length} байт");
        if (bytes[0]!=interupt_byte)
        {
            byte[] result = new byte[bytes.Length - 1];
            for (int i = 0; i < result.Length; i++)
                result[i] = bytes[i+1];
            return result;
        }
        else
        {
            source = null;
            throw new OperationCanceledException();
        }
    }
    public new void Dispose()
    {
        timer_stopper.Cancel();
        timer_starter.Dispose();
        interupter.Dispose();
        timer.Dispose();
        base.Dispose();
    }
}
