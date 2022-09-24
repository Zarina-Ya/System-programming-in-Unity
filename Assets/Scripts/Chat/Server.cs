using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour
{
    private const int MAX_CONNECTION = 10;
    private int port = 8888;
    private int hostID;
    private int reliableChannel;
    private static bool isStarted = false;
    private byte error;
    List<int> connectionIDs = new List<int>();



    [SerializeField] private TMP_Text _statusText;

    public static bool IsStarted { get => isStarted; set => isStarted = value; }

    public void StartServer()
    {
        if (isStarted == false)
        {
            AddStatusServer("Сервер активен");
            NetworkTransport.Init();

            ConnectionConfig connectionConfig = new ConnectionConfig();
            reliableChannel = connectionConfig.AddChannel(QosType.Reliable);
            HostTopology topology = new HostTopology(connectionConfig, MAX_CONNECTION);
            hostID = NetworkTransport.AddHost(topology, port);

            isStarted = true;
        }
        else AddStatusServer("Сервер уже работает");
    }

    private void AddStatusServer(string mess)
    {
        _statusText.text = mess;
    }
    public void ShutDownServer()
    {
        if (isStarted == true)
        {
            AddStatusServer("Сервер выключен");
            NetworkTransport.RemoveHost(hostID);
            NetworkTransport.Shutdown();
            isStarted = false;
        }
        else
        {
            AddStatusServer("Сервер еще не включен");
            return; 
        }
    }

    public void SendMessage(int idConnection, string message)
    {
        byte[] data = Encoding.Unicode.GetBytes(message);
        NetworkTransport.Send(hostID, idConnection,reliableChannel, data, data.Length * sizeof(char), out error);

        if((NetworkError)error != NetworkError.Ok)
            throw new System.Exception(((NetworkError)error).ToString()); 
    }

    public void SendMessageToAllClient(string message)
    {
        foreach(int idConnection in connectionIDs)
            SendMessage(idConnection, message);
    }

    void Update()
    {
        if (isStarted == false)
            return;
        else
        {
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
                        connectionIDs.Add(connectionId);
                        SendMessageToAllClient($"Player {connectionId} has connected.");
                        Debug.Log($"Player {connectionId} has connected.");
                        break;

                    case NetworkEventType.DataEvent:
                        string message = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                        SendMessageToAllClient($"Player {connectionId}: {message}");
                        Debug.Log($"Player {connectionId}: {message}");
                        break;

                    case NetworkEventType.DisconnectEvent:
                        connectionIDs.Remove(connectionId);
                        SendMessageToAllClient($"Player {connectionId} has disconnected.");
                        Debug.Log($"Player {connectionId} has disconnected.");
                        break;

                    case NetworkEventType.BroadcastEvent:
                        break;
                }
            //    recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer,
            //    bufferSize, out dataSize, out error);
            //}
        }
    }
}
