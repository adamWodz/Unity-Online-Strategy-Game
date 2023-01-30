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
            
            foreach(Mission m in value)
            {
                SendMissionsChosenServerRpc(m.start.name, m.end.name, m.points, PlayerGameData.Id, m.isDone);
            }
            
            SpawnMissionsButtons(value);
        }
    }

    private bool firstClick;
    public List<MissionData>[] receivedMissions;
    private Map map;
    private UnityEngine.Color highlightColor = UnityEngine.Color.green;
    private UnityEngine.Color extinguishColor = UnityEngine.Color.white;

    void Start()
    {
        map = GameObject.Find("Space").GetComponent<Map>();

        if (IsHost && !Communication.loadOnStart)
        {
            receivedMissions = new List<MissionData>[10];
            for (int i = 0; i < Server.allPlayersInfo.Count; i++)
            {
                receivedMissions[Server.allPlayersInfo[i].Id] = new();
            }
        }

        firstClick = true;

        missionsFromClickedMissionsCards = new();

        transform.parent.GetComponent<Button>().onClick.AddListener(HighlightPlanets);

        missionsChosen = new();

        AssignValues(234, 585, PanelState.Maximized, true);

    }

    void Update()
    {
        ChangeWidth();
    }

    private void HighlightPlanets()
    {
        if (firstClick)
        {
            ChangePlanetsColor(highlightColor);
            missionsFromClickedMissionsCards.AddRange(missionsChosen.Except(missionsFromClickedMissionsCards, new MissionComparer()).ToList());

            for (int i = 0; i < missionsChosen.Count; i++)
                transform.GetChild(i).GetComponent<Image>().color = UnityEngine.Color.green;

            firstClick = false;
        }
        else
        {
            ChangePlanetsColor(extinguishColor);
            missionsFromClickedMissionsCards = missionsFromClickedMissionsCards.Except(missionsChosen, new MissionComparer()).ToList();

            for (int i = 0; i < missionsChosen.Count; i++)
                transform.GetChild(i).GetComponent<Image>().color = UnityEngine.Color.white;

            firstClick = true;
        }
    }

    public void HighlightPlanet(Mission mission)
    {

        string firstPlanetName = mission.start.name;
        string secondPlanetName = mission.end.name;
        var pom = GameObject.Find(firstPlanetName + "-" + secondPlanetName).GetComponent<Image>();
        if (pom.color == extinguishColor)
        {
            pom.color = highlightColor;
        }
        else
        {
            pom.color = extinguishColor;
        }

        UnityEngine.Color firstPlanetColor = GetPlanetColor(firstPlanetName);
        UnityEngine.Color secondPlanetColor = GetPlanetColor(secondPlanetName);

        if (firstPlanetColor != highlightColor || secondPlanetColor != highlightColor)
        {
            ChangePlanetColor(highlightColor, firstPlanetName);
            ChangePlanetColor(highlightColor, secondPlanetName);
        }
        else
        {
            if (CheckIfPlanetCanBeExtinguished(firstPlanetName, secondPlanetName))
            {
                ChangePlanetColor(extinguishColor, firstPlanetName);
            }

            if (CheckIfPlanetCanBeExtinguished(secondPlanetName, firstPlanetName))
            {
                ChangePlanetColor(extinguishColor, secondPlanetName);
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
            if (CheckIfPlanetCanBeExtinguished(missionsChosen[i].start.name, missionsChosen[i].end.name))
                ChangePlanetColor(color, missionsChosen[i].start.name);
            if (CheckIfPlanetCanBeExtinguished(missionsChosen[i].end.name, missionsChosen[i].start.name))
                ChangePlanetColor(color, missionsChosen[i].end.name);
        }
    }

    void ChangePlanetColor(UnityEngine.Color color, string name)
    {
        GetPlanetRenderer(name).material.color = color;
        GameObject.Find(name).transform.GetChild(0).gameObject.SetActive(color == highlightColor);
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
            missionButton.transform.GetChild(3).gameObject.SetActive(newMissions[i].isDone);
            missionButton.GetComponent<Button>().onClick.AddListener(() => HighlightPlanet(newMissions[copy]));
        }
    }

    public override void LoadData(GameData data)
    {
        if (IsHost)
        {
            receivedMissions = new List<MissionData>[10];
            for (int i = 0; i < data.players.Count; i++)
            {
                receivedMissions[i] = new();
            }

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
            
            PlayerGameData.missions ??= new();
            PlayerGameData.completedMissions ??= new();
            PlayerGameData.completedMissions = missions.Where(m => m.isDone).ToList();
            PlayerGameData.missions = missions.ToList();
            PlayerGameData.groupsOfConnectedPlanets ??= new();
            foreach (Path path in map.Paths)
            {
                if (path.builtById == PlayerGameData.Id)
                {
                    ConnectedPlanets.AddPlanetsFromPathToPlanetsGroups(path, PlayerGameData.groupsOfConnectedPlanets);
                }
            }
            foreach (var artificialPlayer in Server.artificialPlayers)
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
                    if (mission.isDone)
                        artificialPlayer.missionsDone.Add(mission);
                    else
                        artificialPlayer.missionsToDo.Add(mission);
                }
                artificialPlayer.groupsOfConnectedPlanets ??= new();
                foreach (Path path in map.Paths)
                {
                    if (path.builtById == artificialPlayer.Id)
                    {
                        ConnectedPlanets.AddPlanetsFromPathToPlanetsGroups(path, artificialPlayer.groupsOfConnectedPlanets);
                    }
                }
            }

            for (int i = 0; i < data.players.Count; i++)
            {
                if (data.players[i].Id != PlayerGameData.Id)
                {
                    missionsData = data.missionsForEachPalyer[data.players[i].Id];
                    foreach(var mD in missionsData)
                    {
                        LoadMissionsChosenClientRpc(mD.startPlanetName, mD.endPlanetName, mD.points, mD.isDone, data.players[i].Name, data.players[i].Id, data.players[i].UnityId); 
                    }
                }
            }
        }
    }

    public override void SaveData(ref GameData data)
    {
        if (IsHost)
        {
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
                        isDone = mission.isDone,
                    });
                }
                receivedMissions[artificialPlayer.Id] = new(pom);
            }


            for (int i = 0; i < 10; i++)
            {
                if (receivedMissions[i] != null)
                {
                    if (!data.missionsForEachPalyer.ContainsKey(i))
                        data.missionsForEachPalyer.Add(i, receivedMissions[i]);
                    else
                        data.missionsForEachPalyer[i] = receivedMissions[i];
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SendMissionsChosenServerRpc(string startPlanetName, string endPlanetName, int points,int id,bool isDone)   
    {
        receivedMissions[id].Add(new MissionData()
        {
            startPlanetName = startPlanetName,
            endPlanetName = endPlanetName,
            points = points,
            isDone = isDone
        });

    }

    [ClientRpc]
    void LoadMissionsChosenClientRpc(string startPlanetName, string endPlanetName, int points,bool isDone,string name,int id,string UnityId, ClientRpcParams clientRpcParams = default)
    {
        if (PlayerGameData.UnityId == UnityId)
        {
            PlayerGameData.Id = id;
            map = GameObject.Find("Space").GetComponent<Map>();
            Mission mission = map.Missions.Single(m => m.start.name == startPlanetName && m.end.name == endPlanetName && m.points == points);
            mission.isDone = isDone;
            List<Mission> missions = new()
            {
            mission
            };
            PlayerGameData.missions ??= new();
            PlayerGameData.completedMissions ??= new();
            if (isDone)
                PlayerGameData.completedMissions.Add(mission);
            PlayerGameData.missions.Add(mission);
            missionsChosen ??= new();
            MissionsChosen = missions;
            PlayerGameData.groupsOfConnectedPlanets ??= new();
            foreach (Path path in map.Paths)
            {
                if (path.builtById == PlayerGameData.Id)
                {
                    ConnectedPlanets.AddPlanetsFromPathToPlanetsGroups(path, PlayerGameData.groupsOfConnectedPlanets);
                }
            }
        }
    }
}
