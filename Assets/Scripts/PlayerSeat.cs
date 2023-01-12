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

    // Start is called before the first frame update
    void Start()
    {
        Nickname.text = "";
        AI = false;
        lobby = GetComponent<LobbyAndRelay>();
    }

    // Update is called once per frame
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

    public void ChangePlayerType()
    {
        if (lobby == null)
        {
            AI = !AI;
            AIPlayer.gameObject.SetActive(AI);
            joinedBackground.gameObject.SetActive(AI);

            if (AI) Nickname.text = "@ AIplayer";
            Debug.Log($"[PlayerSeat.SwitchMode] AI:{AI}");
        }
        else Debug.Log($"[PlayerSeat.SwitchMode] Cannot edit playerType");
    }
    public void TryLeave()
    {
        lobby.KickPlayer(playerId);
        Debug.Log($"[PlayerSeat.TryLeave] Player {playerId} getting kicked");
    }
}
