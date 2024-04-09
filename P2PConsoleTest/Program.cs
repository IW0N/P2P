using STUN;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Free2P;
using System.Data;
namespace P2PConsoleTest;
internal class Program
{
    static IPEndPoint? companion_endpoint =null;
    static void LaunchPortHolding(STUNClient client)
    {
        ReusableUdpClient udp = client.Udp;
        int length = RandomNumberGenerator.GetInt32(14000,30000);
        byte[] debug = RandomNumberGenerator.GetBytes(length);
        CancellationTokenSource src = new();
        
        Task.Run(() =>
        {
            IPEndPoint? remote=null;
            while (true)
            {
                var task1=client.GetForeignIP();
                task1.Wait();
                if (companion_endpoint!=null)
                {
                    Console.WriteLine($"Попытка подключения к {companion_endpoint}...");
                    udp.Send(debug, companion_endpoint);
                    try
                    {
                        byte[] result=udp.Receive(1500,ref remote);
                        Console.WriteLine($"Полученно: {result[0]}");
                        string msg = Encoding.Unicode.GetString(result);
                        Console.WriteLine(msg);
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Нет подключения!");
                    }
                    

                }
                Thread.Sleep(1500);
            }
        });
    }
    static byte[] GetDefaultData()
    {
        byte[] flood_data = RandomNumberGenerator.GetBytes(30720);
        byte[] test_text = Encoding.Unicode.GetBytes("hello world!");
        byte[] test_data = new byte[flood_data.Length + test_text.Length];
        flood_data.CopyTo(test_data, 0);
        test_text.CopyTo(test_data, test_text.Length);
        return test_data;
    }
    static async Task Main(string[] args)
    {
        IPEndPoint? root = null;
        CancellationTokenSource src = new();
        WaitHandle waiter = src.Token.WaitHandle;
        Regex endpoint_regex = new("^(\\d{1,3}\\.){3}\\d{1,3}:\\d{1,5}");
        const string stun_address = "stun.12voip.com";
        using STUNClient client = new(stun_address);
        IPEndPoint selfEndpoint = await client.GetForeignIP();
        ReusableUdpClient udpClient = client.Udp;
        byte[] def_data = GetDefaultData();
        LaunchPortHolding(client);
        Console.WriteLine($"Ваш endpoint: {selfEndpoint}");
        Console.Write("Введите адрес собеседника: ");
        string endpoint_str = Console.ReadLine();
        while (!endpoint_regex.IsMatch(endpoint_str))
        {
            Console.Write("Неправильный endpoint!\nВведите ещё раз: ");
            endpoint_str = Console.ReadLine();
        }
        companion_endpoint = IPEndPoint.Parse(endpoint_str);
        waiter.WaitOne();
    }
}