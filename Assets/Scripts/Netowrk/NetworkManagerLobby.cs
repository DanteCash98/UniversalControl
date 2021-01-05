using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;
using TMPro;

public class NetworkManagerLobby : NetworkManager
{
    [SerializeField] private int minPlayers = 2;
    [SerializeField] private GameObject[] spawnPoints = new GameObject[4];
    [Scene] [SerializeField] private string menuScene = string.Empty;
    [SerializeField] private GameObject Panel_Lobby = null;

    [Header("Room")]
    [SerializeField] private NetworkRoomPlayerLobby roomPlayerPrefab = null;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;

    public List<NetworkRoomPlayerLobby> RoomPlayers { get; } = new List<NetworkRoomPlayerLobby>();

    public override void OnStartServer() => spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();

    public override void OnStartClient()
    {
        Debug.Log("- Starting client -");

        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        foreach(var prefab in spawnablePrefabs) 
        {
            ClientScene.RegisterPrefab(prefab);
        }

        if(Panel_Lobby != null)
        {
            Panel_Lobby.SetActive(true);
        }
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        Debug.Log("- Client Connecting -");

        base.OnClientConnect(conn);
        OnClientConnected?.Invoke();

    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        Debug.Log("- Client Disconnecting -");

        base.OnClientDisconnect(conn);
        OnClientDisconnected?.Invoke();

    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        if (numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if(conn.identity != null)
        {
            var player = conn.identity.GetComponent<NetworkRoomPlayerLobby>();

            spawnPoints[player.lobbyPlace].SetActive(false);

            RoomPlayers.Remove(player);
            UpdateLobbyPositions();
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnServerAddPlayer(NetworkConnection conn) 
    {
        if (SceneManager.GetActiveScene().path.Equals(menuScene))
        {
            bool isLeader;
            
            GameObject lobbyPosition = spawnPoints[RoomPlayers.Count];
            lobbyPosition.SetActive(true);

            NetworkRoomPlayerLobby roomPlayerInstance = Instantiate(roomPlayerPrefab);
            if(!NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject))
            {
                return;
            }
            roomPlayerInstance.lobbyPlace = RoomPlayers.Count;
            RoomPlayers.Add(roomPlayerInstance);
            UpdateLobbyPositions();

            isLeader = roomPlayerInstance.lobbyPlace == 0 ? true : false;
            
            Debug.Log($"Player {roomPlayerInstance.DisplayName} has entered at place {roomPlayerInstance.lobbyPlace}");
        }
    }

    public override void OnStopServer()
    {
        Debug.Log("- Stopping Server -");
        RoomPlayers.Clear();
    }

    private bool IsReadyToStart()
    {
        if(numPlayers < minPlayers) {return false;}

        foreach (var player in RoomPlayers)
        {
            if(!player.IsReady) {return false;}
        }

        return true;
    }

    private void UpdateLobbyPositions() {
        for(int i = 0; i < RoomPlayers.Count; i++)
        {
            RoomPlayers[i].lobbyPlace = i;
            UpdateDisplayNames(RoomPlayers[i]);
            UpdateReadyStatus(RoomPlayers[i]);
        }
    }

    public void UpdateDisplayNames(NetworkRoomPlayerLobby roomPlayerLobby)
    {
        spawnPoints[roomPlayerLobby.lobbyPlace].transform.GetChild(0).GetComponent<TMP_Text>().text = roomPlayerLobby.DisplayName;
    }

    public void UpdateReadyStatus(NetworkRoomPlayerLobby roomPlayerLobby)
    {
        Debug.LogWarning($"Update ready status for {roomPlayerLobby.DisplayName}; IsReady = {roomPlayerLobby.IsReady}");

        spawnPoints[roomPlayerLobby.lobbyPlace].transform.GetChild(1).GetComponent<TMP_Text>().text = roomPlayerLobby.IsReady ?
                    "<color=green>Ready!</color>" :
                    "<color=red>Not Ready</color>";
    }
}
