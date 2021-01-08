using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Mirror;

[System.Serializable]
public class Match 
{
    public string matchID;
    public SyncListGameObject players = new SyncListGameObject();

    public Match(string matchID, GameObject player)
    {
        this.matchID = matchID;
        players.Add(player);
    }

    public Match() { }
}

[System.Serializable]
public class SyncListGameObject : SyncList<GameObject> { }

[System.Serializable]
public class SyncListMatch : SyncList<Match> { }

public class MatchMaker : NetworkBehaviour
{
    public static MatchMaker instance;

    public SyncListMatch matches = new SyncListMatch();
    public SyncList<string> matchIDs = new SyncList<string>();

    [SerializeField] GameObject turnManagerPrefab;

    private void Start()
    {
        instance = this;
    }

    public bool HostGame(string _matchID, GameObject _player, out int playerIndex)
    {
        playerIndex = -1;
        if(!matchIDs.Contains(_matchID))
        {
            matchIDs.Add(_matchID);
            matches.Add( new Match(_matchID, _player) );
            playerIndex = 1;
            return true;
        } else 
        {
            Debug.LogError($"Match ID {_matchID} already exists");
            return false;
        }
    }

    public bool JoinGame(string _matchID, GameObject _player, out int playerIndex)
    {
        playerIndex = -1;
        if(matchIDs.Contains(_matchID))
        {
            for (int i = 0; i < matches.Count; i++)
            {
                if(matches[i].matchID == _matchID)
                {
                    matches[i].players.Add(_player);
                    playerIndex = matches[i].players.Count;
                    break;
                }
            }
            return true;
        } else 
        {
            Debug.LogError($"Match ID {_matchID} does not exist");
            return false;
        }
    }

    public void StartGame(string _matchID)
    {
        GameObject newTurnManager = Instantiate(turnManagerPrefab);
        NetworkServer.Spawn(turnManagerPrefab);
        newTurnManager.GetComponent<NetworkMatchChecker>().matchId = _matchID.ToGuid();
        TurnManager turnManager = newTurnManager.GetComponent<TurnManager>();

        for (int i = 0; i < matches.Count; i++)
        {
            if(matches[i].matchID == _matchID)
            {
                foreach (var player in matches[i].players)
                {
                    Player _player = player.GetComponent<Player>();
                    turnManager.AddPlayer(_player);
                    _player.StartMatch();
                }
                break;
            }
        }
    }
    public string GetRandomMatchID()
    {
        string _id = string.Empty;

        for(int i = 0; i < 5; i++)
        {
            int random = UnityEngine.Random.Range(0, 36);
            if(random < 26)
            {
                _id += (char)(random + 65);
            } 
            else
            {
                _id += (random - 26).ToString();
            }
        }
        Debug.Log($"Randomly generated match ID: {_id}");

        return _id;
    }
}

public static class MatchExtensions
{
    public static Guid ToGuid(this string id)
    {
        MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
        byte[] inputBytes = Encoding.Default.GetBytes(id);
        byte[] hashBytes = provider.ComputeHash(inputBytes);

        return new Guid(hashBytes);
    }
}
