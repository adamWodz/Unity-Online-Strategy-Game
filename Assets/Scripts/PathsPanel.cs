using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Drawing;
using Assets.GameplayControl;
using Unity.Netcode;
using System.Reflection;

public class PathsPanel : Panel
{
    public GameObject pathButtonPrefab;
    public List<Mission> missionsFromClickedMissionsCards;

    private List<Mission> missionsChosen;
    public List<Mission> MissionsChosen
    {
        get
        {
            return missionsChosen;
        }
        set
        {
            missionsChosen.AddRange(value);
            
            // zapis posiadanych misji przez gracza na bie¿¹co
            foreach(Mission m in value)
            {
                SendMissionsChosenServerRpc(m.start.name, m.end.name, m.points, PlayerGameData.Id);
            }
            
            SpawnMissionsButtons(value);
        }
    }

    private bool firstClick;
    public List<MissionData>[] receivedMissions;
    private Map map;

    // Start is called before the first frame update
    void Start()
    {
        map = GameObject.Find("Space").GetComponent<Map>();

        if (IsHost && !Communication.loadOnStart)
        {
            receivedMissions = new List<MissionData>[Server.allPlayersInfo.Count];
            for (int i = 0; i < Server.allPlayersInfo.Count; i++)
            {
                receivedMissions[i] = new();
            }
        }

        firstClick = true;

        missionsFromClickedMissionsCards = new();

        transform.parent.GetComponent<Button>().onClick.AddListener(HighlightPlanets);

        //missionsChoosed = GetRandomElementsFromList(GameObject.Find("Space").GetComponent<Map>().Missions, 3);

        missionsChosen = new();

        //Debug.Log($"Missions choosed: {missionsChosen.Count}");

        AssignValues(349.4f, 585.4f, PanelState.Maximized, true);

        //SpawnMissionsButtons(missionsChoosed);
    }

    // Update is called once per frame
    void Update()
    {
        ChangeWidth();
    }

    private void HighlightPlanets()
    {
        if (firstClick)
        {
            ChangePlanetsColor(UnityEngine.Color.green);
            missionsFromClickedMissionsCards.AddRange(missionsChosen.Except(missionsFromClickedMissionsCards, new MissionComparer()).ToList());

            // zmiana koloru przycisków
            for (int i = 0; i < missionsChosen.Count; i++)
                transform.GetChild(i).GetComponent<Image>().color = UnityEngine.Color.green;

            firstClick = false;
        }
        else
        {
            ChangePlanetsColor(UnityEngine.Color.white);
            missionsFromClickedMissionsCards = missionsFromClickedMissionsCards.Except(missionsChosen, new MissionComparer()).ToList();

            // zmiana koloru przycisków
            for (int i = 0; i < missionsChosen.Count; i++)
                transform.GetChild(i).GetComponent<Image>().color = UnityEngine.Color.white;

            firstClick = true;
        }
    }

    public void HighlightPlanet(Mission mission)
    {

        string firstPlanetName = mission.start.name;
        string secondPlanetName = mission.end.name;
        //Debug.Log(firstPlanetName + "-" + secondPlanetName);

        // podswietlenie przycisku danej sciezki
        var pom = GameObject.Find(firstPlanetName + "-" + secondPlanetName).GetComponent<Image>();
        //Debug.Log(pom.name);

        if (pom.color == UnityEngine.Color.white)
        {
            pom.color = UnityEngine.Color.green;
        }
        else
        {
            pom.color = UnityEngine.Color.white;
        }

        UnityEngine.Color firstPlanetColor = GetPlanetColor(firstPlanetName);
        UnityEngine.Color secondPlanetColor = GetPlanetColor(secondPlanetName);

        if (firstPlanetColor != UnityEngine.Color.green || secondPlanetColor != UnityEngine.Color.green)
        {
            ChangePlanetColor(UnityEngine.Color.green, firstPlanetName);
            ChangePlanetColor(UnityEngine.Color.green, secondPlanetName);
        }
        else
        {
            if (CheckIfPlanetCanBeExtinguished(firstPlanetName, secondPlanetName))
            {
                ChangePlanetColor(UnityEngine.Color.white, firstPlanetName);
            }

            if (CheckIfPlanetCanBeExtinguished(secondPlanetName, firstPlanetName))
            {
                ChangePlanetColor(UnityEngine.Color.white, secondPlanetName);
            }
        }

        if (missionsFromClickedMissionsCards.Contains(mission))
            missionsFromClickedMissionsCards.Remove(mission);
        else
            missionsFromClickedMissionsCards.Add(mission);
    }

    void ChangePlanetsColor(UnityEngine.Color color)
    {
        for (int i = 0; i < missionsChosen.Count; i++)
        {
            //Debug.Log($"{paths[i].planetFrom.name} - {paths[i].planetTo.name}");
            if (CheckIfPlanetCanBeExtinguished(missionsChosen[i].start.name, missionsChosen[i].end.name))
                ChangePlanetColor(color, missionsChosen[i].start.name);
            if (CheckIfPlanetCanBeExtinguished(missionsChosen[i].end.name, missionsChosen[i].start.name))
                ChangePlanetColor(color, missionsChosen[i].end.name);
        }
    }

    void ChangePlanetColor(UnityEngine.Color color, string name)
    {
        GetPlanetRenderer(name).material.color = color;
    }

    UnityEngine.Color GetPlanetColor(string name)
    {
        return GetPlanetRenderer(name).material.color;
    }

    Renderer GetPlanetRenderer(string name)
    {
        return GameObject.Find(name).GetComponent<Renderer>();
    }

    bool CheckIfPlanetCanBeExtinguished(string planetToExtinguishName, string secondPlanetFromPathName)
    {
        foreach (Mission p in missionsFromClickedMissionsCards)
        {
            if (p.start.name == planetToExtinguishName && p.end.name != secondPlanetFromPathName)
            {
                UnityEngine.Color planetColor = GetPlanetColor(p.end.name);
                if (planetColor == UnityEngine.Color.green)
                {
                    return false;
                }
            }
            else if (p.end.name == planetToExtinguishName && p.start.name != secondPlanetFromPathName)
            {
                UnityEngine.Color planetColor = GetPlanetColor(p.start.name);
                if (planetColor == UnityEngine.Color.green)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void SpawnMissionsButtons(List<Mission> newMissions)
    {
        GameObject missionButton;

        int n = newMissions.Count;

        for (int i = 0; i < n; i++)
        {
            int copy = i;
            missionButton = Instantiate(pathButtonPrefab, transform);
            missionButton.name = newMissions[i].start.name + "-" + newMissions[i].end.name;
            missionButton.transform.GetChild(0).GetComponent<TMP_Text>().text = newMissions[i].start.name;
            missionButton.transform.GetChild(1).GetComponent<TMP_Text>().text = newMissions[i].end.name;
            missionButton.transform.GetChild(2).GetComponent<TMP_Text>().text = newMissions[i].points.ToString();
            missionButton.GetComponent<Button>().onClick.AddListener(() => HighlightPlanet(newMissions[copy]));
        }
    }

    public override void LoadData(GameData data)
    {
        if (IsHost)
        {
            receivedMissions = new List<MissionData>[Server.allPlayersInfo.Count];
            for (int i = 0; i < Server.allPlayersInfo.Count; i++)
            {
                receivedMissions[i] = new();
            }

            // host wczytuje dane bez rpc
            var missionsData = data.missionsForEachPalyer[PlayerGameData.Id];
            var missions = map.Missions.Where(mission =>
            {
                foreach (MissionData m in missionsData)
                {
                    if(m.startPlanetName == mission.start.name && m.endPlanetName == mission.end.name && m.points == mission.points)
                        return true;
                }
                return false;
            });
            MissionsChosen = missions.ToList();

            //AI
            foreach(var artificialPlayer in Server.artificialPlayers)
            {
                missionsData = data.missionsForEachPalyer[artificialPlayer.Id];
                missions = map.Missions.Where(mission =>
                {
                    foreach (MissionData m in missionsData)
                    {
                        if (m.startPlanetName == mission.start.name && m.endPlanetName == mission.end.name && m.points == mission.points)
                            return true;
                    }
                    return false;
                });
                artificialPlayer.missions = missions.ToList();
                foreach(var mission in missions)
                {
                    if (mission.IsCompletedByClientPlayer())
                        artificialPlayer.missionsDone.Add(mission);
                    else
                        artificialPlayer.missionsToDo = missions.ToList();
                }
            }

            // host wysy³a rpc innym graczom z danymi
            for (int i = 0; i < Server.allPlayersInfo.Count; i++)
            {
                if (i != PlayerGameData.Id)
                {
                    // ustawiam rpc na wysy³anie do konkretnego gracza (kazdy gracz musi otrzymac inne dane)
                    ClientRpcParams clientRpcParams = new()
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new ulong[] { (ulong)i }
                        }
                    };

                    missionsData = data.missionsForEachPalyer[i];
                    foreach(var mD in missionsData)
                    {
                        LoadMissionsChosenClientRpc(mD.startPlanetName,mD.endPlanetName,mD.points, clientRpcParams);
                    }
                }
            }
        }
    }

    public override void SaveData(ref GameData data)
    {
        if (IsHost)
        {
            
            // AI
            foreach (var artificialPlayer in Server.artificialPlayers)
            {
                List<MissionData> pom = new();
                foreach (var mission in artificialPlayer.missions)
                {
                    pom.Add(new MissionData()
                    {
                        startPlanetName = mission.start.name,
                        endPlanetName = mission.end.name,
                        points = mission.points,
                    });
                }
                receivedMissions[artificialPlayer.Id] = new(pom);
            }


            for (int i = 0; i < Server.allPlayersInfo.Count; i++)
            {
                //Debug.Log("Received missions:" + receivedMissions[i].Count);
                if (!data.missionsForEachPalyer.ContainsKey(i))
                    data.missionsForEachPalyer.Add(i, receivedMissions[i]);
                else
                    data.missionsForEachPalyer[i] = receivedMissions[i];
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SendMissionsChosenServerRpc(string startPlanetName, string endPlanetName, int points,int id)//ServerRpcParams serverRpcParams = default)
    {
        Debug.Log($"gracz {id} dobra³ misjê");
        
        //int id = (int)serverRpcParams.Receive.SenderClientId;
        receivedMissions[id].Add(new MissionData()
        {
            startPlanetName = startPlanetName,
            endPlanetName = endPlanetName,
            points = points,
        });
        //Debug.Log("Server Rpc received missions: "+ receivedMissions[id].Count);
        //

        Debug.Log($"receivedMissions {receivedMissions.Length}");
        for(int i = 0; i < Server.allPlayersInfo.Count; i++)
            Debug.Log($"data: {i} {receivedMissions[i].Count}");
    }

    [ClientRpc]
    void LoadMissionsChosenClientRpc(string startPlanetName, string endPlanetName, int points, ClientRpcParams clientRpcParams = default)
    {
        map = GameObject.Find("Space").GetComponent<Map>();
        Debug.Log(map);
        Mission mission = map.Missions.Single(m => m.start.name == startPlanetName && m.end.name == endPlanetName && m.points == points);
        Debug.Log(mission);
        List<Mission> missions = new()
        {
            mission
        };
        missionsChosen ??= new();
        MissionsChosen = missions;
    }
}
