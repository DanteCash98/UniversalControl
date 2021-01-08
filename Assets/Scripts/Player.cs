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

    NetworkMatchChecker networkMatchChecker;

    private void Start()
    {
        networkMatchChecker = GetComponent<NetworkMatchChecker>();

        if(isLocalPlayer)
        {
            localPlayer = this;
        }
        else
        {
            UILobby.instance.SpawnPlayerUIPrefab(this);
        }
    }

#region HostGameCalls
    public void HostGame()
    {
        string matchID = MatchMaker.instance.GetRandomMatchID();
        CmdHostGame(matchID);

    }

    [Command]
    private void CmdHostGame(string _matchID) {
        matchID = _matchID;
        if(MatchMaker.instance.HostGame(_matchID, gameObject, out playerIndex))
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

#region StartGameCalls
    public void StartGame()
    {
        CmdStartGame();
    }

    [Command]
    private void CmdStartGame() 
    {
        MatchMaker.instance.StartGame(matchID);
        Debug.Log($"<color=green>Game Beginning...</color>");

        TargetStartGame();
    }

    public void StartMatch()
    {
        TargetStartGame();
    }

    [TargetRpc]
    private void TargetStartGame()
    {
        Debug.Log($"Match with ID: {matchID} Beginnning");
        SceneManager.LoadScene(2, LoadSceneMode.Additive);
    }
#endregion

}
