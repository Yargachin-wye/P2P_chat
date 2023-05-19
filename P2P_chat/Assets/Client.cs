using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Client
{
    private static IPAddress _ipAddress;
    private static int _port;
    public static void StartReceive()
    {
        ThreadStart threadStart = new ThreadStart(ReceiveMessage);
        Thread thread = new Thread(threadStart);
        thread.Start();
    }
    public static void ConnectToIp(IPAddress ipAddress, int port)
    {
        _port = port;
        _ipAddress = ipAddress;
    }
    public static void SendMessage(string str)
    {
        using UdpClient sender = new();

        byte[] data = System.Text.Encoding.UTF8.GetBytes(str);
        sender.Send(data, data.Length, new IPEndPoint(_ipAddress, _port));
        MainManeger.instaince.ShowMessage("you - " + str);
    }
    public static void ReceiveMessage()
    {
        UdpClient udpListener = new UdpClient(0);
        IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);

        int port = ((IPEndPoint)udpListener.Client.LocalEndPoint).Port;
        MainManeger.instaince.ShowConnectInfo(port);

        while (true)
        {
            var result = udpListener.Receive(ref ip);
            var message = System.Text.Encoding.UTF8.GetString(result);
            MainManeger.instaince.ShowMessage(message);
        }
    }
}