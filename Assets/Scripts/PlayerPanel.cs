using Assets.GameplayControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerPanel : NetworkBehaviour
{
    [Serializable]
    public class PlayerInfo
    {
        public int Position;
        public int Points;
        public string Name;
        public int Id;
        public bool IsAI;
    }

    [SerializeField] public List<PlayerInfo> players;
    Queue<GameObject> playersTiles;

    // Start is called before the first frame update
    void Start()
    {
        players = Server.allPlayersInfo;
        
        playersTiles = new();

        GameObject playerTextTemplate = transform.GetChild(0).gameObject;
        GameObject playerTile;

        int n = players.Count;

        for (int i = 0; i < n; i++)
        {
            playerTile = Instantiate(playerTextTemplate, transform);
            playerTile.transform.GetChild(0).GetComponent<TMP_Text>().text = players[i].Position.ToString();
            playerTile.transform.GetChild(1).GetComponent<TMP_Text>().text = players[i].Name;
            playerTile.transform.GetChild(2).GetComponent<TMP_Text>().text = players[i].Points.ToString();
            playersTiles.Enqueue(playerTile);
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
            players[i].Position = ++i;
            playerTile.transform.GetChild(0).GetComponent<TMP_Text>().text = i.ToString();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartNextPlayerTurnServerRpc()
    {
        PlayerInfo nextPlayer = players.Where(p => p.Position == 1).First();
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

    [ServerRpc]
    public void UpdatePlayerPointsServerRpc(int playerId, int playerPoints)
    {
        UpdatePlayerPointsClientRpc(playerId, playerPoints);
    }

    [ClientRpc]
    public void UpdatePlayerPointsClientRpc(int playerId, int playerPoints)
    {
        foreach(var player in players)
        {
            if(player.Id == playerId)
            {
                player.Points = playerPoints;
                break;
            }
        }
    }
}
