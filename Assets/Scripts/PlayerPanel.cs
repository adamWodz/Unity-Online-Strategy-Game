using Assets.GameplayControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

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
}

public class PlayerPanel : NetworkBehaviour, IDataPersistence
{
   
    [SerializeField] public List<PlayerInfo> players;
    [SerializeField] private GameObject playerTilePrefab;
    Queue<GameObject> playersTiles;
    Dictionary<int, GameObject> playerTilesByIds = new Dictionary<int, GameObject>();
    public bool playersOrderChanged = false;


    // Start is called before the first frame update
    void Start()
    {
        if (Communication.loadOnStart)
        {
            players = new();
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

                playerTile = Instantiate(playerTilePrefab, transform);
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
            //players = data.players;
            //Server.allPlayersInfo = data.players;
           
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
            
            /*
            for (int i = 0; i < players.Count; i++)
            {
                var playerTile = playersTiles.Dequeue();
                playerTile.transform.GetChild(0).GetComponent<TMP_Text>().text = players[i].Position.ToString();
                playerTile.transform.GetChild(1).GetComponent<TMP_Text>().text = players[i].Name;
                playerTile.transform.GetChild(2).GetComponent<TMP_Text>().text = players[i].Points.ToString();
                playerTile.transform.SetSiblingIndex(players[i].Position);
                playersTiles.Enqueue(playerTile);
            }
            */
        }
    }

    [ClientRpc]
    void LoadPlayersListClientRpc(int position,int points,string name,int id,bool isAI, int spaceshipsLeft, int playerTileId)
    {
        PlayerInfo playerInfo = new()
        {
            Position = position,
            Points = points,
            Name= name,
            Id = id,
            IsAI = isAI,
            SpaceshipsLeft = spaceshipsLeft,
            PlayerTileId = playerTileId
        };
        players ??= new();
        Server.allPlayersInfo ??= new();
        players.Add(playerInfo);
        Server.allPlayersInfo.Add(playerInfo);
        Debug.Log($"Players: {players.Count}");
        Debug.Log($"ServerPlayers: {Server.allPlayersInfo.Count}");
        //GameObject playerTextTemplate = transform.GetChild(0).gameObject;
        playersTiles ??= new();
        var playerTile = Instantiate(playerTilePrefab, transform);
        playerTile.transform.GetChild(0).GetComponent<TMP_Text>().text = (playerInfo.Position + 1).ToString();
        playerTile.transform.GetChild(1).GetComponent<TMP_Text>().text = playerInfo.Name;
        playerTile.transform.GetChild(2).GetComponent<TMP_Text>().text = playerInfo.Points.ToString();
        playerTile.transform.GetChild(3).GetComponent<TMP_Text>().text = playerInfo.SpaceshipsLeft.ToString();
        playerTile.transform.SetSiblingIndex(playerInfo.Position);
        playersTiles.Enqueue(playerTile);

    }

    public void SaveData(ref GameData data)
    {
        if(IsHost)
        {
            //Debug.Log(players.Count);
            data.players = Server.allPlayersInfo;
            data.curPlayerId = Server.curPlayerId;
        } 
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayersOrderServerRpc()
    {
        UpdatePlayersOrderClientRpc();
    }

    [ClientRpc]
    public void UpdatePlayersOrderClientRpc()
    {
        // pobieram pierwszego gracza z kolejki (tego, ktorego tura sie zakonczyla)
        var firstElement = playersTiles.Dequeue();

        // ustawiam go na koniec w panelu graczy
        firstElement.transform.SetAsLastSibling();
        
        // dodaje go na koniec kolejki
        playersTiles.Enqueue(firstElement);

        // poprawiam pozycje wszystkich graczy
        int i = 0;
        Debug.Log($"Players: {players.Count}");
        Debug.Log($"ServerPlayers: {Server.allPlayersInfo.Count}");
        Debug.Log($"playerTiles: {playersTiles.Count}");
        foreach (var playerTile in playersTiles) 
        {
            Debug.Log($"i :{i}");
            players[i].Position = Math.Abs(players[i].Position - 1) % players.Count;
            Debug.Log(players[i].Name);
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
    }

    [ClientRpc]
    void SendStartTurnClientRpc(ClientRpcParams clientRpcParams = default)
    {
        PlayerGameData.StartTurn();
    }
}
