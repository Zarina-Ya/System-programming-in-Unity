using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;

public class Test : MonoBehaviour
{
    int m_ServerSocket;
    int m_ClientSocket;
    int m_ConnectionID;
    byte m_ChannelID;

    HostTopology m_HostTopology;
    bool m_ClientsActive;
    string myText;

    public Button m_ClientButton, m_ServerButton;

    public TMPro.TMP_InputField m_InputField;

    void Start()
    {
        m_ClientsActive = false;// клиент еще не подключился 
        myText = "Please Type Message Here...";

    
        ConnectionConfig config = new ConnectionConfig();
        m_ChannelID = config.AddChannel(QosType.Reliable);
        m_HostTopology = new HostTopology(config, 20);
        NetworkTransport.Init();

        
        m_ClientButton.onClick.AddListener(ClientButton);
        m_ServerButton.onClick.AddListener(ServerButton);
        m_InputField.onEndEdit.AddListener(delegate { SendMessageField(); });
    }

    
    void SendMyMessage(string textInput)
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

    void Update()
    {
      
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
                   
                    OnConnect(outHostId, outConnectionId, (NetworkError)error);
                    break;
                }

          
            case NetworkEventType.DataEvent:
                {
                  
                    OnData(outHostId, outConnectionId, outChannelId, buffer, receivedSize, (NetworkError)error);
                    break;
                }

            case NetworkEventType.Nothing:
                break;

            default:
                Debug.LogError("Unknown network message type received: " + eventType);
                break;
        }

       
        m_InputField.gameObject.SetActive(m_ClientsActive);
        if (m_ClientsActive)
        {
            m_ClientButton.gameObject.SetActive(false);
            m_ServerButton.gameObject.SetActive(false);
        }
    }

    void OnConnect(int hostID, int connectionID, NetworkError error)
    {
        Debug.Log("OnConnect(hostId = " + hostID + ", connectionId = "
            + connectionID + ", error = " + error.ToString() + ")");
        m_ClientsActive = true;
    }

   
    void OnData(int hostId, int connectionId, int channelId, byte[] data, int size, NetworkError error)
    {
        Stream serializedMessage = new MemoryStream(data);
        BinaryFormatter formatter = new BinaryFormatter();
        string message = formatter.Deserialize(serializedMessage).ToString();

        Debug.Log("OnData(hostId = " + hostId + ", connectionId = "
            + connectionId + ", channelId = " + channelId + ", data = "
            + message + ", size = " + size + ", error = " + error.ToString() + ")");

        m_InputField.text = "data = " + message;
    }

    void ClientButton()
    {
        byte error;
        m_ClientSocket = NetworkTransport.AddHost(m_HostTopology);
        m_ConnectionID = NetworkTransport.Connect(m_ClientSocket, "127.0.0.1", 54321, 0, out error);
      
        if ((NetworkError)error != NetworkError.Ok)
            Debug.Log("Error: " + (NetworkError)error);
        
    }

    void ServerButton()
    {
        byte error;
        m_ServerSocket = NetworkTransport.AddHost(m_HostTopology, 54321);
        NetworkTransport.Connect(m_ServerSocket, "127.0.0.1", 54321, 0, out error);
    }

    void SendMessageField()
    {
        myText = m_InputField.text;
        SendMyMessage(myText);
    }
}



