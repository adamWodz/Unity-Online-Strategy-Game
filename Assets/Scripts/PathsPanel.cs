using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Drawing;

public class PathsPanel : Panel
{
    public List<Path> paths;

    private bool firstClick;

    private List<Path> pathsFromClickedMissionsCards;

    // Start is called before the first frame update
    void Start()
    {
        firstClick = true;

        pathsFromClickedMissionsCards = new List<Path>();

        transform.parent.GetComponent<Button>().onClick.AddListener(HighlightPlanets);

        paths = GetRandomElements(GameObject.Find("Space").GetComponent<Map>().paths, 3);
        
        AssignValues(419.77f, 611.61f, PanelState.Maximized, true);
        
        GameObject playerTextTempalte = transform.GetChild(0).gameObject;
        GameObject path;

        int n = paths.Count;

        for (int i = 0; i < n; i++)
        {
            int copy = i;
            path = Instantiate(playerTextTempalte, transform);
            path.name = path.transform.GetChild(0).GetComponent<TMP_Text>().text = paths[i].planetFrom.name + "-" + paths[i].planetTo.name;
            path.GetComponent<Button>().onClick.AddListener(() => HighlightPlanet(paths[copy]));
        }

        Destroy(playerTextTempalte);
    }

    // Update is called once per frame
    void Update()
    {
        ChangeWidth(); 
    }

    List<Path> GetRandomElements(List<Path> list, int elementsCount)
    {
        return list.OrderBy(arg => Guid.NewGuid()).Take(elementsCount).ToList();
    }

    void HighlightPlanets()
    {
        if (firstClick)
        {
            ChangePlanetsColor(UnityEngine.Color.green);
            pathsFromClickedMissionsCards = new(paths);
            for (int i = 0; i < paths.Count; i++)
                transform.GetChild(i).GetComponent<Image>().color = UnityEngine.Color.green;
            firstClick = false;
        }
        else
        {
            ChangePlanetsColor(UnityEngine.Color.white);
            pathsFromClickedMissionsCards = new();
            for (int i = 0; i < paths.Count; i++)
                transform.GetChild(i).GetComponent<Image>().color = UnityEngine.Color.white;
            firstClick = true;
        }
    }

    void HighlightPlanet(Path path)
    {
        string firstPlanetName = path.planetFrom.name;
        string secondPlanetName = path.planetTo.name;

        var pom = GameObject.Find(firstPlanetName + "-" + secondPlanetName).GetComponent<Image>();
        Debug.Log(pom.name);
        
        if (pom.color == UnityEngine.Color.white)
        {
           pom.color = UnityEngine.Color.green;
        }
        else
        {
            pom.color= UnityEngine.Color.white;
        }
        
        UnityEngine.Color firstPlanetColor = GetPlanetColor(firstPlanetName);
        UnityEngine.Color secondPlanetColor = GetPlanetColor(secondPlanetName);

        if(firstPlanetColor != UnityEngine.Color.green || secondPlanetColor != UnityEngine.Color.green) 
        {
            ChangePlanetColor(UnityEngine.Color.green, firstPlanetName);
            ChangePlanetColor(UnityEngine.Color.green, secondPlanetName);
        }
        else
        {
            if(CheckIfPlanetCanBeExtinguished(firstPlanetName,secondPlanetName))
            {
                ChangePlanetColor(UnityEngine.Color.white, firstPlanetName);
            }

            if (CheckIfPlanetCanBeExtinguished(secondPlanetName, firstPlanetName))
            {
                ChangePlanetColor(UnityEngine.Color.white, secondPlanetName);
            }
        }

        if(pathsFromClickedMissionsCards.Contains(path))
            pathsFromClickedMissionsCards.Remove(path);
        else
            pathsFromClickedMissionsCards.Add(path);
    }

    void ChangePlanetsColor(UnityEngine.Color color)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            //Debug.Log($"{paths[i].planetFrom.name} - {paths[i].planetTo.name}");
            ChangePlanetColor(color, paths[i].planetFrom.name);
            ChangePlanetColor(color, paths[i].planetTo.name);
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
        return GameObject.Find(name + "(Clone)").GetComponent<Renderer>();
    }

    bool CheckIfPlanetCanBeExtinguished(string planetToExtinguishName, string neighbourPlanetFromPathName)
    {
        foreach (Path path in pathsFromClickedMissionsCards)
        {
            if (path.planetFrom.name == planetToExtinguishName && path.planetTo.name != neighbourPlanetFromPathName)
            {
                UnityEngine.Color planetColor = GetPlanetColor(path.planetTo.name);
                if (planetColor == UnityEngine.Color.green)
                {
                    return false;
                }
            }
            else if (path.planetTo.name == planetToExtinguishName && path.planetFrom.name != neighbourPlanetFromPathName)
            {
                UnityEngine.Color planetColor = GetPlanetColor(path.planetFrom.name);
                if (planetColor == UnityEngine.Color.green)
                {
                    return false;
                }
            }
        }
        
        return true;  
    }
}
