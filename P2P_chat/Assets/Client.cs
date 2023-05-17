using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Client
{
    private static string _name;
    private static UdpClient _client;
    private static IPEndPoint _remoteEndPoint;
    private static IPEndPoint _localEndPoint;
    public static void StartClient(string name, string ipAddressString, int port)
    {
        _name = name;
        IPAddress remoteAddress = IPAddress.Parse(ipAddressString);
        Debug.Log("["+ remoteAddress + ":"+port+"]");

        _remoteEndPoint = new IPEndPoint(remoteAddress, port);
        _localEndPoint = new IPEndPoint(IPAddress.Any, port);

        ThreadStart threadStart = new ThreadStart(Main);
        Thread thread = new Thread(threadStart);
        thread.Start();
    }
    static void Main()
    {
        
        _client = new UdpClient();
        _client.Client.Bind(_localEndPoint);

        Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
        receiveThread.Start();
    }

    public static void SendMessage(string message)
    {
        message = _name + ": " + message;
        byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
        _client.Send(data, data.Length, _remoteEndPoint);
    }

    private static void ReceiveMessage()
    {
        while (true)
        {
            byte[] data = _client.Receive(ref _localEndPoint);
            string message = System.Text.Encoding.ASCII.GetString(data);
            MainManeger.ShowMessage(message);
        }
    }
}