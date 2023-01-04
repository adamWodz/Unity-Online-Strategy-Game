using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class PlayerSeat : MonoBehaviour
{
    public LobbyAndRelay lobby;
    public Player player;
    public Text Nickname;
    public Button PlayerType, KickPlayer;
    public Image AIPlayer, regularPlayer;
    public bool AI;
    public string playerId;

    // Start is called before the first frame update
    void Start()
    {
        AI = false;
        lobby = GetComponent<LobbyAndRelay>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchMode()
    {
        AI = !AI;
        if (AI)
        {
            AIPlayer.gameObject.SetActive(true);
            regularPlayer.gameObject.SetActive(false);
            KickPlayer.gameObject.SetActive(false);
        }
        else
        {
            AIPlayer.gameObject.SetActive(false);
            regularPlayer.gameObject.SetActive(true);
            KickPlayer.gameObject.SetActive(true);
        }
        Debug.Log($"[PlayerSeat.SwitchMode] AI:{AI}");
    }
    public void TryLeave()
    {
        lobby.KickPlayer(playerId);
        Debug.Log($"[PlayerSeat.TryLeave] Player {playerId} getting kicked");
    }
}
