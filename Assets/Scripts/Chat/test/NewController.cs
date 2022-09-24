using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NewController : MonoBehaviour
{
    int m_ServerSocket;
    int m_ClientSocket;
    int m_ConnectionID;
    byte m_ChannelID;

    HostTopology m_HostTopology;
    bool m_ClientsActive;
    string myText;

    void Start()
    {

        ConnectionConfig config = new ConnectionConfig();
        m_ChannelID = config.AddChannel(QosType.Reliable);
        m_HostTopology = new HostTopology(config, 20);
        NetworkTransport.Init();

    }

    
}
