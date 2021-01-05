using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class UILobby : MonoBehaviour
{
    public static UILobby instance;
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private TMP_InputField joinMatchInput;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Canvas lobbyCanvas;


    private void Start()
    {
        instance = this;
    }
    public void Host()
    {
        joinMatchInput.interactable = false;
        joinButton.interactable = false;
        hostButton.interactable = false;

        Player.localPlayer.HostGame();
    }

    public void HostSuccess(bool success)
    {
        if(success)
        {
            lobbyCanvas.enabled = true;
        }
        else
        {
            joinMatchInput.interactable = true;
            joinButton.interactable = true;
            hostButton.interactable = true;
        }
    }

    public void Join()
    {
        joinMatchInput.interactable = false;
        joinButton.interactable = false;
        hostButton.interactable = false;

        Player.localPlayer.JoinGame(joinMatchInput.text);
    }
    
    public void JoinSuccess(bool success)
    {
        if(success)
        {
            lobbyCanvas.enabled = true;
        }
        else
        {
            joinMatchInput.interactable = true;
            joinButton.interactable = true;
            hostButton.interactable = true;
        }
    }
}
