using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

public class Client
{
    private static IPAddress _neighbour1Ip;
    private static int _port1;
    private static IPAddress _neighbour2Ip;
    private static int _port2;

    private static IPAddress _TrackerIp;
    private static IPAddress _myIpAddress;
    private static int _myPort;
    private static int _trackerPort;

    private static NetworkStream _trackerStream;
    private static TcpClient _trackerTcpClient;
    private static UdpClient _udpClient;
    private static bool _end = false;


    public static void StartReceive(IPAddress myIpAddress, IPAddress TrackerIp, int port)
    {
        _myIpAddress = myIpAddress;
        _TrackerIp = TrackerIp;
        _trackerPort = port;

        _trackerTcpClient = new TcpClient();
        _trackerTcpClient.Connect(_TrackerIp, _trackerPort);
        _trackerStream = _trackerTcpClient.GetStream();

        ThreadStart threadT = new ThreadStart(ReadMessageFromTracke);
        Thread thread1 = new Thread(threadT);
        thread1.Start();

        ThreadStart threadR = new ThreadStart(ReceiveMessage);
        Thread thread2 = new Thread(threadR);
        thread2.Start();

    }
    public static void SendNewMessage(string str)
    {
        using UdpClient sender1 = new();
        JMessageData jdm = new JMessageData(_myIpAddress.ToString(), _myPort, str);
        byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jdm));

        sender1.Send(data, data.Length, new IPEndPoint(_neighbour1Ip, _port1));

        sender1.Close();

        if (_neighbour2Ip == null)
            return;

        using UdpClient sender2 = new();

        sender2.Send(data, data.Length, new IPEndPoint(_neighbour2Ip, _port2));

        sender2.Close();
    }
    public static void SendMessage(string str, IPAddress ip, int port)
    {
        using UdpClient sender1 = new();
        JMessageData jdm = new JMessageData(_myIpAddress.ToString(), _myPort, str);
        byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jdm));

        sender1.Send(data, data.Length, new IPEndPoint(ip, port));

        sender1.Close();
    }
    public static void ReceiveMessage()
    {
        _udpClient = new UdpClient(0);
        int port = ((IPEndPoint)_udpClient.Client.LocalEndPoint).Port;
        _myPort = port;
        IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);

        SendJDataToTracker(new JTrackerData(TipeJData.Connect, _myIpAddress.ToString(), port));

        while (true)
        {
            var result = _udpClient.Receive(ref ip);
            string json = Encoding.UTF8.GetString(result);
            JMessageData jdm = JsonConvert.DeserializeObject<JMessageData>(json);

            if (!_end)
            {
                if (_neighbour1Ip.ToString() == jdm.idIp && _port1 == jdm.idPort)
                {
                    SendMessage(jdm.message, _neighbour2Ip, _port2);
                }
                else
                {
                    SendMessage(jdm.message, _neighbour1Ip, _port1);
                }
            }
            ManClientMenager.instance.ShowMessage(jdm.message);
        }
    }

    private static void SendJDataToTracker(JTrackerData jd)
    {
        string json = JsonConvert.SerializeObject(jd);
        byte[] data = Encoding.UTF8.GetBytes(json);
        _trackerStream.Write(data, 0, data.Length);
    }
    private static void ReadMessageFromTracke()
    {
        byte[] buffer;
        while (true)
        {
            buffer = new byte[2048];
            _trackerStream.Read(buffer, 0, buffer.Length);
            string json = Encoding.UTF8.GetString(buffer);
            JTrackerData jd = JsonConvert.DeserializeObject<JTrackerData>(json);

            switch (jd.tipeData)
            {
                case TipeJData.Connect:

                    break;
                case TipeJData.SetPosition:
                    ManClientMenager.instance.ShowMessage("You connected pos:");
                    SetPosition(jd.ip1, jd.port1, jd.ip2, jd.port2);

                    if (jd.end == 1)
                        _end = true;
                    else
                        _end = false;

                    break;
                default:
                    ManClientMenager.instance.ShowMessage("wrong TipeJData");
                    break;
            }
        }
    }
    private static void SetPosition(string neighbour1Ip, int port1, string neighbour2Ip = null, int port2 = 0)
    {
        switch (neighbour1Ip)
        {
            case "save":
                break;
            case "del":
                _neighbour1Ip = null;
                _port1 = 0;
                break;
            default:
                _neighbour1Ip = IPAddress.Parse(neighbour1Ip);
                _port1 = port1;
                ManClientMenager.instance.ShowMessage(_neighbour1Ip + "  " + _port1.ToString());
                break;
        }
        switch (neighbour2Ip)
        {
            case "save":
                break;
            case "del":
                _neighbour2Ip = null;
                _port2 = 0;
                break;
            default:
                _neighbour2Ip = IPAddress.Parse(neighbour2Ip);
                _port2 = port2;
                ManClientMenager.instance.ShowMessage(_neighbour2Ip + "  " + _port2.ToString());
                break;
        }
    }
    public static void DisConnectFromTracker()
    {
        _trackerStream.Close();
    }

}