using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

public class Client
{
    private static IPAddress _ipAddress;
    private static IPAddress _TrackerIp;
    private static IPAddress _myIpAddress;
    private static int _port;

    private static NetworkStream _trackerStream;
    private static TcpClient _trackerTcpClient;
    private static UdpClient _udpClient;

    public static void StartReceive(IPAddress myIpAddress, IPAddress TrackerIp, int port)
    {
        _myIpAddress = myIpAddress;
        _TrackerIp = TrackerIp;
        _port = port;

        _trackerTcpClient = new TcpClient();
        _trackerTcpClient.Connect(_TrackerIp, _port);
        _trackerStream = _trackerTcpClient.GetStream();

        ThreadStart threadT = new ThreadStart(ReadMessageFromTracke);
        Thread thread1 = new Thread(threadT);
        thread1.Start();

        ThreadStart threadR = new ThreadStart(ReceiveMessage);
        Thread thread2 = new Thread(threadR);
        thread2.Start();

    }
    public static void SendMessage(string str)
    {
        using UdpClient sender = new();

        byte[] data = Encoding.UTF8.GetBytes(str);
        sender.Send(data, data.Length, new IPEndPoint(_ipAddress, _port));
        ManClientMenager.instance.ShowMessage("you - " + str);
    }
    public static void ReceiveMessage()
    {
        _udpClient = new UdpClient(0);
        int port = ((IPEndPoint)_udpClient.Client.LocalEndPoint).Port;
        IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
        SendMessageToTracker(_myIpAddress + ":" + port.ToString());

        while (true)
        {
            var result = _udpClient.Receive(ref ip);
            var message = Encoding.UTF8.GetString(result);
            ManClientMenager.instance.ShowMessage(message);
        }
    }

    private static void SendMessageToTracker(string str)
    {
        byte[] data = Encoding.UTF8.GetBytes(str);
        _trackerStream.Write(data, 0, data.Length);
    }
    private static void ReadMessageFromTracke()
    {
        byte[] data = new byte[1024];
        while (true)
        {
            _trackerStream.Read(data, 0, 1024);
            string message = Encoding.UTF8.GetString(data);
            ManClientMenager.instance.ShowMessage(message);
        }
    }
    private static void DisConnectFromTracker()
    {
        _trackerStream.Close();
        _trackerTcpClient.Close();
    }
}