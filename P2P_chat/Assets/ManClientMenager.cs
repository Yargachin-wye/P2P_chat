using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TMPro;

public class ManClientMenager : MonoBehaviour
{
    [Header("Input for start")]
    [SerializeField] private TMP_InputField _ipInputField;
    [SerializeField] private TMP_InputField _portInputField;
    [Header("Messaging")]
    [SerializeField] private TextMeshProUGUI _messagerTextMP;
    [SerializeField] private TMP_InputField _outMessageText;
    [Header("UI")]
    [SerializeField] private GameObject _sendMessageButton;
    [SerializeField] private GameObject _conectButton;
    [SerializeField] private GameObject _restartButton;

    public static ManClientMenager instance;
    bool client = false;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("MainManeger not allone");
        }
        instance = this;

        _messagerTextMP.text = " ";
    }
    private void OnApplicationQuit()
    {
        if (!client)
            return;
        Debug.Log("Quit");
        Client.DisConnectFromTracker();
    }
    public void StartClient()
    {
        client = true;
        _messagerTextMP.text = "";
        _sendMessageButton.SetActive(false);
        _restartButton.SetActive(true);
        _conectButton.SetActive(true);
    }
    public void StartMessager()
    {
        IPAddress TrackerIp;
        if (!IPAddress.TryParse(_ipInputField.text, out TrackerIp))
        {
            _messagerTextMP.text = "Wrong IP";
            return;
        }

        IPAddress myIp;
        IPAddress.TryParse(GetLocalIPAddress(), out myIp);
        _messagerTextMP.text = myIp.ToString();

        Client.StartReceive(myIp, TrackerIp, int.Parse(_portInputField.text));
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
