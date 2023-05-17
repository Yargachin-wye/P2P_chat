using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Client
{
    private static IPAddress _ipAddress;
    private static int _port1;
    private static int _port2;
    public static void StartClient(string ipAddressString, int port1, int port2, bool isServer )
    {
        _port1 = port1;
        _ipAddress = IPAddress.Parse(ipAddressString);
        _port2 = port2;

        if (isServer)
        {
            ThreadStart threadStart = new ThreadStart(ReceiveMessage);
            Thread thread = new Thread(threadStart);
            thread.Start();
        }
    }
    public static void SendMessage(string str)
    {
        using UdpClient sender = new();

        byte[] data = System.Text.Encoding.UTF8.GetBytes(str);
        sender.Send(data, data.Length, new IPEndPoint(_ipAddress, _port1));
        Debug.Log("Message sended");
    }
    public static void ReceiveMessage()
    {
        using UdpClient receiver = new(_port2);
        IPEndPoint ip = new IPEndPoint(IPAddress.Parse(GetLocalIPAddress()), _port2);
        //MainManeger.ShowMessage("start ReceiveMessage");
        Debug.Log("start ReceiveMessage");
        while (true)
        {
            var result = receiver.Receive(ref ip);
            var message = System.Text.Encoding.UTF8.GetString(result);
            //MainManeger.ShowMessage(message);
            Debug.Log(message);
        }
    }
    private static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        Debug.LogWarning("IP-адрес не найден.");
        return " ";
    }
}