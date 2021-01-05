using UnityEngine;
using Mirror;

public class NetworkRoomPlayerLobby : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private string playerName;
    public string playerReady;
    public int lobbyPlace;

    [SyncVar(hook = nameof(HnadleDisplayNameChanged))]
    public string DisplayName = "Loading...";

    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady = false;

    private bool isLeader;
    public bool IsLeader
    {
        set {
            isLeader = value;
        }
    }

    private NetworkManagerLobby room;
    private NetworkManagerLobby Room
    {
        get {
            if(room != null) {return room;}
            return room = NetworkManager.singleton as NetworkManagerLobby;
        }
    }

    public override void OnStartAuthority()
    {
        this.DisplayName = PlayerNameInput.DisplayName;
    }

    public override void OnStartClient()
    {
        this.DisplayName = PlayerNameInput.DisplayName;
    }

    public override void OnStopClient()
    {
        this.DisplayName = PlayerNameInput.DisplayName;
        Room.RoomPlayers.Remove(this);
    }

    public void HnadleDisplayNameChanged(string oldValue, string newValue) => Room.UpdateDisplayNames(this);
    public void HandleReadyStatusChanged(bool oldValue, bool newValue) => CmdUpdateReadyStatus(!IsReady);

    [Command]
    public void CmdStartGame()
    {
        if(Room.RoomPlayers[0].connectionToClient != connectionToClient) { return; }

        // Start Game
    }
    
    
    public void CmdUpdateReadyStatus(bool ReadyStatus)
    {
        IsReady = ReadyStatus;
        UpdateReadyStatus();
    }
    public void UpdateReadyStatus() {
        foreach(var player in Room.RoomPlayers)
        {
            if(player == this)
            {
                Room.UpdateReadyStatus(this);
                player.playerReady = player.IsReady ?
                    "<color=green>Ready!</color>" :
                    "<color=red>Not Ready</color>";
                return;
            }
        }
    }
}
