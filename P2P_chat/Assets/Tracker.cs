using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using TMPro;
using System.Threading;
using System.Text;
using Newtonsoft.Json;

public class Tracker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textInfoForConect;
    [SerializeField] private TextMeshProUGUI _textInfoTextClients;

    private static TextMeshProUGUI textInfo;
    private static TextMeshProUGUI textClients;

    private static List<Thread> _threads = new List<Thread>();
    private static LinkedList<ClientTracker> _clients = new LinkedList<ClientTracker>();

    private void Awake()
    {
        textInfo = _textInfoForConect;
        textClients = _textInfoTextClients;
        textInfo.text = "";
        textClients.text = "";
    }
    public static void StartReceive()
    {
        textInfo.text = "";
        ThreadStart threadStart = new ThreadStart(ReceiveMessage);
        Thread thread = new Thread(threadStart);
        thread.Start();
        _threads.Add(thread);
    }
    public static void ReceiveMessage()
    {
        string ipAddress = GetLocalIPAddress();
        TcpListener tcpListener = new TcpListener(IPAddress.Parse(ipAddress), 0);
        tcpListener.Start();


        int port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            ShowTextInfo(ipAddress);
            ShowTextInfo(port.ToString());
        });

        TcpClient tcpClient = tcpListener.AcceptTcpClient();

        byte[] buffer = new byte[2048];
        string json;
        string ipClient = " ";
        while (true)
        {
            NetworkStream networkStream = tcpClient.GetStream();

            if ((int)networkStream.Read(buffer, 0, buffer.Length) == 0)
            {
                ShowTextClients(ipClient + " disconnected");
                tcpClient.Close();
                tcpListener.Stop();
                _threads.Remove(Thread.CurrentThread);
                return;
            }
            json = Encoding.UTF8.GetString(buffer);
            JTrackerData jd = JsonConvert.DeserializeObject<JTrackerData>(json);

            switch (jd.tipeData)
            {
                case TipeJData.Connect:
                    ShowTextClients(jd.ip1 + " connected");

                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        StartReceive();
                    });

                    AddClient(jd.ip1, jd.port1, buffer, networkStream);
                    ipClient = jd.ip1;

                    break;
                default:
                    ShowTextClients("wrong TipeJData");
                    break;
            }
        }
        tcpClient.Close();
        tcpListener.Stop();
    }
    private static void AddClient(string ip, int port, byte[] buffer, NetworkStream networkStream)
    {
        float x = 0, y = 0;
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            x = Random.Range(-200f, 200f);
            y = Random.Range(-200f, 200f);
        });


        JTrackerData jd = new JTrackerData(TipeJData.Connect);

        buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jd));
        networkStream.Write(buffer, 0, buffer.Length);

        if (_clients.Count < 1)
        {
            _clients.AddLast(new ClientTracker(ip, port, x, y, networkStream));

            jd = new JTrackerData(TipeJData.SetPosition, null, 0, null, 0);
            buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jd));
            networkStream.Write(buffer, 0, buffer.Length);

            return;
        }
        else if (_clients.Count < 2)
        {
            _clients.AddLast(new ClientTracker(ip, port, x, y, networkStream));

            jd = new JTrackerData(TipeJData.SetPosition, _clients.First.Value.ip, _clients.First.Value.port, null, 0);
            buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jd));
            networkStream.Write(buffer, 0, buffer.Length);

            jd = new JTrackerData(TipeJData.SetPosition, ip, port, null, 0);
            buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jd));
            _clients.First.Value.networkStream.Write(buffer, 0, buffer.Length);
        }
        else if (_clients.Count < 3)
        {
            _clients.AddLast(new ClientTracker(ip, port, x, y, networkStream));

            jd = new JTrackerData(TipeJData.SetPosition,
                _clients.First.Next.Value.ip,
                _clients.First.Next.Value.port,
                _clients.First.Value.ip,
                _clients.First.Value.port);
            buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jd));
            networkStream.Write(buffer, 0, buffer.Length);

            jd = new JTrackerData(TipeJData.SetPosition,
                ip,
                port,
                _clients.First.Next.Value.ip,
                _clients.First.Next.Value.port);
            buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jd));
            _clients.First.Value.networkStream.Write(buffer, 0, buffer.Length);

            jd = new JTrackerData(TipeJData.SetPosition,
                _clients.First.Value.ip,
                _clients.First.Value.port,
                ip,
                port);
            buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jd));
            _clients.First.Next.Value.networkStream.Write(buffer, 0, buffer.Length);
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
    private static void ShowTextClients(string str)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            textClients.text += str + "\n";
        });
    }
    private static void ShowTextInfo(string str)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            textInfo.text += str + "\n";
        });
    }
}
public class ClientTracker
{
    public string ip;
    public int port;
    public float x;
    public float y;
    public NetworkStream networkStream;
    public ClientTracker(string Ip, int Port, float X, float Y, NetworkStream NetworkStream)
    {
        ip = Ip;
        port = Port;
        x = X;
        y = Y;
        networkStream = NetworkStream;
    }
}