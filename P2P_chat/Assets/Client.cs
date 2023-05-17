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
    private static IPEndPoint _endPoint;
    private static int _port;

    private static UdpClient _client;
    public static void StartClient(string name, string ipAddressString, int port)
    {
        _name = name;
        IPAddress ipAddress = IPAddress.Parse(ipAddressString);

        _remoteEndPoint = new IPEndPoint(ipAddress, port);
        _endPoint = new IPEndPoint(ipAddress, port);
        _port = port;

        Debug.Log("[" + ipAddress + ":" + _port + "]");

        ThreadStart threadStart = new ThreadStart(ReceiveMessage);
        Thread thread = new Thread(threadStart);
        thread.Start();
    }
    public static void SendMessage(string str)
    {
        string message = _name + ": " + str;
        UdpClient client = new UdpClient();

        // Отправка сообщения
        byte[] bytes = System.Text.Encoding.ASCII.GetBytes(message);
        client.Send(bytes, bytes.Length, _endPoint);

        // Закрытие UDP клиента
        client.Close();
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
            MainManeger.ShowMessage(receivedMessage);
        }
        client.Close();
    }
}