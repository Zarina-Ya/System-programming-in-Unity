using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NewServer : MonoBehaviour
{
    int m_ServerSocket;
    int m_ConnectionID;
    byte m_ChannelID;
    public static int postServer = 99999;
    HostTopology m_HostTopology;

    List<int> clientsSokets = new List<int>();
    public Button m_ServerButton;

    public TMPro.TMP_Text _log;

    bool _isActiveServer = false;
    private void Start()
    {
        _isActiveServer = false;
        m_ServerButton.onClick.AddListener(StartServer);

    }

    private void InitNetworkTransport()
    {
        ConnectionConfig config = new ConnectionConfig();
        m_ChannelID = config.AddChannel(QosType.Reliable);
        m_HostTopology = new HostTopology(config, 20);
        NetworkTransport.Init();
    }

    public void StartServer()
    {
        if (_isActiveServer == false)
        {
            InitNetworkTransport();
            AddToLogMess("Server is on");
            byte error;
            m_ServerSocket = NetworkTransport.AddHost(m_HostTopology, postServer);
            NetworkTransport.Connect(m_ServerSocket, "127.0.0.1", postServer, 0, out error);


            m_ServerButton.GetComponentInChildren<TMPro.TMP_Text>().text = "ShutDownServer";
            m_ServerButton.onClick.RemoveAllListeners();
            m_ServerButton.onClick.AddListener(ShutDownServer);

            _isActiveServer = true;

        }
        else AddToLogMess("Server is Active");

    }

    public void ShutDownServer()
    {
        if (_isActiveServer == true)
        {
            NetworkTransport.RemoveHost(m_ServerSocket);
            NetworkTransport.Shutdown();

            m_ServerButton.GetComponentInChildren<TMPro.TMP_Text>().text = "Start Server";
            m_ServerButton.onClick.RemoveAllListeners();
            m_ServerButton.onClick.AddListener(StartServer);
            _isActiveServer = false;

            AddToLogMess("Server is off");
        }
        else AddToLogMess("Server is UnActive");

    }

    void Update()
    {
        if (_isActiveServer == false) return;
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
                    clientsSokets.Add(outConnectionId);
                    OnConnect(outHostId, outConnectionId, (NetworkError)error);
                    break;
                }


            case NetworkEventType.DataEvent:
                {
                    OnData(outHostId, outConnectionId, outChannelId, buffer, receivedSize, (NetworkError)error);
                    break;
                }
            case NetworkEventType.DisconnectEvent:
                {
                    clientsSokets.Remove(outConnectionId);
                    AddToLogMess($"Player {outConnectionId} has disconnected.");
                    SendMessageToAllClient($"Player {outConnectionId} has disconnected.");
                    break;
                }

            case NetworkEventType.Nothing:
                break;

            default:
                Debug.LogError("Unknown network message type received: " + eventType);
                break;
        }

    }

    void OnData(int hostId, int connectionId, int channelId, byte[] data, int size, NetworkError error)
    {
        Stream serializedMessage = new MemoryStream(data);
        BinaryFormatter formatter = new BinaryFormatter();
        string message = formatter.Deserialize(serializedMessage).ToString();

        Debug.Log("OnData(hostId = " + hostId + ", connectionId = "
            + connectionId + ", channelId = " + channelId + ", data = "
            + message + ", size = " + size + ", error = " + error.ToString() + ")");

        //m_InputField.text = "data = " + message;
        AddToLogMess($"Player {connectionId} : {message}");
        SendMessageToAllClient($"Player {connectionId} : {message}");
    }

    void OnConnect(int hostID, int connectionID, NetworkError error)
    {
        Debug.Log("OnConnect(hostId = " + hostID + ", connectionId = "
            + connectionID + ", error = " + error.ToString() + ")");
       
        AddToLogMess($"Player {connectionID} has connected.");

        SendMessageToAllClient($"Player {connectionID} has connected.");
    }


    private void AddToLogMess(string mess)
    {
        _log.text += $"{'\n'}{mess}";
    }

    public void SendMessage(int m_ClientSocket, string textInput)
    {
        byte error;
        byte[] buffer = new byte[1024];
        Stream message = new MemoryStream(buffer);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(message, textInput);
        NetworkTransport.Send(m_ConnectionID, m_ClientSocket, m_ChannelID, buffer, (int)message.Position, out error);


        if ((NetworkError)error != NetworkError.Ok)
            Debug.Log("Message send error: " + (NetworkError)error);

    }

    public void SendMessageToAllClient(string message)
    {
        foreach (int idConnection in clientsSokets)
            SendMessage(idConnection, message);
    }
}

