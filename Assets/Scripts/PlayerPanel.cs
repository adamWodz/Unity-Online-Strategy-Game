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

    public PlayerInfo()
    {
        Position = 1;
        Points = 0;
        Name = "Gracz";
        Id = 0;
    }
    
    public PlayerInfo(int position, int points, string name,int id) 
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

    // Start is called before the first frame update
    void Start()
    {
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

    public void LoadData(GameData data)
    {
        
    }

    public void SaveData(ref GameData data)
    {
        data.actualPlayer = players.Single(player => player.Position == 1);
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
            int pos = players[i].Position;
            players[i].Position = pos - 1 < 1 ? players.Count : pos - 1;
            playerTile.transform.GetChild(0).GetComponent<TMP_Text>().text = (++i).ToString();
        }
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
