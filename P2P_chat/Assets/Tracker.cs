using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using TMPro;
using System.Threading;
using System.Text;

public class Tracker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textInfo;
    private static TextMeshProUGUI textInfo;

    private void Awake()
    {
        textInfo = _textInfo;
        textInfo.text = "";
    }

    public static void StartReceive()
    {
        ThreadStart threadStart = new ThreadStart(ReceiveMessage);
        Thread thread = new Thread(threadStart);
        thread.Start();
    }

    public static void ReceiveMessage()
    {
        string ipAddress = GetLocalIPAddress();
        TcpListener tcpListener = new TcpListener(IPAddress.Parse(ipAddress), 0);
        tcpListener.Start();

        int port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            ShowText(ipAddress);
            ShowText(port.ToString());
        });

        TcpClient tcpClient = tcpListener.AcceptTcpClient();
        NetworkStream networkStream = tcpClient.GetStream();

        byte[] buffer = new byte[1024];
        int bytesRead;
        string message;

        while (true)
        {
            networkStream.Read(buffer, 0, 1024);
            message = Encoding.UTF8.GetString(buffer);
            ShowText(message + "was conected");
            buffer = Encoding.UTF8.GetBytes("You was conected");
            networkStream.Write(buffer, 0, buffer.Length);
        }

        networkStream.Close();
        tcpClient.Close();
        tcpListener.Stop();
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
    private static void ShowText(string str)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            textInfo.text += str + "\n";
        });
    }
}