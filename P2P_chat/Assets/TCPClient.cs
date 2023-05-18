using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPClient : MonoBehaviour
{
    private static IPAddress _ipAddress;
    private static int _port;
    public static void StartClient(string ipAddressString, int port, bool isServer)
    {
        _port = port;
        _ipAddress = IPAddress.Parse(ipAddressString);

        if (isServer)
        {
            ThreadStart threadStart = new ThreadStart(ReceiveMessage);
            Thread thread = new Thread(threadStart);
            thread.Start();
        }
    }

    public static void SendMessage(string str)
    {
        using TcpClient client = new TcpClient();

        try
        {
            client.Connect(_ipAddress, _port);
            NetworkStream stream = client.GetStream();

            byte[] data = Encoding.UTF8.GetBytes(str);
            stream.Write(data, 0, data.Length);

            Debug.Log("Message sent");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to send message: " + e.Message);
        }
    }

    public static void ReceiveMessage()
    {
        TcpListener listener = null;

        try
        {
            listener = new TcpListener(_ipAddress, _port);
            listener.Start();

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(client);
            }
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException: " + e);
        }
        finally
        {
            listener?.Stop();
        }
    }

    private static void HandleClient(object clientObj)
    {
        TcpClient client = (TcpClient)clientObj;
        NetworkStream stream = client.GetStream();

        byte[] buffer = new byte[1024];
        int bytesRead;

        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Debug.Log(message);
        }

        client.Close();
    }
}
