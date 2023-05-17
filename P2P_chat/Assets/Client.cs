using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Client
{
    private static string _name;
    private static IPEndPoint _receivePoint;
    private static IPEndPoint _sendPoint;
    private static int _port1;

    private static UdpClient _client;
    public static void StartClient(string name, string ipAddressString, int port1, int port2)
    {
        _name = name;
        IPAddress ipAddress = IPAddress.Parse(ipAddressString);

        //_receivePoint = new IPEndPoint(ipAddress, port1);
        _sendPoint = new IPEndPoint(ipAddress, port2);
        _port1 = port1;

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
        client.Send(bytes, bytes.Length, _sendPoint);

        // Закрытие UDP клиента
        client.Close();
    }

    private static void ReceiveMessage()
    {
        UdpClient client = new UdpClient(_port1);
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