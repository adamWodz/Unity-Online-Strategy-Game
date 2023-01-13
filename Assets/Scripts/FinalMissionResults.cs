using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FinalMissionResults : MonoBehaviour
{
    public GameObject missionTalePrefab;
    List<GameObject> missionTiles = new List<GameObject>();
    public int missionsPlayerId;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void UpdatePlayerId(int id)
    {
        missionsPlayerId = id;
        RefreshList(Server.allPlayersInfo.First(p => p.Id == missionsPlayerId).missions);
    }

    void RefreshList(List<Mission> missions)
    {
        foreach(var tile in missionTiles)
        {
            Destroy(tile);
        }

        missionTiles.Clear();
        
        GameObject missionTile;
        int missionsNum = missions.Count;

        for (int i = 0; i < missionsNum; i++)
        {
            missionTile = Instantiate(missionTalePrefab, transform);
            missionTile.name = missions[i].start.name + "-" + missions[i].end.name;
            missionTile.transform.GetChild(0).GetComponent<TMP_Text>().text = missions[i].start.name;
            missionTile.transform.GetChild(1).GetComponent<TMP_Text>().text = missions[i].end.name;
            missionTile.transform.GetChild(2).GetComponent<TMP_Text>().text = missions[i].points.ToString();
            if (missions[i].isDone) missionTile.transform.GetChild(3).gameObject.SetActive(true);
            missionTiles.Add(missionTile);
        }
    }
}
