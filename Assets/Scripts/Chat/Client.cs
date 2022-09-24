using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour
{
    public delegate void OnMessageReceive(object message);
    public event OnMessageReceive onMessageReceive;
    private const int MAX_CONNECTION = 10;
    private int port = 0;
    private int serverPort = 8888;
    private int hostID;
    private int reliableChannel;

    private int connectionID;
    private bool isConnected = false;
    private byte error;

    [SerializeField] private TMP_Text _statusText;
    public void Connect()
    {
        if (isConnected == false && Server.IsStarted == true)
        {

            NetworkTransport.Init();
            ConnectionConfig cc = new ConnectionConfig();
            reliableChannel = cc.AddChannel(QosType.Reliable);
            HostTopology topology = new HostTopology(cc, MAX_CONNECTION);
            hostID = NetworkTransport.AddHost(topology, port);
            connectionID = NetworkTransport.Connect(hostID, "127.0.0.1", serverPort, 0, out error);
            if ((NetworkError)error == NetworkError.Ok)
            {
                isConnected = true;
                AddStatusClient("Удачное подключаение к серверу");
            }
            else
            {
                AddStatusClient(((NetworkError)error).ToString());
            }
        }
        else if(isConnected == true)
            AddStatusClient("Уже подключены к серверу");
        else
            AddStatusClient("Сервер не включен");


    }

    private void AddStatusClient(string mess)
    {
        _statusText.text = mess;
    }


    public void Disconnect()
    {
        if (isConnected == true)
        {
            AddStatusClient("Успешное отключение");
            NetworkTransport.Disconnect(hostID, connectionID, out error);
            isConnected = false;
        }
        else
        {
            AddStatusClient("Вы не подключены еще к серверу");
            return;
        }
    }
    void Update()
    {
        if (!isConnected) return;
        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out
        channelId, recBuffer, bufferSize, out dataSize, out error);
        //while (recData != NetworkEventType.Nothing)
        //{
            switch (recData)
            {
                case NetworkEventType.Nothing:
                    break;

                case NetworkEventType.ConnectEvent:
                    onMessageReceive?.Invoke($"You have been connected to server.");
                    Debug.Log($"You have been connected to server.");
                    break;
                case NetworkEventType.DataEvent:
                    string message = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    onMessageReceive?.Invoke(message);
                    Debug.Log(message);
                    break;
                case NetworkEventType.DisconnectEvent:
                    isConnected = false;
                    onMessageReceive?.Invoke($"You have been disconnected from server.");
                    Debug.Log($"You have been disconnected from server.");
                    break;
                case NetworkEventType.BroadcastEvent:
                    break;
            }
            //recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer,
            //bufferSize, out dataSize, out error);

        //}

    }

    public void SendMessage(string message)
    {
        byte[] buffer = Encoding.Unicode.GetBytes(message);
        NetworkTransport.Send(hostID, connectionID, reliableChannel, buffer, message.Length *
        sizeof(char), out error);
        if ((NetworkError)error != NetworkError.Ok) Debug.Log((NetworkError)error);
    }
}