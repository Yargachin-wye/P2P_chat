using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

public class ClientScript : MonoBehaviour
{
    [Header("General Attribute")]
    public TextMeshProUGUI helloText;
    public TextMeshProUGUI informationText;
    public TextMeshProUGUI notificationText;
    public TMP_InputField inputText;
    public Button sendButton;
    public string ipAddress;
    public string name;
    public int processOrder;
    public bool done;

    [Header("Online Attribute")]
    public TcpClient client;
    public StreamReader reader;
    public StreamWriter writer;

    void Start()
    {
        informationText.text = "connecting~~";
        client = new TcpClient(ipAddress, 8080);
        informationText.text = "input your name first~~";

        reader = new StreamReader(client.GetStream());
        writer = new StreamWriter(client.GetStream());

        sendButton.onClick.AddListener(SubmitMessage);
    }

    void Update()
    {
        try
        {
            if (processOrder == 0)
                informationText.text = "input your name first~~";

            if (processOrder == 1)
                informationText.text = "input your message~~";

            if (processOrder == 2)
                ReceiveMessage();

            if (done)
            {
                reader.Close();
                writer.Close();
                client.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    void ReceiveMessage()
    {
        string notif = reader.ReadLine();
        string name = reader.ReadLine();
        string message = reader.ReadLine();

        notificationText.text = notif + "\n" + name + "\n" + message;
        processOrder = 0;
    }

    void SubmitMessage()
    {
        if (inputText.text == "exit")
            done = true;
        else if (inputText.text != null)
        {
            writer.WriteLine(inputText.text);
            writer.Flush();

            if (processOrder == 0)
            {
                name = inputText.text;
                helloText.text = "Hello, " + name;
                inputText.text = string.Empty;
                processOrder = 1;
            }
            else if (processOrder == 1)
            {
                inputText.text = string.Empty;
                processOrder = 2;
            }
        }
        else
            informationText.text = "input your name first~~";
    }
}
