using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using TMPro;

public class MainManeger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _ipText;
    [SerializeField] private TextMeshProUGUI _inMessageText;
    private static TextMeshProUGUI _message;
    [SerializeField] private TMP_InputField _ipInputField;
    [SerializeField] private TMP_InputField _portInputField;
    [SerializeField] private TMP_InputField _outMessageText;
    [SerializeField] private GameObject SendMessageButton;
    [SerializeField] private bool _isServer = false;
    private int port;
    private void Start()
    {
        _message = _inMessageText;
        string ipAddressString = GetLocalIPAddress();
        port = GetAvailablePort();
        _ipText.text = "my ip:\n" + ipAddressString + "\nyour free port:\n" + port;
        SendMessageButton.SetActive(false);
    }
    public void DoSomeDirtyShit()
    {
        Client.StartClient(_ipInputField.text, int.Parse(_portInputField.text), port, _isServer);
        SendMessageButton.SetActive(true);
    }
    public void SendMessage()
    {
        Client.SendMessage(_outMessageText.text);
    }
    public static void ShowMessage(string str)
    {
        _message.text = str + _message.text;
    }
    private string GetLocalIPAddress()
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
    private int GetAvailablePort()
    {
        TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
