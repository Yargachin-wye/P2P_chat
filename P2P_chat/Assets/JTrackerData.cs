using System;
using System.Net;
public class JTrackerData
{
    public TipeJData tipeData { get; set; }
    public int port1 { get; set; }
    public string ip1 { get; set; }
    public int port2 { get; set; }
    public string ip2 { get; set; }
    public int end { get; set; }
    public JTrackerData(TipeJData TipeData, string Ip1 = "", int Port1 = 0, string Ip2 = "", int Port2 = 0, int End = 0)
    {
        tipeData = TipeData;

        ip1 = Ip1;
        port1 = Port1;
        ip2 = Ip2;
        port2 = Port2;

        end = End;
    }
}
public class JMessageData
{
    public string idIp { get; set; }
    public int idPort { get; set; }
    public string message { get; set; }
    public JMessageData(string IdIp, int IdPort, string Message = " ")
    {
        idIp = IdIp;
        idPort = IdPort;
        message = Message;
    }
}
public enum TipeJData
{
    Connect,
    SetPosition,
}
