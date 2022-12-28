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
    public GameObject pathButtonPrefab;
    public List<Path> pathsFromClickedMissionsCards;
    
    private List<Path> pathsChoosed;
    public List<Path> PathsChoosed
    {
        get 
        { 
            return pathsChoosed; 
        }
        set 
        { 
            
            var newPaths = value.Except(pathsChoosed,new PathComparer()).ToList();
            pathsChoosed = value;
            
            GameObject path;

            int n = newPaths.Count;

            for (int i = 0; i < n; i++)
            {
                int copy = i;
                path = Instantiate(pathButtonPrefab, transform);
                path.name = path.transform.GetChild(0).GetComponent<TMP_Text>().text = newPaths[i].planetFrom.name + "-" + newPaths[i].planetTo.name;
                path.GetComponent<Button>().onClick.AddListener(() => HighlightPlanet(newPaths[copy]));
            }
        }
    }

    private bool firstClick;

    // Start is called before the first frame update
    void Start()
    {
        firstClick = true;

        pathsFromClickedMissionsCards = new();

        transform.parent.GetComponent<Button>().onClick.AddListener(HighlightPlanets);

        pathsChoosed = GetRandomElements(GameObject.Find("Space").GetComponent<Map>().Paths, 3);
        
        AssignValues(419.77f, 611.61f, PanelState.Maximized, true);
        
        //GameObject playerTextTempalte = transform.GetChild(0).gameObject;
        GameObject path;

        int n = pathsChoosed.Count;

        for (int i = 0; i < n; i++)
        {
            int copy = i;
            path = Instantiate(pathButtonPrefab, transform);
            path.name = path.transform.GetChild(0).GetComponent<TMP_Text>().text = pathsChoosed[i].planetFrom.name + "-" + pathsChoosed[i].planetTo.name;
            path.GetComponent<Button>().onClick.AddListener(() => HighlightPlanet(pathsChoosed[copy]));
        }

        //Destroy(playerTextTempalte);
    }

    // Update is called once per frame
    void Update()
    {
        ChangeWidth(); 
    }

    public List<Path> GetRandomElements(List<Path> list, int elementsCount)
    {
        return list.OrderBy(arg => Guid.NewGuid()).Take(elementsCount).ToList();
    }

    private void HighlightPlanets()
    {
        if (firstClick)
        {
            ChangePlanetsColor(UnityEngine.Color.green);
            //pathsFromClickedMissionsCards = new(pathsChoosed);
            pathsFromClickedMissionsCards.AddRange(pathsChoosed.Except(pathsFromClickedMissionsCards, new PathComparer()).ToList());
            
            // zmiana koloru przycisków
            for (int i = 0; i < pathsChoosed.Count; i++)
                transform.GetChild(i).GetComponent<Image>().color = UnityEngine.Color.green;
            
            firstClick = false;
        }
        else
        {
            ChangePlanetsColor(UnityEngine.Color.white);
            //pathsFromClickedMissionsCards = new();
            pathsFromClickedMissionsCards = pathsFromClickedMissionsCards.Except(pathsChoosed, new PathComparer()).ToList();
            
            // zmiana koloru przycisków
            for (int i = 0; i < pathsChoosed.Count; i++)
                transform.GetChild(i).GetComponent<Image>().color = UnityEngine.Color.white;
            
            firstClick = true;
        }
    }

    public void HighlightPlanet(Path path)
    {

        string firstPlanetName = path.planetFrom.name;
        string secondPlanetName = path.planetTo.name;
        Debug.Log(firstPlanetName + "-" + secondPlanetName);

        // podswietlenie przycisku danej sciezki
        var pom = GameObject.Find(firstPlanetName + "-" + secondPlanetName).GetComponent<Image>();
        Debug.Log(pom.name);

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

        if (pathsFromClickedMissionsCards.Contains(path))
            pathsFromClickedMissionsCards.Remove(path);
        else
            pathsFromClickedMissionsCards.Add(path);
    }

    void ChangePlanetsColor(UnityEngine.Color color)
    {
        for (int i = 0; i < pathsChoosed.Count; i++)
        {
            //Debug.Log($"{paths[i].planetFrom.name} - {paths[i].planetTo.name}");
            if(CheckIfPlanetCanBeExtinguished(pathsChoosed[i].planetFrom.name, pathsChoosed[i].planetTo.name))
                ChangePlanetColor(color, pathsChoosed[i].planetFrom.name);
            if (CheckIfPlanetCanBeExtinguished(pathsChoosed[i].planetTo.name, pathsChoosed[i].planetFrom.name))
                ChangePlanetColor(color, pathsChoosed[i].planetTo.name);
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
        foreach (Path p in pathsFromClickedMissionsCards)
        {
            if (p.planetFrom.name == planetToExtinguishName && p.planetTo.name != neighbourPlanetFromPathName)
            {
                UnityEngine.Color planetColor = GetPlanetColor(p.planetTo.name);
                if (planetColor == UnityEngine.Color.green)
                {
                    return false;
                }
            }
            else if (p.planetTo.name == planetToExtinguishName && p.planetFrom.name != neighbourPlanetFromPathName)
            {
                UnityEngine.Color planetColor = GetPlanetColor(p.planetFrom.name);
                if (planetColor == UnityEngine.Color.green)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
