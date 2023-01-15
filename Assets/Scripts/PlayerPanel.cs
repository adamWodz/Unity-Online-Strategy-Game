using Assets.GameplayControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UIElements;


enum PlayerColors
{
    blue,
    red,
    yellow,
    pink,
    green
}

[Serializable]
public class PlayerInfo
{
    public int Position;
    public int Points;
    public string Name;
    public int Id;
    public bool IsAI;
    public int SpaceshipsLeft;
    public int PlayerTileId;
    public int PlayerColorNumber;
    public GameObject TilePrefab;
    public GameObject SpaceshipPrefab;
    public List<Mission> missions; // uzupe³niana na koniec gry
}

[Serializable]
public struct PlayerInfoData
{
    public int Position;
    public int Points;
    public string Name;
    public int Id;
    public bool IsAI;
    public int SpaceshipsLeft;
    public int PlayerTileId;
    public int PlayerColorNumber;
}
public class PlayerInfoComparer: IComparer<PlayerInfo>
{
    public int Compare(PlayerInfo x, PlayerInfo y)
    {
        if(x == null)
        {
            if (y == null)
                return 0;
            else
                return -1;
        }
        else
        {
            if(y == null) 
                return 1;
            else
            {
                if (x.Position < y.Position)
                    return -1;
                else if (x.Position == y.Position)
                    return 0;
                else
                    return 1;
            }
        }
    }
}

public class PlayerPanel : NetworkBehaviour, IDataPersistence
{
   
    [SerializeField] public List<PlayerInfo> players;
    [SerializeField] private GameObject playerTilePrefab;
    Queue<GameObject> playersTiles;
    Dictionary<int, GameObject> playerTilesByIds = new Dictionary<int, GameObject>();
    public bool playersOrderChanged = false;
    GameManager gameManager;

    // Start is called before the first frame update
    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (Communication.loadOnStart)
        {
            //players = new();
            //Server.allPlayersInfo = new();
        }
        else
        {
            players = Server.allPlayersInfo;

            playersTiles = new();

            //GameObject playerTextTemplate = transform.GetChild(0).gameObject;
            GameObject playerTile;

            int playersCount = players.Count;


            for (int i = 0; i < playersCount; i++)
            {
                var player = players.Where(p => p.Position == i).First();

                playerTile = Instantiate(player.TilePrefab, transform);
                playerTile.transform.GetChild(0).GetComponent<TMP_Text>().text = (players[i].Position + 1).ToString();
                playerTile.transform.GetChild(1).GetComponent<TMP_Text>().text = players[i].Name;
                playerTile.transform.GetChild(2).GetComponent<TMP_Text>().text = players[i].Points.ToString();
                playerTile.transform.GetChild(3).GetComponent<TMP_Text>().text = players[i].SpaceshipsLeft.ToString();
                playersTiles.Enqueue(playerTile);

                player.PlayerTileId = playerTile.GetInstanceID();
                playerTilesByIds.Add(player.PlayerTileId, playerTile);
            }
            //Destroy(playerTextTemplate);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void LoadData(GameData data)
    {
        if (IsHost)
        {
            var artificialPlayers = data.players.Where(player => player.IsAI).ToList();
            foreach(var artificialPlayer in artificialPlayers)
            {
                ArtificialPlayer AI = new()
                {
                    Name = artificialPlayer.Name,
                    Id = artificialPlayer.Id,
                    curentPoints = artificialPlayer.Points,
                    spaceshipsLeft = artificialPlayer.SpaceshipsLeft,
                };

                for(int i = 0;i< data.cardsForEachPalyer[artificialPlayer.Id].Length;i++)
                {
                    AI.numOfCardsInColor[(Color)i] = data.cardsForEachPalyer[artificialPlayer.Id][i];
                }
                Server.artificialPlayers.Add(AI);
            }
            
            foreach(var player in data.players) 
            {
                LoadPlayersListClientRpc(player.Position, player.Points, player.Name, player.Id, player.IsAI, player.SpaceshipsLeft, player.PlayerTileId);
            }
            
            int id = data.players.Single(player => player.Position == 0).Id;
            ClientRpcParams clientRpcParams = new()
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { (ulong)id }
                }
            };
            SendStartTurnClientRpc(clientRpcParams);
        }

        //gameManager.spaceshipCounter.text = Server.allPlayersInfo.Single(p => p.Id == PlayerGameData.Id).SpaceshipsLeft.ToString();
    }

    [ClientRpc]
    void LoadPlayersListClientRpc(int position,int points,string name,int id,bool isAI, int spaceshipsLeft, int playerTileId)
    {
        PlayerInfo playerInfo = new PlayerInfo()
        {
            Position = position,
            Points = points,
            Name = name,
            Id = id,
            IsAI = isAI,
            SpaceshipsLeft = spaceshipsLeft,
            PlayerTileId = playerTileId,
            PlayerColorNumber = id,
            //SpaceshipPrefab = gameManager.shipGameObjectList[id],
            //TilePrefab = gameManager.playerTileObjectList[id],
            //missions = new()
        };
        Debug.Log(playerInfo);
        players ??= new();
        Server.allPlayersInfo ??= new();
        players.Add(playerInfo);
        Server.allPlayersInfo.Add(playerInfo);
        //Debug.Log($"Players: {players.Count}");
        //Debug.Log($"ServerPlayers: {Server.allPlayersInfo.Count}");
        //GameObject playerTextTemplate = transform.GetChild(0).gameObject;
        playersTiles ??= new();
        var playerTile = Instantiate(gameManager.playerTileObjectList[id], transform);
        playerTile.transform.GetChild(0).GetComponent<TMP_Text>().text = (playerInfo.Position + 1).ToString();
        playerTile.transform.GetChild(1).GetComponent<TMP_Text>().text = playerInfo.Name;
        playerTile.transform.GetChild(2).GetComponent<TMP_Text>().text = playerInfo.SpaceshipsLeft.ToString();
        playerTile.transform.GetChild(3).GetComponent<TMP_Text>().text = playerInfo.Points.ToString();
        playerTile.transform.SetSiblingIndex(playerInfo.Position);
        
        var player = players.Where(p => p.Position == playerInfo.Position).First();
        player.PlayerTileId = playerTile.GetInstanceID();
        playerTilesByIds ??= new();
        playerTilesByIds.Add(player.PlayerTileId, playerTile);
        playersTiles.Enqueue(playerTile);

        if (PlayerGameData.Id == playerInfo.Id)
        {
            PlayerGameData.spaceshipsLeft = playerInfo.SpaceshipsLeft;
            PlayerGameData.curentPoints = playerInfo.Points;
            if(IsHost)
                gameManager.spaceshipCounter.text = playerInfo.SpaceshipsLeft.ToString();
        }
    }

    public void SaveData(ref GameData data)
    {
        if(IsHost)
        {
            /*
            data.players = Server.allPlayersInfo;
            data.players.Sort(new PlayerInfoComparer());
            */
            data.players = new();
            Server.allPlayersInfo.Sort(new PlayerInfoComparer());
            foreach(var player in Server.allPlayersInfo)
            {
                PlayerInfoData playerInfoData = new()
                {
                    Position = player.Position,
                    Points = player.Points,
                    Id = player.Id,
                    Name = player.Name,
                    IsAI = player.IsAI,
                    SpaceshipsLeft = player.SpaceshipsLeft,
                    PlayerColorNumber = player.PlayerColorNumber,
                    PlayerTileId = player.PlayerTileId
                };
                data.players.Add(playerInfoData);
            }

        } 
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayersOrderServerRpc()
    {
        Debug.Log("UpdatePlayersOrder: "+PlayerGameData.Id);
        UpdatePlayersOrderClientRpc();
    }

    [ClientRpc]
    public void UpdatePlayersOrderClientRpc()
    {
        //Debug.Log("Tiles: " + playersTiles.Count);
        // pobieram pierwszego gracza z kolejki (tego, ktorego tura sie zakonczyla)
        var firstElement = playersTiles.Dequeue();
        //Debug.Log("Index tile:" + firstElement.transform.GetSiblingIndex());
        //Debug.Log("Index tile 2:" + secondElement.transform.GetSiblingIndex());
        // ustawiam go na koniec w panelu graczy
        firstElement.transform.SetAsLastSibling();
        //Debug.Log("Index tile:" + firstElement.transform.GetSiblingIndex());
        // dodaje go na koniec kolejki
        playersTiles.Enqueue(firstElement);

        // poprawiam pozycje wszystkich graczy
        int i = 0;
        //Debug.Log($"Players: {players.Count}");
        //Debug.Log($"ServerPlayers: {Server.allPlayersInfo.Count}");
        //Debug.Log($"playerTiles: {playersTiles.Count}");
        foreach (var playerTile in playersTiles) 
        {
            //Debug.Log($"i :{i}");
            players[i].Position = (players[i].Position - 1 + players.Count) % players.Count;
            //Debug.Log(players[i].Name);
            i++;
            playerTile.transform.GetChild(0).GetComponent<TMP_Text>().text = i.ToString();
        }

        playersOrderChanged = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartNextPlayerTurnServerRpc()
    {
        while(!playersOrderChanged) { }
        
        PlayerInfo nextPlayer = players.Where(p => p.Position == 0).First();
        //Debug.Log("StartNextPlayerTurnServerRpc; playerId: " + nextPlayer.Id);
        if (nextPlayer.IsAI)
            Communication.StartAiTurn(nextPlayer.Id);
        else
            StartNextPlayerTurnClientRpc(nextPlayer.Id);
    }

    [ClientRpc]
    public void StartNextPlayerTurnClientRpc(int playerId)
    {
        //Debug.Log("StartNextPlayerTurnClientRpc; playerId: " + playerId + " thisPlayerId: " + PlayerGameData.Id);
        Communication.StartTurn(playerId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdatePointsAndSpeceshipsNumServerRpc(int playerId, int playerPoints, int spaceshipsLeft)
    {
        UpdatePointsAndSpaceshipsNumClientRpc(playerId, playerPoints, spaceshipsLeft);
    }

    [ClientRpc]
    public void UpdatePointsAndSpaceshipsNumClientRpc(int playerId, int playerPoints, int spaceshipsLeft)
    {
        var player = players.Where(p => p.Id == playerId).First();
        player.Points = playerPoints;
        player.SpaceshipsLeft = spaceshipsLeft;
        playerTilesByIds[player.PlayerTileId].transform.GetChild(2).GetComponent<TMP_Text>().text = player.SpaceshipsLeft.ToString();
        playerTilesByIds[player.PlayerTileId].transform.GetChild(3).GetComponent<TMP_Text>().text = player.Points.ToString();
    }

    [ClientRpc]
    void SendStartTurnClientRpc(ClientRpcParams clientRpcParams = default)
    {
        PlayerGameData.StartTurn();
    }
}
