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
    public List<GameObject> playerTilePrefabs;
    public List<GameObject> spaceshipPrefabs;
    int allPlayersLimit = 5;
    int startSpaceshipsNumber = 10;
    string IndexesAI, IndexesReg;
    public PlayerList PlayerList;
    public List<PlayerSeat> seats;

    public void StartGame()
    {
        Server.playerTilePrefabs = new List<GameObject>();

        foreach(var prefab in playerTilePrefabs)
            Server.playerTilePrefabs.Add(prefab);
        
        //Communication.loadOnStart = true;
        //Debug.Log("StartGame; " + NetworkManager.Singleton.IsServer);
        SetMapDataClientRpc(Communication.mapDataNumber,Communication.loadOnStart);
        string name = "Scenes/Main Game";
        Debug.Log($"[ChooseMapMenu.StartGame] {NetworkManager != null} {NetworkManager.SceneManager != null}");
        var status = NetworkManager.SceneManager.LoadScene(name, LoadSceneMode.Single);

        SetClientIdClientRpc();
        LobbyAndRelay lobby = GameObject.Find("LobbyAndRelay").GetComponent<LobbyAndRelay>();
        PlayerList = GameObject.Find("PlayerList").GetComponent<PlayerList>();
        seats = PlayerList.seats;
        IndexesAI = PlayerList.IndexesAI;
        IndexesReg = PlayerList.IndexesReg;
        AdjustPositions();

        //int aiPlayersNum = allPlayersLimit - lobby.maxPlayers;
        int aiPlayersNum = IndexesAI.Length;
        //int nonAiPlayersNum = lobby.joinedLobby.Players.Count;
        int nonAiPlayersNum = IndexesReg.Length;

        seats = PlayerList.seats;

        Debug.Log($"[StarGame] AI:{aiPlayersNum} Reg:{nonAiPlayersNum}");
        InitializePlayersListsClientRpc(aiPlayersNum, nonAiPlayersNum);

        Server.connectedPlayersCount = nonAiPlayersNum;
            
        Debug.Log("[StartGame] Print players from host");
        lobby.PrintPlayers(lobby.joinedLobby);
        var lobbyplayers = lobby.joinedLobby.Players;
        if (!Communication.loadOnStart)
        {
            var clients = NetworkManager.Singleton.ConnectedClientsList;

            int clientID;
            int iAI = 0, iRe = 0;
            int n = IndexesReg.Length + IndexesAI.Length;
            string nick = "";
            for (int pos = 0; pos<n; pos++)
            {
                int position = int.Parse(pos.ToString());
                if (IndexesAI.Contains(pos.ToString()))
                {
                    nick = "AIPlayer" + iAI.ToString();
                    if (iAI < PlayerGameData.AINames.Count) nick = PlayerGameData.AINames[iAI];
                    AddAiPlayerClientRpc(nick, position, iAI++);
                }
                else if (IndexesReg.Contains(pos.ToString()))
                {
                    if (iRe == clients.Count)
                    {
                        Debug.Log($"[StartGame] {pos}th PlayerSeat ({iRe}/{nonAiPlayersNum} Regular), there's {clients.Count} clients. ");
                        break;
                    }
                    position = int.Parse(pos.ToString());
                    clientID = (int)clients[iRe].ClientId;
                    nick = "Gracz";
                    if (lobbyplayers[iRe].Data != null) nick = lobbyplayers[iRe++].Data["UserName"].Value;
                    AddRealPlayerClientRpc(nick, position, clientID);
                }
            }

            Debug.Log("PlayerIDs");
            if (Server.allPlayersInfo != null)
            {
                foreach (var player in Server.allPlayersInfo)
                    Debug.Log(player.Id);
                Debug.Log("end PlayerIDs");
            }
            SetClientNamesClientRpc();

            FirstTurn();
        }
        
    }
    

    public string GetPlayeListNick(int charOldIndex, string indexes)
    {
        string result = "Gracz";
        int seatIndex = -1;
        PlayerSeat player;

        if (charOldIndex<indexes.Length) seatIndex = int.Parse(indexes[charOldIndex].ToString());
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
        for (int i=0; i<types.Length; i++)
        {
            if (i+1<types.Length && types[i]==' ')
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
    public void AddRealPlayerClientRpc(string name, int position, int id)
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
        };
        Server.allPlayersInfo.Add(playerState);
    }

    [ClientRpc]
    public void AddAiPlayerClientRpc(string nick, int position, int id)
    {
        /**
        PlayerInfo playerState = new PlayerInfo
        {
            Position = position,
            Points = 0,
            Name = "AIplayer" + id.ToString(),
            Id = id,
            IsAI = true,
            SpaceshipsLeft = Board.startSpaceshipsNumber,
            ColorNum= position,
        };
        Server.allPlayersInfo.Add(playerState);
        Server.artificialPlayers.Add(new ArtificialPlayer
        {
            Name = "AIplayer" + position.ToString(),
            Id = id,
        });
        */

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
        Server.artificialPlayers.Add(new ArtificialPlayer
        {
            Name = nick,
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
        LobbyAndRelay lobby = GameObject.Find("LobbyAndRelay").GetComponent<LobbyAndRelay>();
        Debug.Log("[SetMapDataClientRpc] Print players from host");
        lobby.PrintPlayers(lobby.joinedLobby);

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

    void FirstTurn()
    {
        if (Server.allPlayersInfo != null)
        {
            PlayerInfo nextPlayer = Server.allPlayersInfo.Where(p => p.Position == 0).First();
            if (nextPlayer.IsAI)
            { } // gameManager zaczyna tur� gracza AI
            else
                FirstTurnClientRpc(nextPlayer.Id);
        }
    }

    [ClientRpc]
    void FirstTurnClientRpc(int id)
    {
        if (PlayerGameData.Id == id)
            PlayerGameData.StartTurn();
    }
}
