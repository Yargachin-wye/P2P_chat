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

                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        StartReceive();
                    });

                    AddClient(jd.ip1, jd.port1, networkStream);
                    ipClient = jd.ip1;

                    break;
                default:
                    break;
            }
        }
        tcpClient.Close();
        tcpListener.Stop();
    }
    private static void AddClient(string ip, int port, NetworkStream networkStream)
    {
        float x = 0, y = 0;
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            x = Random.Range(-200f, 200f);
            y = Random.Range(-201f, 201f);
        });

        if (_clients.Count < 1)
        {
            ClientSetPosition("0", 0, "0", 0, networkStream);
            _clients.AddLast(new ClientTracker(ip, port, new Vector2(x, y), networkStream));
        }
        else if (_clients.Count < 2)
        {
            ClientSetPosition(_clients.First.Value.ip, _clients.First.Value.port, "0", 0, networkStream);
            ClientSetPosition(ip, port, "0", 0, _clients.First.Value.networkStream);
            _clients.AddLast(new ClientTracker(ip, port, new Vector2(x, y), networkStream));
        }
        else if (_clients.Count < 3)
        {
            ClientSetPosition(ip, port, _clients.First.Next.Value.ip, _clients.First.Next.Value.port, _clients.First.Value.networkStream);
            ClientSetPosition(_clients.First.Value.ip, _clients.First.Value.port, ip, port, _clients.First.Next.Value.networkStream);
            ClientSetPosition(_clients.First.Next.Value.ip, _clients.First.Next.Value.port, _clients.First.Value.ip, _clients.First.Value.port, networkStream);

            _clients.AddAfter(_clients.First.Next, new ClientTracker(ip, port, new Vector2(x, y), networkStream));
        }
        else
        {
            float d;
            float min = 6000f;
            LinkedListNode<ClientTracker> minC = _clients.First;

            for (LinkedListNode<ClientTracker> node = _clients.First.Next; node != null; node = node.Next)
            {
                d = CalculateDistanceToLine(new Vector2(x, y), node.Previous.Value.pos, node.Value.pos);
                if (min > d)
                {
                    min = d;
                    minC = node.Previous;
                }
            }
            d = CalculateDistanceToLine(new Vector2(x, y), _clients.First.Value.pos, _clients.Last.Value.pos);
            if (min > d)
            {
                min = d;
                minC = _clients.Last;
            }

            LinkedListNode<ClientTracker> minCNext;
            if (minC.Next.Value != null)
                minCNext = minC.Next;
            else
                minCNext = _clients.First;

            ClientSetPosition(ip, port, "1", 0, minCNext.Value.networkStream);

            ClientSetPosition("1", 0, ip, port, minC.Value.networkStream);

            ClientSetPosition(minC.Value.ip, minC.Value.port, minCNext.Value.ip, minCNext.Value.port, networkStream);

            _clients.AddAfter(minC, new ClientTracker(ip, port, new Vector2(x, y), networkStream));
        }
        ShowTextClients();
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
    private static void ShowTextClients()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            textClients.text = " ";
            foreach (var client in _clients)
            {
                textClients.text += client.ip + ":" + client.port.ToString() + "(" + client.pos.x.ToString() + "; " + client.pos.x.ToString() + ")\n";
            }
        });
    }
    private static void ShowTextInfo(string str)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            textInfo.text += str + "\n";
        });
    }
    private static void ClientSetPosition(string ip1, int port1, string ip2, int port2, NetworkStream networkStream)
    {
        //ip1 1 = do not touch; 0 = delete
        byte[] buffer = new byte[2048];
        JTrackerData jd = new JTrackerData(TipeJData.SetPosition, ip1, port1, ip2, port2);
        buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jd));
        networkStream.Write(buffer, 0, buffer.Length);
    }
    private static float CalculateDistanceToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 lineDirection = lineEnd - lineStart;
        float lineLength = lineDirection.magnitude;
        Vector2 normalizedLineDirection = lineDirection / lineLength;

        float distance = Vector3.Dot((point - lineStart), normalizedLineDirection);

        distance = Mathf.Clamp(distance, 0f, lineLength);

        Vector2 closestPoint = lineStart + normalizedLineDirection * distance;

        float distanceToLine = Vector2.Distance(point, closestPoint);

        return distanceToLine;
    }
}
public class ClientTracker
{
    public string ip;
    public int port;
    public Vector2 pos;
    public NetworkStream networkStream;
    public ClientTracker(string Ip, int Port, Vector2 Pos, NetworkStream NetworkStream)
    {
        ip = Ip;
        port = Port;
        pos = Pos;
        networkStream = NetworkStream;
    }
}