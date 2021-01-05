using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    public static Player localPlayer;
    [SyncVar] public string matchID;

    NetworkMatchChecker networkMatchChecker;

    private void Start()
    {
        if(isLocalPlayer)
        {
            localPlayer = this;
        }

        networkMatchChecker = GetComponent<NetworkMatchChecker>();
    }
    public void HostGame()
    {
        string matchID = MatchMaker.instance.GetRandomMatchID();
        CmdHostGame(matchID);

    }

    [Command]
    private void CmdHostGame(string _matchID) {
        matchID = _matchID;
        if(MatchMaker.instance.HostGame(_matchID, gameObject))
        {
            Debug.Log($"Game hosted <color=green>successfully</color>");

            networkMatchChecker.matchId = _matchID.ToGuid();
            RpcHostGame(true, _matchID);
        }
        else
        {
            Debug.LogWarning($"Game hosted <color=red>unsuccessfully</color>");
            RpcHostGame(false, _matchID);
        }
    }

    [TargetRpc]
    private void RpcHostGame(bool success, string _matchID)
    {
        Debug.Log($"match ID: {matchID} == {_matchID}");
        UILobby.instance.HostSuccess(success);
    }

    public void JoinGame(string _matchID)
    {
        CmdJoinGame(_matchID);
    }

    [Command]
    private void CmdJoinGame(string _matchID) {
        matchID = _matchID;
        if(MatchMaker.instance.JoinGame(_matchID, gameObject))
        {
            Debug.Log($"Game Joined <color=green>successfully</color>");

            networkMatchChecker.matchId = _matchID.ToGuid();
            RpcJoinGame(true, _matchID);
        }
        else
        {
            Debug.LogWarning($"Game Joined <color=red>unsuccessfully</color>");
            RpcJoinGame(false, _matchID);
        }
    }

    [TargetRpc]
    private void RpcJoinGame(bool success, string _matchID)
    {
        Debug.Log($"match ID: {matchID} == {_matchID}");
        UILobby.instance.JoinSuccess(success);
    }
}
