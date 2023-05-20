using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Client
{
    private static IPAddress _ipAddress;
    private static IPAddress _myIpAddress;
    private static int _port;
    public static void StartReceive(IPAddress myIpAddress)
    {
        _myIpAddress = myIpAddress;
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
        MainManager.instance.ShowMessage("you - " + str);
    }
    public static void ReceiveMessage()
    {

        UdpClient udpListener = new UdpClient(0);
        

        int port = ((IPEndPoint)udpListener.Client.LocalEndPoint).Port;
        MainManager.instance.ShowConnectInfo(port);
        IPEndPoint ip = new IPEndPoint(_myIpAddress, port);
        while (true)
        {
            var result = udpListener.Receive(ref ip);
            var message = System.Text.Encoding.UTF8.GetString(result);
            MainManager.instance.ShowMessage(message);
        }
    }
}