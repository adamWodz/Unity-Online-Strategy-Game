using Assets.GameplayControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Netcode;
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
        
        missionButtonsAndConfirmButton[^1].onClick.AddListener(AddMissions);
        drawMissionsCardsButton.onClick.AddListener(DrawMissions);
    }

    // Update is called once per frame
    void Update()
    {
        ChangeWidth();
    }

    [ClientRpc]
    void SyncMissionsToChooseClientRpc(string startPlanetName, string endPlanetName)
    {
        missionsToChoose.RemoveAll(m => m.start.name == startPlanetName && m.end.name == endPlanetName);
    }

    [ServerRpc(RequireOwnership = false)]
    void SyncMissionsToChooseServerRpc(string startPlanetName, string endPlanetName)
    {
        SyncMissionsToChooseClientRpc(startPlanetName, endPlanetName);
    }
    
    private void DrawMissions()
    {
        if (missionsToChoose.Count < 3)
        {
            SetPopUpWindow("Jest za ma³o kart misji, aby mo¿na by³o dobraæ nowe!");
        }
        else
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

    }
    void AddMissions()
    {
        // misje, które dobraliœmy
        var missionsChoosed = pathsPanel.missionsFromClickedMissionsCards.Except(pathsPanel.MissionsChoosed, new MissionComparer()).ToList();
        
        if (missionsChoosed.Count > 0) // gracz musi dobraæ co najmniej jedn¹ kartê misji
        {
            pathsPanel.MissionsChoosed = missionsChoosed;

            // wygaszamy planety i przyciski
            foreach (Mission m in missionsChoosed)
            {
                pathsPanel.HighlightPlanet(m);
            }

            // "czyœcimy" przyciski z MissionsPanel
            for (int i = 0; i < missionButtonsAndConfirmButton.Length - 1; i++)
            {
                missionButtonsAndConfirmButton[i].onClick.RemoveAllListeners();
                missionButtonsAndConfirmButton[i].name = "";
            }

            // usuwamy misje z listy misju mo¿liwych do wyboru i synchronizujemy t¹ listê z innymi graczami
            foreach (Mission m in missionsChoosed)
            {
                SyncMissionsToChooseServerRpc(m.start.name, m.end.name);
            }

            // przywracamy stan paneli sprzed dobierania
            ChangeState();
            pathsPanel.ChangeState();
            drawMissionsCardsButton.enabled = true;
        }
        else
        {
            SetPopUpWindow("Musisz wybraæ co najmniej jedn¹ kartê misji!");
        }
    }

    void SetPopUpWindow(string message)
    {
        var popUp = transform.parent.GetChild(0);
        popUp.GetChild(0).GetComponent<TMP_Text>().text = message;
        popUp.gameObject.SetActive(true);
    }
}
