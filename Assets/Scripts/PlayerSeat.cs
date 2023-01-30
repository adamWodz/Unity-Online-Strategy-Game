using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;

public class PlayerSeat : MonoBehaviour
{
    public LobbyAndRelay lobby;
    public Player player;
    public Text Nickname;
    public Button PlayerType, KickPlayer;
    public Image AIPlayer, regularPlayer, joinedBackground;
    public bool AI;
    public string playerId;

    void Start()
    {
        Nickname.text = "";
        AI = false;
        lobby = GetComponent<LobbyAndRelay>();
    }

    void Update()
    {
        
    }

    public void HideIcon()
    {
        AIPlayer.gameObject.SetActive(false);
        regularPlayer.gameObject.SetActive(false);
        PlayerType.gameObject.SetActive(false);
        KickPlayer.gameObject.SetActive(false);
    }

    public void DisplayJoined(bool cankick)
    {
        joinedBackground.gameObject.SetActive(true);
        if (cankick) KickPlayer.gameObject.SetActive(true);
    }
    public void FreeSeat()
    {
        joinedBackground.gameObject.SetActive(false);
        KickPlayer.gameObject.SetActive(false);
        Nickname.text = "";
    }

    public void ChangePlayerType()
    {
        if (lobby == null)
        {
            AI = !AI;
            AIPlayer.gameObject.SetActive(AI);
            joinedBackground.gameObject.SetActive(AI);

            var list = GameObject.Find("PlayerList").GetComponent<PlayerList>();
            list.RefreshList();
            
        }
        
    }
    public void TryLeave()
    {
        var found = GameObject.Find("LobbyAndRelay");
        if (found != null) lobby = found.GetComponent<LobbyAndRelay>();
        if (lobby!=null) lobby.KickRecentlyJoined();

        lobby.RefreshPlayerList();
    }
}
