using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FinalPlayerResults : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var players = Server.allPlayersInfo.OrderByDescending(p => p.Points).ToList();

        GameObject playerTile;

        int playersCount = players.Count;

        for (int i = 0; i < playersCount; i++)
        {
            var player = players[i];

            playerTile = Instantiate(player.TilePrefab, transform);
            playerTile.transform.GetChild(0).GetComponent<TMP_Text>().text = (players[i].Position + 1).ToString();
            playerTile.transform.GetChild(1).GetComponent<TMP_Text>().text = players[i].Name;
            playerTile.transform.GetChild(2).GetComponent<TMP_Text>().text = players[i].SpaceshipsLeft.ToString();
            playerTile.transform.GetChild(3).GetComponent<TMP_Text>().text = players[i].Points.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
