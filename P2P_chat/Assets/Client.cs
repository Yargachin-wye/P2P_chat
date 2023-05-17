using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Client
{
    private static string _name;
    private static IPEndPoint _sendPoint;
    private static IPAddress _ipAddress;
    private static int _port1;
    private static int _port2;
    public static void StartClient(string name, string ipAddressString, int port1, int port2)
    {
        _name = name;
        _port1 = port1;
        _ipAddress = IPAddress.Parse(ipAddressString);
        _port2 = port2;

        ThreadStart threadStart = new ThreadStart(ReceiveMessage);
        Thread thread = new Thread(threadStart);
        thread.Start();
    }
    public static void SendMessage(string str)
    {
        using UdpClient sender = new();

        byte[] data = System.Text.Encoding.UTF8.GetBytes(str);
        sender.Send(data, data.Length, new IPEndPoint(_ipAddress, _port1));
    }
    public static void ReceiveMessage()
    {
        using UdpClient receiver = new(_port2);
        IPEndPoint ip = null;
        while (true)
        {
            var result = receiver.Receive(ref ip);
            var message = System.Text.Encoding.UTF8.GetString(result);
            MainManeger.ShowMessage(message);
        }
    }
}