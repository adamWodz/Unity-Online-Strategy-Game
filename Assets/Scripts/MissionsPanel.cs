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

    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        pathsPanel = GameObject.Find("PathsPanel").GetComponent<PathsPanel>();
        
        AssignValues(0, 242.9984f, PanelState.Minimized, false);

        //missionsToChoose.AddRange(GameObject.Find("Space").GetComponent<Map>().Missions.Except(pathsPanel.MissionsChoosed,new MissionComparer()).ToList());
        missionsToChoose.AddRange(GameObject.Find("Space").GetComponent<Map>().Missions);

        Debug.Log($"Missions To Choose: {missionsToChoose.Count}");
        missionButtonsAndConfirmButton = transform.GetComponentsInChildren<Button>();
        
        missionButtonsAndConfirmButton[^1].onClick.AddListener(AddMissions);
        drawMissionsCardsButton.onClick.AddListener(DrawMissions);

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
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
    /*
    [ClientRpc]
    void SyncLoadMissionsToChoose(string startPlanetName, string endPlanetName,int points)
    {
        Mission m = ScriptableObject.CreateInstance<Mission>();
        m.start = Planets;
        missionsToChoose.Add();
    }
    */
    private void DrawMissions()
    {
        if (missionsToChoose.Count < 3)
        {
            gameManager.SetPopUpWindow("Jest za ma�o kart misji, aby mo�na by�o dobra� nowe!");
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
        // misje, kt�re dobrali�my
        var missionsChoosed = pathsPanel.missionsFromClickedMissionsCards.Except(pathsPanel.MissionsChosen, new MissionComparer()).ToList();

        Communication.DrawMissions(missionsChoosed);

        if (missionsChoosed.Count > 0) // gracz musi dobra� co najmniej jedn� kart� misji
        {
            pathsPanel.MissionsChosen = missionsChoosed;

            // wygaszamy planety i przyciski
            foreach (Mission m in missionsChoosed)
            {
                pathsPanel.HighlightPlanet(m);
            }

            // "czy�cimy" przyciski z MissionsPanel
            for (int i = 0; i < missionButtonsAndConfirmButton.Length - 1; i++)
            {
                missionButtonsAndConfirmButton[i].onClick.RemoveAllListeners();
                missionButtonsAndConfirmButton[i].name = "";
            }

            // usuwamy misje z listy misju mo�liwych do wyboru i synchronizujemy t� list� z innymi graczami
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
            gameManager.SetPopUpWindow("Musisz wybra� co najmniej jedn� kart� misji!");
        }
    }
    public override void LoadData(GameData data)
    {
        if (IsHost)
        {
            //missionsToChoose = data.missionsToChoose;
            /*
            foreach (PlayerInfo playerInfo in Server.allPlayersInfo)
            {
                var missionsChoosed = data.missionsForEachPalyer[playerInfo.Id];
                foreach (MissionData m in missionsChoosed)
                    SyncMissionsToChooseClientRpc(m.startPlanetName, m.endPlanetName);
            }
            */
        }
    }

    public override void SaveData(ref GameData data)
    {
        if (IsHost)
        {
            //data.missionsToChoose = missionsToChoose;
            //var missionschoosed
        }
    }

}
