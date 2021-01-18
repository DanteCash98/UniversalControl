using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class UILobby : MonoBehaviour
{
    public static UILobby instance;

    [Header("Host Join")]
    [SerializeField] private TMP_InputField joinMatchInput;
    [SerializeField] private List<Selectable> lobbySelectables = new List<Selectable>();
    [SerializeField] private Canvas lobbyCanvas;
    [SerializeField] private Canvas matchmakingCanvas;

    [Header("Lobby")]
    [SerializeField] Transform UIPlayerParent;
    [SerializeField] GameObject UIPlayerPrefab;
    [SerializeField] TMP_Text matchIDText;
    [SerializeField] GameObject startGameButton;
    [SerializeField] GameObject chatUIWindowPrefab;
    GameObject lobbyPlayerUI;

    private bool searching = false;

    private void Start()
    {
        instance = this;
    }
    public void HostPrivate()
    {
        joinMatchInput.interactable = false;
        lobbySelectables.ForEach(x => x.interactable = false);

        Player.localPlayer.HostGame(false);
    }

    public void HostPublic()
    {
        joinMatchInput.interactable = false;
        lobbySelectables.ForEach(x => x.interactable = false);

        Player.localPlayer.HostGame(true);
    }

    public void HostSuccess(bool success, string matchID)
    {
        if(success)
        {
            lobbyCanvas.enabled = true;
            if(lobbyPlayerUI != null)
            {
                Destroy(lobbyPlayerUI);
            }
            lobbyPlayerUI = SpawnPlayerUIPrefab(Player.localPlayer);
            matchIDText.text = matchID;
            startGameButton.SetActive(true);
        }
        else
        {
            joinMatchInput.interactable = true;
            lobbySelectables.ForEach(x => x.interactable = true);
        }
    }

    public void Join()
    {
        joinMatchInput.interactable = false;        
        lobbySelectables.ForEach(x => x.interactable = false);


        Player.localPlayer.JoinGame(joinMatchInput.text.ToUpper());
    }
    
    public void JoinSuccess(bool success, string matchID)
    {
        if(success)
        {
            lobbyCanvas.enabled = true;
            startGameButton.SetActive(false);
            if(lobbyPlayerUI != null)
            {
                Destroy(lobbyPlayerUI);
            }
            lobbyPlayerUI = SpawnPlayerUIPrefab(Player.localPlayer);
            matchIDText.text = matchID;
        }
        else
        {
            joinMatchInput.interactable = true;
            lobbySelectables.ForEach(x => x.interactable = true);
        }
    }

    public GameObject SpawnPlayerUIPrefab(Player player) 
    {
        GameObject newUIPlayer = Instantiate(UIPlayerPrefab, UIPlayerParent);
        newUIPlayer.GetComponent<UIPlayer>().SetPlayer(player);
        newUIPlayer.transform.SetSiblingIndex(player.playerIndex - 1);
        return newUIPlayer;
    }

    public void StartGame()
    {
        Player.localPlayer.StartGame();
    }

    public void SearchForGame()
    {
        Debug.Log($"<color=yellow>Searching for game...</color>");
        matchmakingCanvas.enabled = true;
        StartCoroutine(nameof(SearchingForGame));
    }

    private IEnumerator SearchingForGame()
    {
        searching = true;
        float currentTime = 1f;
        while(searching)
        {
            if(currentTime > 0)
            {
                currentTime -= Time.deltaTime;
            }
            else
            {
                currentTime = 1;
                Player.localPlayer.SearchGame();
            }
            yield return null;
        }
    }

    public void SearchSuccess(bool success, string matchID)
    {
        if(success)
        {
            matchmakingCanvas.enabled = false;
            JoinSuccess(success, matchID);
            searching = false;
        }
    }

    public void SearchCancel()
    {
        matchmakingCanvas.enabled = false;
        searching = false;
        joinMatchInput.interactable = true;
        lobbySelectables.ForEach(x => x.interactable = true);
    }

    public void DisconnectLobby()
    {
        if(lobbyPlayerUI != null)
        {
            Destroy(lobbyPlayerUI);
            Player.localPlayer.DisconnectGame();

            lobbyCanvas.enabled = false;
            lobbySelectables.ForEach(x => x.interactable = true);
            startGameButton.SetActive(false);
        }
    }
}
