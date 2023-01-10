using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Drawing;
using Assets.GameplayControl;

public class PathsPanel : Panel
{
    public GameObject pathButtonPrefab;
    public List<Mission> missionsFromClickedMissionsCards;

    private List<Mission> missionsChoosed;
    public List<Mission> MissionsChoosed
    {
        get
        {
            return missionsChoosed;
        }
        set
        {
            //Debug.Log($"New missions : {value.Count}");
            //Debug.Log($"Mission Choosed (old) : {missionsChoosed.Count}");
            missionsChoosed.AddRange(value);
            //Debug.Log($"Mission Choosed (new) : {missionsChoosed.Count}");

            SpawnMissionsButtons(value);
        }
    }

    private bool firstClick;

    // Start is called before the first frame update
    void Start()
    {
        firstClick = true;

        missionsFromClickedMissionsCards = new();

        transform.parent.GetComponent<Button>().onClick.AddListener(HighlightPlanets);

        //missionsChoosed = GetRandomElementsFromList(GameObject.Find("Space").GetComponent<Map>().Missions, 3);

        missionsChoosed = new();

        Debug.Log($"Missions choosed: {missionsChoosed.Count}");

        AssignValues(368.62f, 611.61f, PanelState.Maximized, true);

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
            missionsFromClickedMissionsCards.AddRange(missionsChoosed.Except(missionsFromClickedMissionsCards, new MissionComparer()).ToList());

            // zmiana koloru przycisków
            for (int i = 0; i < missionsChoosed.Count; i++)
                transform.GetChild(i).GetComponent<Image>().color = UnityEngine.Color.green;

            firstClick = false;
        }
        else
        {
            ChangePlanetsColor(UnityEngine.Color.white);
            missionsFromClickedMissionsCards = missionsFromClickedMissionsCards.Except(missionsChoosed, new MissionComparer()).ToList();

            // zmiana koloru przycisków
            for (int i = 0; i < missionsChoosed.Count; i++)
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
        for (int i = 0; i < missionsChoosed.Count; i++)
        {
            //Debug.Log($"{paths[i].planetFrom.name} - {paths[i].planetTo.name}");
            if (CheckIfPlanetCanBeExtinguished(missionsChoosed[i].start.name, missionsChoosed[i].end.name))
                ChangePlanetColor(color, missionsChoosed[i].start.name);
            if (CheckIfPlanetCanBeExtinguished(missionsChoosed[i].end.name, missionsChoosed[i].start.name))
                ChangePlanetColor(color, missionsChoosed[i].end.name);
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
            missionButton.name = missionButton.transform.GetChild(0).GetComponent<TMP_Text>().text = newMissions[i].start.name + "-" + newMissions[i].end.name;
            missionButton.GetComponent<Button>().onClick.AddListener(() => HighlightPlanet(newMissions[copy]));
        }
    }

    public override void LoadData(GameData data)
    {
        MissionsChoosed = data.missionsForEachPalyer[PlayerGameData.Id];
    }

    public override void SaveData(ref GameData data)
    {
        if (!data.missionsForEachPalyer.ContainsKey(PlayerGameData.Id))
            data.missionsForEachPalyer.Add(PlayerGameData.Id, missionsChoosed);
        else
            data.missionsForEachPalyer[PlayerGameData.Id] = missionsChoosed;
    }
}
