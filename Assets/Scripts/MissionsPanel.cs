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
    public int missionsDrawNumber = 3;
    
    public Button[] missionButtonsAndConfirmButton;           

    private PathsPanel pathsPanel;

    private GameManager gameManager;

    Map map;

    void Start()
    {
        pathsPanel = GameObject.Find("PathsPanel").GetComponent<PathsPanel>();
        
        AssignValues(0, 370, PanelState.Minimized, false);

        missionsToChoose = new();
        missionsToChoose.AddRange(Map.mapData.missions);
        missionButtonsAndConfirmButton = transform.GetComponentsInChildren<Button>();
        
        missionButtonsAndConfirmButton[^1].onClick.AddListener(AddMissions);
        drawMissionsCardsButton.onClick.AddListener(DrawMissions);

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

    }

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
    public void SyncMissionsToChooseServerRpc(string startPlanetName, string endPlanetName)
    {
        SyncMissionsToChooseClientRpc(startPlanetName, endPlanetName);
    }
    private void DrawMissions()
    {
        if(!Communication.TryStartDrawingMission())
            return;
        
        if (missionsToChoose.Count < 3)
        {
            gameManager.SetPopUpWindow("Jest za ma�o kart misji, aby mo�na by�o dobra� nowe!");
        }
        else
        {
            popUpPanel.SetActive(true);

            for (int i = 0; i < missionButtonsAndConfirmButton.Length - 1; i++)
            {
                missionButtonsAndConfirmButton[i].onClick.RemoveAllListeners();
                missionButtonsAndConfirmButton[i].name = "";
            }

            var randomMissions = GetRandomMissions();

            for (int i = 0; i < missionButtonsAndConfirmButton.Length - 1; i++)
            {
                int copy = i;
                missionButtonsAndConfirmButton[copy].name =  randomMissions[copy].start.name + "-" + randomMissions[copy].end.name;
                missionButtonsAndConfirmButton[copy].transform.GetChild(0).GetComponent<TMP_Text>().text = randomMissions[copy].start.name;
                missionButtonsAndConfirmButton[copy].transform.GetChild(1).GetComponent<TMP_Text>().text = randomMissions[copy].end.name;
                missionButtonsAndConfirmButton[copy].transform.GetChild(2).GetComponent<TMP_Text>().text = randomMissions[copy].points.ToString();
                missionButtonsAndConfirmButton[copy].onClick.AddListener(() => pathsPanel.HighlightPlanet(randomMissions[copy]));
            }
        }

    }
    void AddMissions()
    {
        var missionsChoosed = pathsPanel.missionsFromClickedMissionsCards.Except(pathsPanel.MissionsChosen, new MissionComparer()).ToList();

        if (missionsChoosed.Count > 0)         
        {
            Communication.DrawMissions(missionsChoosed);

            pathsPanel.MissionsChosen = missionsChoosed;

            foreach (Mission m in missionsChoosed)
            {
                pathsPanel.HighlightPlanet(m);
            }

            for (int i = 0; i < missionButtonsAndConfirmButton.Length - 1; i++)
            {
                missionButtonsAndConfirmButton[i].onClick.RemoveAllListeners();
                missionButtonsAndConfirmButton[i].name = "";
            }

            foreach (Mission m in missionsChoosed)
            {
                SyncMissionsToChooseServerRpc(m.start.name, m.end.name);
            }

            ChangeState();
            pathsPanel.ChangeState();
            drawMissionsCardsButton.enabled = true;
        }
        else
        {
            gameManager.SetPopUpWindow("Musisz wybrać co najmniej jednć kartę misji!");
        }
    }

    public List<Mission> GetRandomMissions()
    {
        return missionsToChoose.OrderBy(arg => Guid.NewGuid()).Take(missionsDrawNumber).ToList();
    }
    public override void LoadData(GameData data)
    {
        if (IsHost)
        {
            foreach (PlayerInfo playerInfo in data.players)
            {
                List<MissionData> missionsChoosed = data.missionsForEachPalyer[playerInfo.Id];
                foreach (MissionData m in missionsChoosed)
                {
                    SyncMissionsToChooseClientRpc(m.startPlanetName, m.endPlanetName);
                }
            }
            
        }
    }

    public override void SaveData(ref GameData data)
    {
        if (IsHost)
        {
        }
    }

}
