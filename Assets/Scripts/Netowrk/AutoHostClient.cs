using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AutoHostClient : NetworkBehaviour
{
    [SerializeField] private NetworkManager networkManager;

    private void Start()
    {
        if(!Application.isBatchMode) // Headless build
        {
            Debug.Log("-- Client Connected --");
            networkManager.StartClient();
        }
        else
        {
            Debug.Log("-- Server Starting --");
        }
    }

    public void JoinLocal()
    {
        networkManager.networkAddress = "localhost";
        networkManager.StartClient();
    }
}
