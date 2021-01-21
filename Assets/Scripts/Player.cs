using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class Player : NetworkBehaviour
{
    public static Player localPlayer;
    [SyncVar] public string matchID;
    [SyncVar] public int playerIndex;
    GameObject lobbyPlayerUI;
    GameObject lobbyPlayerChatUI;

    NetworkMatchChecker networkMatchChecker;

    private void Awake()
    {
        networkMatchChecker = GetComponent<NetworkMatchChecker>();
    }

    public override void OnStartClient()
    {
        if(isLocalPlayer)
        {
            localPlayer = this;
        }
        else
        {
            lobbyPlayerUI = UILobby.instance.SpawnPlayerUIPrefab(this);
        }
    }

    public override void OnStopClient()
    {
        Debug.Log($"Client stop");
        ClientDisconnect();
    }

    public override void OnStopServer()
    {
        Debug.Log($"Client stop on server");
        ServerDisconnect();
    }



#region HostGameCalls
    public void HostGame(bool isPublic)
    {
        string matchID = MatchMaker.instance.GetRandomMatchID();
        CmdHostGame(matchID, isPublic);

    }

    [Command]
    private void CmdHostGame(string _matchID, bool isPublic) {
        matchID = _matchID;
        if(MatchMaker.instance.HostGame(_matchID, gameObject, isPublic, out playerIndex))
        {
            Debug.Log($"Game hosted <color=green>successfully</color>");

            networkMatchChecker.matchId = _matchID.ToGuid();
            TargetHostGame(true, _matchID, playerIndex);
        }
        else
        {
            Debug.LogWarning($"Game hosted <color=red>unsuccessfully</color>");
            TargetHostGame(false, _matchID, playerIndex);
        }
    }

    [TargetRpc]
    private void TargetHostGame(bool success, string _matchID, int _playerIndex)
    {
        matchID = _matchID;
        playerIndex = _playerIndex;
        Debug.Log($"match ID: {matchID} == {_matchID}");
        UILobby.instance.HostSuccess(success, matchID);
    }

    public void JoinGame(string _matchID)
    {
        CmdJoinGame(_matchID);
    }
#endregion

#region JoinGameCalls
    [Command]
    private void CmdJoinGame(string _matchID) 
    {
        matchID = _matchID;
        if(MatchMaker.instance.JoinGame(_matchID, gameObject, out playerIndex))
        {
            Debug.Log($"Game Join attempt <color=green>successfully</color>");

            networkMatchChecker.matchId = _matchID.ToGuid();
            TargetJoinGame(true, _matchID, playerIndex);
        }
        else
        {
            Debug.LogWarning($"Game join attempt <color=red>unsuccessful</color>");
            TargetJoinGame(false, _matchID, playerIndex);
        }
    }

    [TargetRpc]
    private void TargetJoinGame(bool success, string _matchID, int _playerIndex)
    {
        matchID = _matchID;
        playerIndex = _playerIndex;
        Debug.Log($"match ID: {matchID} == {_matchID}");
        UILobby.instance.JoinSuccess(success, _matchID);
    }
#endregion

#region SearchGame
    public void SearchGame()
    {
        CmdSearchGame();
    }
    [Command]
    public void CmdSearchGame()
    {
        if(MatchMaker.instance.SearchGame(gameObject, out playerIndex, out matchID))
        {
            Debug.Log($"Game found <color=green>successfully</color>");

            networkMatchChecker.matchId = matchID.ToGuid();
            TargetSearchGame(true, matchID, playerIndex);
        }
        else
        {
            Debug.LogWarning($"Game found <color=red>unsuccessful</color>");
            TargetSearchGame(false, matchID, playerIndex);
        }
    }

    [TargetRpc]
    public void TargetSearchGame(bool success, string _matchID, int _playerIndex)
    {
        playerIndex = _playerIndex;
        matchID = _matchID;
        Debug.Log($"match ID: {matchID} == {_matchID}");
        UILobby.instance.SearchSuccess(success, _matchID);
    }
#endregion

#region StartGameCalls
    public void StartGame()
    {
        CmdStartGame();
    }

    [Command]
    private void CmdStartGame() 
    {
        MatchMaker.instance.StartGame(matchID);
        Debug.Log($"<color=yellow>Game Beginning...</color>");

        TargetStartGame();
    }

    public void StartMatch()
    {
        TargetStartGame();
    }

    [TargetRpc]
    private void TargetStartGame()
    {
        gameObject.GetComponent<ChatBehaviour>().HideChatUI();
        Debug.Log($"Match with ID: {matchID} Beginnning");
        SceneManager.LoadScene(2, LoadSceneMode.Additive);
    }
#endregion

#region DisconnectGame
    public void DisconnectGame()
    {
        CmdDisconnectGame();
    }

    [Command]
    public void CmdDisconnectGame()
    {
        ServerDisconnect();
    }

    private void ServerDisconnect()
    {
        MatchMaker.instance.PlayerDisconnect(this, matchID);
        networkMatchChecker.matchId = string.Empty.ToGuid();
        RpcDisconnectGame();
    }

    [ClientRpc]
    public void RpcDisconnectGame()
    {
        ClientDisconnect();
    }

    private void ClientDisconnect()
    {
        if(lobbyPlayerUI != null)
        {
            Destroy(lobbyPlayerUI);
        }
        
        matchID = string.Empty;
        gameObject.GetComponent<ChatBehaviour>().HideChatUI();
    }
#endregion
}
