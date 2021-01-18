using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Mirror;

[System.Serializable]
public class Match 
{
    public string matchID = string.Empty;

    public bool isPublic = false;
    public bool isMatchInLobby = true;
    public bool isMatchfull = false;
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

    public bool HostGame(string _matchID, GameObject _player, bool isPublic, out int playerIndex)
    {
        playerIndex = -1;
        if(!matchIDs.Contains(_matchID))
        {
            matchIDs.Add(_matchID);
            Match match = new Match(_matchID, _player);
            match.isPublic = isPublic;
            match.isMatchInLobby = true;
            matches.Add(match);
            playerIndex = 1;
            return true;
        } 
        else 
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
                    matches[i].isMatchfull = (matches[i].players.Count == 12);
                    break;
                }
            }
            return true;
        } 
        else 
        {
            Debug.LogError($"Match ID {_matchID} does not exist");
            return false;
        }
    }

    public bool SearchGame(GameObject _player, out int playerIndex, out string matchID)
    {
        playerIndex = -1;
        matchID = string.Empty;

        for (int i = 0; i < matches.Count; i++)
        {
            if(matches[i].isPublic && !matches[i].isMatchfull && matches[i].isMatchInLobby)
            {
                matchID = matches[i].matchID;
                if(JoinGame(matchID, _player, out playerIndex))
                {
                    return true;
                }
            }
        }

        return false;
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
                matches[i].isMatchInLobby = false;
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

    public void PlayerDisconnect(Player player, string _matchID)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            if(matches[i].matchID == _matchID)
            {
                int playerIndex = matches[i].players.IndexOf(player.gameObject);
                matches[i].players.RemoveAt(playerIndex);
                Debug.Log($"Player Disconnected from match ID: {_matchID} | {matches[i].players.Count}");

                if(matches[i].players.Count == 0)
                {
                    Debug.Log($"<color=yellow>No more player in matchID {_matchID}</color>");
                    matches.RemoveAt(i);
                    matchIDs.Remove(_matchID);
                }

                break;
            }
        }
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
