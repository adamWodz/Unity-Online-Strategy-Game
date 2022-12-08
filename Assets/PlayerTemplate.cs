using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;

public class PlayerTemplate : MonoBehaviour
{
    public Image playerImage;
    public Text playerName;
    public Player player;

    public void UpdatePlayer(Player player)
    {
        this.player = player;
        //playerName.text = player.Id;
        LobbySetup.PlayerCharacter playerCharacter =
            System.Enum.Parse<LobbySetup.PlayerCharacter>(player.Data[LobbySetup.KEY_PLAYER_CHARACTER].Value);
    }
}
