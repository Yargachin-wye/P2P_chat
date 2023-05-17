using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Client
{
    private static string _name;
    private static IPEndPoint _remoteEndPoint;
    private static int _port;

    private static UdpClient _client;
    public static void StartClient(string name, string ipAddressString, int port)
    {
        _name = name;
        IPAddress remoteAddress = IPAddress.Parse(ipAddressString);

        _remoteEndPoint = new IPEndPoint(remoteAddress, port);

        _port = port;

        Debug.Log("[" + remoteAddress + ":" + _port + "]");

        ThreadStart threadStart = new ThreadStart(ReceiveMessage);
        Thread thread = new Thread(threadStart);
        thread.Start();
    }
    public static void SendMessage(string message)
    {
        message = _name + ": " + message;
        byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
        _client.Send(data, data.Length, _remoteEndPoint);
    }

    private static void ReceiveMessage()
    {
        UdpClient client = new UdpClient(_port);
        // Ожидание сообщения
        while (true)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] receivedBytes = client.Receive(ref remoteEndPoint);
            string receivedMessage = System.Text.Encoding.ASCII.GetString(receivedBytes);
        }
        client.Close();
    }
}