using Assets.GameplayControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
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

    public PlayerInfo()
    {
        Position = 0;
        Points = 0;
        Name = "Gracz";
        Id = 0;
    }

    public PlayerInfo(int position, int points, string name, int id)
    {
        Position = position;
        Points = points;
        Name = name;
        Id = id;  
    }
}

public class PlayerPanel : NetworkBehaviour, IDataPersistence
{
   
    [SerializeField] public List<PlayerInfo> players;
    Queue<GameObject> playersTiles;
    Dictionary<int, GameObject> playerTilesByIds = new Dictionary<int, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        players = Server.allPlayersInfo;
        
        playersTiles = new();

        GameObject playerTextTemplate = transform.GetChild(0).gameObject;
        GameObject playerTile;

        int playersCount = players.Count;

        for (int i = 0; i < playersCount; i++)
        {
            var player = players.Where(p => p.Position == i).First();

            playerTile = Instantiate(playerTextTemplate, transform);
            playerTile.transform.GetChild(0).GetComponent<TMP_Text>().text = (players[i].Position + 1).ToString();
            playerTile.transform.GetChild(1).GetComponent<TMP_Text>().text = players[i].Name;
            playerTile.transform.GetChild(2).GetComponent<TMP_Text>().text = players[i].SpaceshipsLeft.ToString();
            playerTile.transform.GetChild(3).GetComponent<TMP_Text>().text = players[i].SpaceshipsLeft.ToString();
            playersTiles.Enqueue(playerTile);

            player.PlayerTileId = playerTile.GetInstanceID();
            playerTilesByIds.Add(player.PlayerTileId, playerTile);
        }
        Destroy(playerTextTemplate);
    }

    // Update is called once per frame
    void Update()
    {
        // test
        if(Input.GetKeyDown(KeyCode.T)) 
        {
            UpdatePlayersOrderServerRpc();
        }   
    }

    public void LoadData(GameData data)
    {
        players = data.players;
        for(int i=0; i < players.Count; i++)
        {
            var playerTile = playersTiles.Dequeue();
            playerTile.transform.GetChild(0).GetComponent<TMP_Text>().text = players[i].Position.ToString();
            playerTile.transform.GetChild(1).GetComponent<TMP_Text>().text = players[i].Name;
            playerTile.transform.GetChild(2).GetComponent<TMP_Text>().text = players[i].Points.ToString();
            playerTile.transform.SetSiblingIndex(players[i].Position);
            playersTiles.Enqueue(playerTile);
        }
    }

    public void SaveData(ref GameData data)
    {
        data.actualPlayer = players.Single(player => player.Position == 0);
        data.players = players;
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
        foreach(var playerTile in playersTiles) 
        {
            players[i].Position = (players[i].Position + 1) % players.Count;
            //Debug.Log(players[i].Name + ". Position: " + players[i].Position);
            i++;
            playerTile.transform.GetChild(0).GetComponent<TMP_Text>().text = i.ToString();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartNextPlayerTurnServerRpc()
    {
        PlayerInfo nextPlayer = players.Where(p => p.Position == 0).First();
        if (nextPlayer.IsAI)
            Server.artificialPlayers.Where(ai => ai.Id == nextPlayer.Id).First().BestMove();
        else
            StartNextPlayerTurnClientRpc(nextPlayer.Id);
    }

    [ClientRpc]
    public void StartNextPlayerTurnClientRpc(int playerId)
    {
        Debug.Log("StartNextPlayerTurnClientRpc; playerId: " + playerId + " thisPlayerId: " + PlayerGameData.Id);
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
}
