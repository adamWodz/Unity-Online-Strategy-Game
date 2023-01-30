using Assets.GameplayControl;
using NUnit.Framework.Constraints;
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
    public List<GameObject> playerTilePrefabs;
    public List<GameObject> spaceshipPrefabs;
    string IndexesAI, IndexesReg;
    public PlayerList PlayerList;
    public List<PlayerSeat> seats;

    public void StartGame()
    {
        ClearAvailableMaps();
        if (Communication.loadOnStart)
        {
            string[] playerUnityIds = PlayerPrefs.GetString("players").Split(';');
            foreach (string id in playerUnityIds)
            {
            }
        }
        Server.playerTilePrefabs = new List<GameObject>();

        foreach (var prefab in playerTilePrefabs)
            Server.playerTilePrefabs.Add(prefab);

        SetMapDataClientRpc(Communication.mapDataNumber, Communication.loadOnStart);
        string name = "Scenes/Main Game";
        var status = NetworkManager.SceneManager.LoadScene(name, LoadSceneMode.Single);

        SetClientIdClientRpc();

        LobbyAndRelay lobby = GameObject.Find("LobbyAndRelay").GetComponent<LobbyAndRelay>();
        PlayerList = GameObject.Find("PlayerList").GetComponent<PlayerList>();
        seats = PlayerList.seats;
        IndexesAI = PlayerList.IndexesAI;
        IndexesReg = PlayerList.IndexesReg;
        AdjustPositions();

        int aiPlayersNum = IndexesAI.Length;
        int nonAiPlayersNum = IndexesReg.Length;

        seats = PlayerList.seats;

        InitializePlayersListsClientRpc(aiPlayersNum, nonAiPlayersNum);

        Server.connectedPlayersCount = nonAiPlayersNum;

        lobby.PrintPlayers(lobby.joinedLobby);
        var lobbyplayers = lobby.joinedLobby.Players;
        if (!Communication.loadOnStart)
        {
            var clients = NetworkManager.Singleton.ConnectedClientsList;

            int clientID;
            string UnityId = "";
            int iAI = 5, iRe = 0;
            int n = IndexesReg.Length + IndexesAI.Length;
            string nick = "";
            for (int pos = 0; pos < n; pos++)
            {
                int position = int.Parse(pos.ToString());
                if (IndexesAI.Contains(pos.ToString()))
                {
                    nick = "AIPlayer" + iAI.ToString();
                    if (iAI - 5 < PlayerGameData.AINames.Count) nick = PlayerGameData.AINames[iAI - 5];
                    AddAiPlayerClientRpc(nick, position, iAI++);
                }
                else if (IndexesReg.Contains(pos.ToString()))
                {
                    if (iRe == clients.Count)
                    {
                        break;
                    }
                    position = int.Parse(pos.ToString());
                    clientID = (int)clients[iRe].ClientId;
                    nick = "Gracz";

                    if (lobbyplayers[iRe].Data != null)
                    {
                        nick = lobbyplayers[iRe].Data["UserName"].Value;
                        UnityId = lobbyplayers[iRe++].Id;
                    }
                    AddRealPlayerClientRpc(nick, position, clientID, UnityId);
                }
            }

            if (Server.allPlayersInfo != null)
            {
                foreach (var player in Server.allPlayersInfo)
                    Debug.Log(player.Id);
            }

            FirstTurn();
        }

    }

    public void ClearAvailableMaps()
    {
        MapData mapData = availableMapsData[0];
        
        if (mapData.missions != null)
            foreach (Mission mission in mapData.missions)
            {
                mission.isDone = false;
            }

        if (mapData.paths != null)
            foreach (Path path in mapData.paths)
            {
                path.isBuilt = false;
                path.builtById = -1;
            }
    }

    public string GetPlayeListNick(int charOldIndex, string indexes)
    {
        string result = "Gracz";
        int seatIndex = -1;
        PlayerSeat player;

        if (charOldIndex < indexes.Length) seatIndex = int.Parse(indexes[charOldIndex].ToString());
        if (seatIndex != -1 && seatIndex < seats.Count)
        {
            player = seats[seatIndex];
            if (player.Nickname != null) result = player.Nickname.text;
        }
        return result;
    }

    public void AdjustPositions()
    {
        string spaces = new string(' ', 5);
        char[] types = spaces.ToCharArray();
        foreach (var a in IndexesAI)
        {
            int i = int.Parse(a.ToString());
            types[i] = 'a';
        }
        foreach (var r in IndexesReg)
        {
            int i = int.Parse(r.ToString());
            types[i] = 'r';
        }
        IndexesAI = "";
        IndexesReg = "";
        for (int i = 0; i < types.Length; i++)
        {
            if (i + 1 < types.Length && types[i] == ' ')
            {
                types[i] = types[i + 1];
                types[i + 1] = ' ';
            }
            switch (types[i])
            {
                case 'a':
                    IndexesAI += i.ToString();
                    break;
                case 'r':
                    IndexesReg += i.ToString();
                    break;
            }
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
    public void AddRealPlayerClientRpc(string name, int position, int id, string UnityId)
    {
        PlayerInfo playerState = new PlayerInfo
        {
            Position = position,
            Points = 0,
            Name = name,
            Id = id,
            IsAI = false,
            SpaceshipsLeft = Board.startSpaceshipsNumber,
            ColorNum = position,
            UnityId = UnityId
        };
        Server.allPlayersInfo.Add(playerState);
    }

    [ClientRpc]
    public void AddAiPlayerClientRpc(string nick, int position, int id)
    {
        PlayerInfo playerState = new PlayerInfo
        {
            Position = position,
            Points = 0,
            Name = nick,
            Id = id,
            IsAI = true,
            SpaceshipsLeft = Board.startSpaceshipsNumber,
            ColorNum = position,
        };
        Server.allPlayersInfo.Add(playerState);
        ArtificialPlayer newAI = new ArtificialPlayer
        {
            Name = nick,
            Id = id,
        };
        Server.artificialPlayers.Add(newAI);
    }

    [ClientRpc]
    public void SetClientIdClientRpc()
    {
        PlayerGameData.Id = (int)NetworkManager.Singleton.LocalClientId;
        PlayerGameData.UnityId = AuthenticationService.Instance.PlayerId;
    }
    [ClientRpc]
    public void SetMapDataClientRpc(int mapNumber, bool loadOnStart)
    {
        LobbyAndRelay lobby = GameObject.Find("LobbyAndRelay").GetComponent<LobbyAndRelay>();
        lobby.PrintPlayers(lobby.joinedLobby);

        Map.mapData = availableMapsData[mapNumber];
        Communication.availableMapsData = new();
        Communication.availableMapsData.AddRange(availableMapsData);
        Communication.loadOnStart = loadOnStart;
    }

    [ServerRpc]
    public void SetMapDataServerRpc(int mapNumber)
    {
        Map.mapData = availableMapsData[mapNumber];
    }

    void FirstTurn()
    {
        if (Server.allPlayersInfo != null)
        {
            PlayerInfo nextPlayer = Server.allPlayersInfo.Where(p => p.Position == 0).First();
            if (nextPlayer.IsAI)
            {
                var ai = Server.artificialPlayers.Where(p => p.Id ==nextPlayer.Id).FirstOrDefault();
                if(ai != null)
                    ai.StartAiTurn();
            }      
            else
                FirstTurnClientRpc(nextPlayer.Id);
        }
    }

    [ClientRpc]
    void FirstTurnClientRpc(int id)
    {
        if (PlayerGameData.Id == id)
        {
            Communication.CheckIfNewMissionIsCompleted();
            PlayerGameData.StartTurn();
        }
    }
}