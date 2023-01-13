using Assets.GameplayControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class StartGameButton : NetworkBehaviour
{
    public List<MapData> availableMapsData;
    int allPlayersLimit = 5;
    int startSpaceshipsNumber = 10;
    public List<PlayerSeat> seats;

    public void StartGame()
    {
        //Communication.loadOnStart = true;
        //Debug.Log("StartGame; " + NetworkManager.Singleton.IsServer);
        SetMapDataClientRpc(Communication.mapDataNumber,Communication.loadOnStart);
        string name = "Scenes/Main Game";
        Debug.Log($"[ChooseMapMenu.StartGame] {NetworkManager != null} {NetworkManager.SceneManager != null}");
        var status = NetworkManager.SceneManager.LoadScene(name, LoadSceneMode.Single);

        SetClientIdClientRpc();
        //LobbyAndRelay lobby = GameObject.Find("LobbyAndRelay").GetComponent<LobbyAndRelay>();

        PlayerList playerList = GameObject.Find("PlayerList").GetComponent<PlayerList>();
        //int aiPlayersNum = allPlayersLimit - lobby.maxPlayers;
        int aiPlayersNum = playerList.IndexesAI.Length;
        //int nonAiPlayersNum = lobby.joinedLobby.Players.Count;
        int nonAiPlayersNum = playerList.IndexesReg.Length;

        seats = playerList.seats;

        Debug.Log($"[StarGame] AI:{aiPlayersNum} Reg:{nonAiPlayersNum}");
        InitializePlayersListsClientRpc(aiPlayersNum, nonAiPlayersNum);
            
        if (!Communication.loadOnStart)
        {
            int position = 0;
            foreach (var s in seats) AddPlayerClientRpc(position++);
            //foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
            //{
            //    AddRealPlayerClientRpc(position, (int)player.ClientId);
            //    position++;
            //}
            //for (int i = 0; i < aiPlayersNum; i++)
            //{
            //    AddAiPlayerClientRpc(position, nonAiPlayersNum + i);
            //    position++;
            //}

            //SetClientNamesClientRpc();

            PlayerGameData.StartTurn();
        }
    }

    [ClientRpc]
    public void InitializePlayersListsClientRpc(int aiPlayersNum, int nonAiPlayersNum)
    {
        if (!Communication.loadOnStart)
        {
            Server.artificialPlayers = new List<Assets.GameplayControl.ArtificialPlayer>(aiPlayersNum);
            Server.allPlayersInfo = new List<PlayerInfo>(aiPlayersNum + nonAiPlayersNum);
        }
        else
        {
            Server.artificialPlayers = new();
            Server.allPlayersInfo = new();
        }
    }

    [ClientRpc]
    public void AddPlayerClientRpc(int pos)
    {
        PlayerSeat seat = seats[pos];
        PlayerInfo playerState = new PlayerInfo
        {
            Position = pos,
            Points = 0,
            Name = seat.Nickname.text,
            Id = pos,
            IsAI = false,
            SpaceshipsLeft = Board.startSpaceshipsNumber,
        };

        Server.allPlayersInfo.Add(playerState);
        if (seat.AI)
        {
            Server.artificialPlayers.Add(new ArtificialPlayer
            {
                Name = seat.Nickname.text,
                Id = pos,
            });
        }
    }

    [ClientRpc]
    public void AddRealPlayerClientRpc(int position, int id)
    {
        PlayerInfo playerState = new PlayerInfo
        {
            Position = position,
            Points = 0,
            Name = "player" + position.ToString(),
            Id = id,
            IsAI = false,
            SpaceshipsLeft = Board.startSpaceshipsNumber,
        };
        Server.allPlayersInfo.Add(playerState);
    }

    [ClientRpc]
    public void AddAiPlayerClientRpc(int position, int id)
    {
        PlayerInfo playerState = new PlayerInfo
        {
            Position = position,
            Points = 0,
            Name = "AIplayer" + position.ToString(),
            Id = id,
            IsAI = true,
            SpaceshipsLeft = Board.startSpaceshipsNumber,
        };
        Server.allPlayersInfo.Add(playerState);
        Server.artificialPlayers.Add(new ArtificialPlayer
        {
            Name = "AIplayer" + position.ToString(),
            Id = id,
        });
    }

    [ClientRpc]
    public void SetClientIdClientRpc()
    {
        PlayerGameData.Id = (int)NetworkManager.Singleton.LocalClientId;
    }

    [ClientRpc]
    public void SetClientNamesClientRpc()
    {
        SetNameServerRpc(PlayerGameData.Id, PlayerGameData.Name);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetNameServerRpc(int playerId, string name)
    {
        SetNameClientRpc(playerId, name);
    }

    [ClientRpc]
    public void SetNameClientRpc(int playerId, string name)
    {
        var player = Server.allPlayersInfo.Where(p => p.Id == playerId).First();
        player.Name = name;
    }

    [ClientRpc]
    public void SetMapDataClientRpc(int mapNumber,bool loadOnStart)
    {
        Debug.Log("SetMapDataClientRpc");
        Map.mapData = availableMapsData[mapNumber];
        Debug.Log(Map.mapData);
        Communication.availableMapsData = new();
        Communication.availableMapsData.AddRange(availableMapsData);
        Communication.loadOnStart = loadOnStart;
    }

    [ServerRpc]
    public void SetMapDataServerRpc(int mapNumber)
    {
        Debug.Log("SetMapDataServerRpc");
        Map.mapData = availableMapsData[mapNumber];
        Debug.Log(Map.mapData);
    }
}
