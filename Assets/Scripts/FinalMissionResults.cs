using Assets.GameplayControl;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FinalMissionResults : MonoBehaviour
{
    public GameObject missionTalePrefab;

    // Start is called before the first frame update
    void Start()
    {
        GameObject missionTale;
        var missions = PlayerGameData.missions;
        int missionsNum = missions.Count;

        for (int i = 0; i < missionsNum; i++)
        {
            missionTale = Instantiate(missionTalePrefab, transform);
            missionTale.name = missions[i].start.name + "-" + missions[i].end.name;
            missionTale.transform.GetChild(0).GetComponent<TMP_Text>().text = missions[i].start.name;
            missionTale.transform.GetChild(1).GetComponent<TMP_Text>().text = missions[i].end.name;
            missionTale.transform.GetChild(2).GetComponent<TMP_Text>().text = missions[i].points.ToString();
            if(missions[i].isDone) missionTale.transform.GetChild(3).gameObject.SetActive(true);
        }
    }
}
