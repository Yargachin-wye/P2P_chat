using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TMPro;

public class MainManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _ipText;
    [SerializeField] private TextMeshProUGUI _messagerTextMP;
    [SerializeField] private TMP_InputField _ipInputField;
    [SerializeField] private TMP_InputField _portInputField;
    [SerializeField] private TMP_InputField _outMessageText;
    [SerializeField] private GameObject _sendMessageButton;
    [SerializeField] private GameObject _conectButton;
    [SerializeField] private GameObject _restartButton;
    [SerializeField] private bool _isServer = false;
    private int port;

    public static MainManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("MainManeger not allone");
        }
        instance = this;

        _messagerTextMP.text = " ";
    }
    private void Start()
    {
        IPAddress myIp;
        IPAddress.TryParse(GetExternaIPAddress(), out myIp);
        _messagerTextMP.text = myIp.ToString();
        Client.StartReceive(myIp);

        _sendMessageButton.SetActive(false);
        _restartButton.SetActive(true);
        _conectButton.SetActive(true);
    }
    public void ShowConnectInfo(int port)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            _ipText.text =
            "my external ip:\t" + GetExternaIPAddress() + "\n" +
            "my local ip:\t\t" + GetLocalIPAddress() + "\n" +
            "your free port:\t" + port.ToString();
        });
    }
    public void ConnectToIp()
    {
        IPAddress ip;
        if (!IPAddress.TryParse(_ipInputField.text, out ip))
        {
            _messagerTextMP.text = "Wrong IP";
            return;
        }
        Client.ConnectToIp(ip, int.Parse(_portInputField.text));

        _sendMessageButton.SetActive(true);
        _conectButton.SetActive(false);
    }
    public void SendMessage()
    {
        Client.SendMessage(_outMessageText.text);
    }
    public void ShowMessage(string str)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            _messagerTextMP.text = _messagerTextMP.text + "\n" + str;
        });
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
    private string GetExternaIPAddress()
    {
        WebClient client = new WebClient();
        string externalIP = client.DownloadString("https://api.ipify.org");
        return externalIP;
    }

}
