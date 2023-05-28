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
        LinkedListNode<ClientTracker> client = null;
        while (true)
        {
            NetworkStream networkStream = tcpClient.GetStream();

            if ((int)networkStream.Read(buffer, 0, buffer.Length) == 0)
            {
                tcpClient.Close();
                tcpListener.Stop();
                RemoveClient(client);
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

                    client = AddClient(jd.ip1, jd.port1, networkStream);
                    break;
                default:
                    break;
            }
        }
        tcpClient.Close();
        tcpListener.Stop();
    }
    private static LinkedListNode<ClientTracker> AddClient(string ip, int port, NetworkStream networkStream)
    {
        LinkedListNode<ClientTracker> newClient;
        int x = 0, y = 0;
        System.Random rnd = new System.Random();
        x = rnd.Next(-201, 201);
        y = rnd.Next(-201, 201);


        if (_clients.Count < 1)
        {
            ClientSetPosition("0", 0, "0", 0, networkStream);
            newClient = _clients.AddLast(new ClientTracker(ip, port, new Vector2(x, y), networkStream));
        }
        else if (_clients.Count < 2)
        {
            ClientSetPosition(_clients.First.Value.ip, _clients.First.Value.port, "0", 0, networkStream);
            ClientSetPosition(ip, port, "0", 0, _clients.First.Value.networkStream);
            newClient = _clients.AddLast(new ClientTracker(ip, port, new Vector2(x, y), networkStream));
        }
        else if (_clients.Count < 3)
        {
            ClientSetPosition(ip, port, _clients.First.Next.Value.ip, _clients.First.Next.Value.port, _clients.First.Value.networkStream);
            ClientSetPosition(_clients.First.Value.ip, _clients.First.Value.port, ip, port, _clients.First.Next.Value.networkStream);
            ClientSetPosition(_clients.First.Next.Value.ip, _clients.First.Next.Value.port, _clients.First.Value.ip, _clients.First.Value.port, networkStream);

            newClient = _clients.AddAfter(_clients.First.Next, new ClientTracker(ip, port, new Vector2(x, y), networkStream));
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

            newClient = _clients.AddAfter(minC, new ClientTracker(ip, port, new Vector2(x, y), networkStream));
        }
        ShowTextClients();
        return newClient;
    }
    private static void RemoveClient(LinkedListNode<ClientTracker> client)
    {
        LinkedListNode<ClientTracker> next;
        LinkedListNode<ClientTracker> previous;
        if (client.Next.Value == null)
            next = _clients.First;
        else
            next = client.Next;

        if (client.Previous.Value == null)
            previous = _clients.Last;
        else
            previous = client.Previous;
        if (_clients.Count == 4)
        {
            _clients.Remove(client.Value);

            ClientSetPosition(_clients.First.Value.ip, _clients.First.Value.port, _clients.Last.Value.ip, _clients.Last.Value.port, _clients.First.Next.Value.networkStream);

            ClientSetPosition(_clients.Last.Value.ip, _clients.Last.Value.port, _clients.First.Next.Value.ip, _clients.First.Next.Value.port, _clients.First.Value.networkStream);

            ClientSetPosition(_clients.First.Next.Value.ip, _clients.First.Next.Value.port, _clients.First.Value.ip, _clients.First.Value.port, _clients.Last.Value.networkStream);
        }
        else if (_clients.Count == 3)
        {
            ClientSetPosition(previous.Value.ip, previous.Value.port, "0", 0, next.Value.networkStream);
            ClientSetPosition(next.Value.ip, next.Value.port, "0", 0, previous.Value.networkStream);
            _clients.Remove(client.Value);
        }
        else if (_clients.Count == 2)
        {

            ClientSetPosition("0", 0, "0", 0, next.Value.networkStream);
            _clients.Remove(client.Value);
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