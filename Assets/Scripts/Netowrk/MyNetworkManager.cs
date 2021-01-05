using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MyNetworkManager : NetworkManager
{
    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("- Server Started -");
    }

    public override void OnStopServer() 
    {
        base.OnStopServer();
        Debug.Log("- Server Stopped -");

    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log("- Client connected to server -");

    }

    public override void OnClientDisconnect(NetworkConnection conn) 
    {
        base.OnClientDisconnect(conn);
        Debug.Log("- Client disconnected from server -");

    }
    
}
