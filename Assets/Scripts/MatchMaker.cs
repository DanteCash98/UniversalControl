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
    public SyncListString matchIDs = new SyncListString();

    private void Start()
    {
        instance = this;
    }

    public bool HostGame(string _matchID, GameObject _player)
    {
        if(!matchIDs.Contains(_matchID))
        {
            matchIDs.Add(_matchID);
            matches.Add( new Match(_matchID, _player) );
            return true;
        } else 
        {
            Debug.LogError($"Match ID {_matchID} alreadyExists");
            return false;
        }
    }

    public bool JoinGame(string _matchID, GameObject _player)
    {
        if(matchIDs.Contains(_matchID))
        {
            for (int i = 0; i < matches.Count; i++)
            {
                if(matches[i].matchID == _matchID)
                {
                    matches[i].players.Add(_player);
                    break;
                }
            }
            return true;
        } else 
        {
            Debug.LogError($"Match ID {_matchID} does not Exists");
            return false;
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
