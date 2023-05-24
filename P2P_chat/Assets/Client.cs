﻿using System;
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

    private static Dictionary<string, int> _messages = new Dictionary<string, int>();
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
    public static void SendMessage(string str)
    {
        using UdpClient sender = new();
        JMessageData jdm = new JMessageData(_myIpAddress.ToString(), str);
        byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jdm));

        sender.Send(data, data.Length, new IPEndPoint(_neighbour1Ip, _port1));

        if (_neighbour2Ip == null)
            return;

        sender.Send(data, data.Length, new IPEndPoint(_neighbour2Ip, _port2));

        ManClientMenager.instance.ShowMessage("you - " + str);
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
        byte[] buffer = new byte[2048];
        while (true)
        {
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
                    break;
                default:
                    ManClientMenager.instance.ShowMessage("wrong TipeJData");
                    break;
            }
        }
    }
    private static void SetPosition(string neighbour1Ip, int port1, string neighbour2Ip = null, int port2 = 0)
    {
        if (neighbour1Ip == null)
        {
            ManClientMenager.instance.ShowMessage("you are alone");
            return;
        }

        _neighbour1Ip = IPAddress.Parse(neighbour1Ip);
        _port1 = port1;
        ManClientMenager.instance.ShowMessage(_neighbour1Ip + "  " + _port1.ToString());

        if (neighbour2Ip == null)
            return;

        _neighbour2Ip = IPAddress.Parse(neighbour2Ip);
        _port2 = port2;
        ManClientMenager.instance.ShowMessage(_neighbour2Ip + "  " + _port2.ToString());
    }
    public static void DisConnectFromTracker()
    {
        _trackerStream.Close();
    }
}