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
    [SerializeField] public TextMeshProUGUI _inMessageText;
    [SerializeField] private TMP_InputField _ipInputField;
    [SerializeField] private TMP_InputField _portInputField;
    [SerializeField] private TMP_InputField _outMessageText;
    [SerializeField] private GameObject SendMessageButton;
    [SerializeField] private bool _isServer = false;
    private int port;

    public static MainManeger instaince;
    private void Awake()
    {
        if(instaince != null)
        {
            Debug.LogError("MainManeger not allone");
        }
        instaince = this;
        _inMessageText = _inMessageText;
        _inMessageText.text = " ";
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
    public void ShowMessage(string str)
    {
        if (str == null)
            return;

        Debug.Log(str);
        _inMessageText.text = str + _inMessageText.text;
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
        Debug.LogWarning("IP-����� �� ������.");
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
