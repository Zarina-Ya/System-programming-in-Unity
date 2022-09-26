
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;




public class NewClient : MonoBehaviour
{
    byte m_ChannelID;
    int m_ClientSocket;
    int m_ConnectionID;
    [SerializeField] Button _sendButton;
    [SerializeField] Button _connectButton;
    [SerializeField] Button _disconnectButton;
    [SerializeField] TMPro.TMP_Text _text;

    HostTopology m_HostTopology;
    public TMPro.TMP_InputField m_InputField;
    public TMPro.TMP_InputField m_nameInputField;

    public bool m_IsConnected = false;
    string nameClient;
    string myText;

    public static Action<string> OnMessage;
    void Start()
    {
        m_IsConnected = false;
        m_ConnectionID = -1;
        ConnectionConfig config = new ConnectionConfig();
        m_ChannelID = config.AddChannel(QosType.Reliable);
        m_HostTopology = new HostTopology(config, 20);
        NetworkTransport.Init();

        m_InputField.gameObject.SetActive(m_IsConnected);
        _sendButton.gameObject.SetActive(m_IsConnected);


        _sendButton.onClick.AddListener(SendMessageField);
        _connectButton.onClick.AddListener(Connect);
        _disconnectButton.onClick.AddListener(Disconnect);
    }
    void SendMessageField()
    {
        myText = m_InputField.text;
        SendMessage(myText);
    }
    public void Connect()
    {
        nameClient  = m_nameInputField.text;
        if (!string.IsNullOrEmpty(nameClient) && m_IsConnected == false) 
        {
            byte error;
            if (m_ConnectionID == -1)
            {
                m_ClientSocket = NetworkTransport.AddHost(m_HostTopology);
                m_ConnectionID = NetworkTransport.Connect(m_ClientSocket, "127.0.0.1", NewServer.postServer, 0, out error);
                if ((NetworkError)error != NetworkError.Ok)
                {
                    OnMessage?.Invoke("Error: " + (NetworkError)error);
                }
                else
                {
                    m_IsConnected = true;
                    m_InputField.gameObject.SetActive(m_IsConnected);
                    _sendButton.gameObject.SetActive(m_IsConnected);
                    m_nameInputField.gameObject.SetActive(!m_IsConnected);
                }

            }


        }
        else 
        {
            OnMessage?.Invoke("Enter your name");
        }
        
    }

    public void Disconnect()
    {
        if (m_IsConnected)
        {
            byte error;
            NetworkTransport.Disconnect(m_ClientSocket, m_ConnectionID, out error);
            if ((NetworkError)error != NetworkError.Ok)
                OnMessage?.Invoke("Error: " + (NetworkError)error);
            else
            {
                m_IsConnected = false;
                m_ConnectionID = -1;
                m_InputField.gameObject.SetActive(m_IsConnected);
                m_nameInputField.gameObject.SetActive(!m_IsConnected);
                _sendButton.gameObject.SetActive(m_IsConnected);
            }
        }
        else
        {
            OnMessage?.Invoke("You are not connected");
        }
    }

    public void AddTextChat(string mess)
    {
        _text.text += $"{'\n'}{mess}";
    }
    void Update()
    {
        if (m_IsConnected == false) return;
        int outHostId;
        int outConnectionId;
        int outChannelId;
        byte[] buffer = new byte[1024];
        int receivedSize;
        byte error;


        NetworkEventType eventType = NetworkTransport.Receive(out outHostId, out outConnectionId, out outChannelId, buffer, buffer.Length, out receivedSize, out error);

        switch (eventType)
        {

            case NetworkEventType.ConnectEvent:
                {
                    Debug.Log($"You have been connected to server.");
                    SendMessage(nameClient);
                    break;
                }


            case NetworkEventType.DataEvent:
                {
                    Stream serializedMessage = new MemoryStream(buffer);
                    BinaryFormatter formatter = new BinaryFormatter();
                    string message = formatter.Deserialize(serializedMessage).ToString();
                    //AddTextChat(message);
                    OnMessage?.Invoke(message);
                    Debug.Log(message);
                    break;
                }

            case NetworkEventType.DisconnectEvent:
                {
                    m_IsConnected = false;
                    Debug.Log($"You have been disconnected from server.");
                    break;
                }

            case NetworkEventType.Nothing:
                break;

            default:
                Debug.LogError("Unknown network message type received: " + eventType);
                break;
        }



    }

    public void SendMessage( string textInput)
    {
        byte error;
        byte[] buffer = new byte[1024];
        Stream message = new MemoryStream(buffer);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(message, textInput);
        NetworkTransport.Send(m_ClientSocket, m_ConnectionID, m_ChannelID, buffer, (int)message.Position, out error);


        if ((NetworkError)error != NetworkError.Ok)
            Debug.Log("Message send error: " + (NetworkError)error);

    }
}