using System;
using UnityEngine;
using TMPro;
using Mirror;

public class ChatBehaviour : NetworkBehaviour
{
    [SerializeField] private TMP_Text chatText = null;
    [SerializeField] private TMP_InputField inputField = null;
    [SerializeField] private Canvas chatWindow = null;

    private static Action<string> OnMessage;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            Send();
        } 
        else if(Input.GetKeyDown(KeyCode.T) && !chatWindow.enabled && !Player.localPlayer.matchID.Equals(string.Empty))
        {
            chatWindow.enabled = true;
        }
        else if(Input.GetKeyDown(KeyCode.Escape) && chatWindow.enabled)
        {
            HideChatUI();
        }
    }

    public override void OnStartAuthority()
    {
        OnMessage += HandleNewMessage;
    }

    public void HideChatUI()
    {
        chatWindow.enabled = false;
    }

    [ClientCallback]
    private void OnDestroy()
    {
        if(!hasAuthority) return;

        OnMessage -= HandleNewMessage;
    }

    private void HandleNewMessage(string message)
    {
        chatText.text += message;
    }

    [Client]
    public void Send()
    {
        if(string.IsNullOrWhiteSpace(inputField.text)) return;

        CmdSendMessage(inputField.text);

        inputField.text = string.Empty;
    }

    [Command]
    private void CmdSendMessage(string message)
    {
        // @TO-DO: Validate
        RpcHandleMessage($"[player {Player.localPlayer.playerIndex}]: {message}");
    }

    [ClientRpc]
    private void RpcHandleMessage(string message)
    {
        OnMessage?.Invoke($"\n{message}");
    }
}
