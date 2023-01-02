using Assets.GameplayControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MissionsPanel : Panel
{
    public List<Mission> missionsToChoose = new();
    
    public Button[] missionButtonsAndConfirmButton; // 3 pierwsze przyciski to karty misji, a ostatni to zatwierdzenie

    private PathsPanel pathsPanel;

    // Start is called before the first frame update
    void Start()
    {
        pathsPanel = GameObject.Find("PathsPanel").GetComponent<PathsPanel>();
        
        AssignValues(0, 242.9984f, PanelState.Minimized, false);
        
        missionsToChoose.AddRange(GameObject.Find("Space").GetComponent<Map>().Missions.Except(pathsPanel.MissionsChoosed,new MissionComparer()).ToList());
        
        Debug.Log($"Missions To Choose: {missionsToChoose.Count}");
        missionButtonsAndConfirmButton = transform.GetComponentsInChildren<Button>();
        /*
        var randomPaths = pathsPanel.GetRandomElements(missionssToChoose, 3);
        
        Debug.Log($"RandomPaths: {randomPaths.Count}");
        for (int i = 0; i < missionButtonsAndConfirmButton.Length - 1; i++)
        {
            int copy = i;
            missionButtonsAndConfirmButton[copy].name = missionButtonsAndConfirmButton[copy].transform.GetChild(0).GetComponent<TMP_Text>().text = randomPaths[copy].start.name + "-" + randomPaths[copy].end.name;
            missionButtonsAndConfirmButton[copy].onClick.AddListener(() => pathsPanel.HighlightPlanet(randomPaths[copy]));
        }
        */
        missionButtonsAndConfirmButton[^1].onClick.AddListener(AddMissions);
        button.onClick.AddListener(DrawMissions);
    }

    // Update is called once per frame
    void Update()
    {
        ChangeWidth();
    }
    private void DrawMissions()
    {
        popUpPanel.SetActive(true);

        var randomPaths = pathsPanel.GetRandomElementsFromList(missionsToChoose, 3);

        Debug.Log($"RandomPaths: {randomPaths.Count}");
        for (int i = 0; i < missionButtonsAndConfirmButton.Length - 1; i++)
        {
            int copy = i;
            missionButtonsAndConfirmButton[copy].name = missionButtonsAndConfirmButton[copy].transform.GetChild(0).GetComponent<TMP_Text>().text = randomPaths[copy].start.name + "-" + randomPaths[copy].end.name;
            missionButtonsAndConfirmButton[copy].onClick.AddListener(() => pathsPanel.HighlightPlanet(randomPaths[copy]));
        }

    }
    void AddMissions()
    {
        // misje, które dobraliœmy
        var missionsChoosed = pathsPanel.missionsFromClickedMissionsCards.Except(pathsPanel.MissionsChoosed, new MissionComparer()).ToList();
        
        pathsPanel.MissionsChoosed = missionsChoosed;
        
        // wygaszamy planety i przyciski
        foreach(Mission m in missionsChoosed) 
        {
            pathsPanel.HighlightPlanet(m);
        }

        // "czyœcimy" przyciski z MissionsPanel
        for (int i = 0; i < missionButtonsAndConfirmButton.Length - 1; i++)
        {
            missionButtonsAndConfirmButton[i].onClick.RemoveAllListeners();
            missionButtonsAndConfirmButton[i].name = "";
        }

        // usuwamy misje z listy misju mo¿liwych do wyboru
        missionsToChoose.RemoveAll(m => missionsChoosed.Contains(m));

        // przywracamy stan paneli sprzed dobierania
        ChangeState();
        pathsPanel.ChangeState();
        button.enabled = true;
    }
}
