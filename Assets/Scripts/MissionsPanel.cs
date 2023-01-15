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
    
    public Button[] missionButtonsAndConfirmButton; // 3 pierwsze przyciski to karty misji, a ostatni to zatwierdzenie

    private PathsPanel pathsPanel;

    private GameManager gameManager;

    Map map;

    // Start is called before the first frame update
    void Start()
    {
        //gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        pathsPanel = GameObject.Find("PathsPanel").GetComponent<PathsPanel>();
        
        AssignValues(0, 242.9984f, PanelState.Minimized, false);

        //missionsToChoose.AddRange(GameObject.Find("Space").GetComponent<Map>().Missions.Except(pathsPanel.MissionsChoosed,new MissionComparer()).ToList());
        //Debug.Log(GameObject.Find("Space").GetComponent<Map>().Missions);
        missionsToChoose = new();
        missionsToChoose.AddRange(Map.mapData.missions);
        /*
        if (!Communication.loadOnStart)
        {
            missionsToChoose.AddRange(GameObject.Find("Space").GetComponent<Map>().Missions);
        }
        */
        //Debug.Log(missionsToChoose);
        //Debug.Log($"Missions To Choose: {missionsToChoose.Count}");
        missionButtonsAndConfirmButton = transform.GetComponentsInChildren<Button>();
        
        missionButtonsAndConfirmButton[^1].onClick.AddListener(AddMissions);
        drawMissionsCardsButton.onClick.AddListener(DrawMissions);

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        Debug.Log(Map.mapData.missions);
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
    public void SyncMissionsToChooseServerRpc(string startPlanetName, string endPlanetName)
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
        if(!Communication.TryStartDrawingMission())
            return;
        
        if (missionsToChoose.Count < 3)
        {
            gameManager.SetPopUpWindow("Jest za ma�o kart misji, aby mo�na by�o dobra� nowe!");
        }
        else
        {
            popUpPanel.SetActive(true);

            // "czy�cimy" przyciski z MissionsPanel
            for (int i = 0; i < missionButtonsAndConfirmButton.Length - 1; i++)
            {
                missionButtonsAndConfirmButton[i].onClick.RemoveAllListeners();
                missionButtonsAndConfirmButton[i].name = "";
            }

            var randomMissions = GetRandomMissions();

            Debug.Log($"RandomPaths: {randomMissions.Count}");
            for (int i = 0; i < missionButtonsAndConfirmButton.Length - 1; i++)
            {
                int copy = i;
                missionButtonsAndConfirmButton[copy].name = missionButtonsAndConfirmButton[copy].transform.GetChild(0).GetComponent<TMP_Text>().text = randomMissions[copy].start.name + "-" + randomMissions[copy].end.name;
                missionButtonsAndConfirmButton[copy].onClick.AddListener(() => pathsPanel.HighlightPlanet(randomMissions[copy]));
            }
        }

    }
    void AddMissions()
    {
        // misje, kt�re dobrali�my
        var missionsChoosed = pathsPanel.missionsFromClickedMissionsCards.Except(pathsPanel.MissionsChosen, new MissionComparer()).ToList();

        if (missionsChoosed.Count > 0) // gracz musi dobra� co najmniej jedn� kart� misji
        {
            Communication.DrawMissions(missionsChoosed);

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

    public List<Mission> GetRandomMissions()
    {
        return missionsToChoose.OrderBy(arg => Guid.NewGuid()).Take(missionsDrawNumber).ToList();
    }
    public override void LoadData(GameData data)
    {
        if (IsHost)
        {
            map = GameObject.Find("Space").GetComponent<Map>();
            Debug.Log($"Mapa: {map}");
            Debug.Log(missionsToChoose);
            missionsToChoose ??= new();
            Debug.Log(map.Missions);
            Debug.Log(Map.mapData.missions);
            //missionsToChoose.AddRange(map.Missions);
            //missionsToChoose = data.missionsToChoose;
            
            foreach (PlayerInfo playerInfo in Server.allPlayersInfo)
            {
                var missionsChoosed = data.missionsForEachPalyer[playerInfo.Id];
                foreach (MissionData m in missionsChoosed)
                    SyncMissionsToChooseClientRpc(m.startPlanetName, m.endPlanetName);
            }
            
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
