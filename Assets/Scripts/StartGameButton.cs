using Assets.GameplayControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class StartGameButton : NetworkBehaviour
{
    public List<MapData> availableMapsData;
    int allPlayersLimit = 5;

    public void StartGame()
    {
        //Debug.Log("StartGame; " + NetworkManager.Singleton.IsServer);
        SetMapDataClientRpc(Communication.mapDataNumber);
        string name = "Scenes/Main Game";
        Debug.Log($"[ChooseMapMenu.StartGame] {NetworkManager != null} {NetworkManager.SceneManager != null}");
        var status = NetworkManager.SceneManager.LoadScene(name, LoadSceneMode.Single);

        SetClientIdClientRpc();
        LobbyAndRelay lobby = GameObject.Find("LobbyAndRelay").GetComponent<LobbyAndRelay>();
        int aiPlayersNum = allPlayersLimit - lobby.maxPlayers;
        int nonAiPlayersNum = lobby.joinedLobby.Players.Count;
        InitializePlayersListsClientRpc(aiPlayersNum, nonAiPlayersNum);
        int position = 1;
        //NetworkManager.Singleton.pla
        foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
        {
            AddToAllPlayersInfoClientRpc(position, (int)player.ClientId);
            position++;
        }
        foreach(var player in Server.artificialPlayers)
        {
            AddToAiPlayersInfoClientRpc(position, position);
            AddToAllPlayersInfoClientRpc(position, position);
            position++;
        }

        PlayerGameData.StartTurn();
    }

    [ClientRpc]
    public void InitializePlayersListsClientRpc(int aiPlayersNum, int nonAiPlayersNum)
    {
        Server.artificialPlayers = new List<Assets.GameplayControl.ArtificialPlayer>(aiPlayersNum);
        Server.allPlayersInfo = new List<PlayerPanel.PlayerInfo>(aiPlayersNum + nonAiPlayersNum);
    }

    [ClientRpc]
    public void AddToAllPlayersInfoClientRpc(int position, int id)
    {
        PlayerPanel.PlayerInfo playerState = new PlayerPanel.PlayerInfo
        {
            Position = position,
            Points = 0,
            Name = "player" + position.ToString(),
            Id = position,
            IsAI = false,
        };
        Server.allPlayersInfo.Add(playerState);
    }

    [ClientRpc]
    public void AddToAiPlayersInfoClientRpc(int position, int id)
    {
        PlayerPanel.PlayerInfo playerState = new PlayerPanel.PlayerInfo
        {
            Position = position,
            Points = 0,
            Name = "AIplayer" + position.ToString(),
            Id = position,
            IsAI = false,
        };
        Server.allPlayersInfo.Add(playerState);
        Server.artificialPlayers.Add(new ArtificialPlayer
        {
            Name = "AIplayer" + position.ToString(),
            Id = position,
        });
    }

    [ClientRpc]
    public void SetClientIdClientRpc()
    {
        PlayerGameData.Id = (int)NetworkManager.Singleton.LocalClientId;
    }

    [ClientRpc]
    public void SetMapDataClientRpc(int mapNumber)
    {
        Debug.Log("SetMapDataClientRpc");
        Map.mapData = availableMapsData[mapNumber];
        Debug.Log(Map.mapData);
    }

    [ServerRpc]
    public void SetMapDataServerRpc(int mapNumber)
    {
        Debug.Log("SetMapDataServerRpc");
        Map.mapData = availableMapsData[mapNumber];
        Debug.Log(Map.mapData);
    }
}
