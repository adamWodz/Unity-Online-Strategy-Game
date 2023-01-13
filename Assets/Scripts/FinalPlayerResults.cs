using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FinalPlayerResults : MonoBehaviour
{
    public GameObject missionResultsPanel;
    
    // Start is called before the first frame update
    void Start()
    {
        FinalMissionResults missionResults = missionResultsPanel.GetComponent<FinalMissionResults>();
        
        var players = Server.allPlayersInfo.OrderByDescending(p => p.Points).ToList();

        GameObject playerTile;

        int playersCount = players.Count;

        for (int i = 0; i < playersCount; i++)
        {
            var player = players[i];

            playerTile = Instantiate(player.TilePrefab, transform);
            playerTile.transform.GetChild(0).GetComponent<TMP_Text>().text = (players[i].Position + 1).ToString();
            playerTile.transform.GetChild(1).GetComponent<TMP_Text>().text = players[i].Name;
            playerTile.transform.GetChild(2).GetComponent<TMP_Text>().text = players[i].Points.ToString();
            playerTile.GetComponent<Button>().onClick.AddListener(() => missionResults.UpdatePlayerId(player.Id));
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
