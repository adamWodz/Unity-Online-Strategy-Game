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
    public List<Path> pathsToChoose = new();
    
    public Button[] missionButtonsAndConfirmButton; // 3 pierwsze przyciski to karty misji, a ostatni to zatwierdzenie

    private PathsPanel pathsPanel;

    // Start is called before the first frame update
    void Start()
    {
        pathsPanel = GameObject.Find("PathsPanel").GetComponent<PathsPanel>();
        
        AssignValues(0, 242.9984f, PanelState.Minimized, false);

        pathsToChoose.AddRange(GameObject.Find("Space").GetComponent<Map>().Paths.Except(pathsPanel.PathsChoosed,new PathComparer()).ToList());

        missionButtonsAndConfirmButton = transform.GetComponentsInChildren<Button>();

        var randomPaths = pathsPanel.GetRandomElements(pathsToChoose, 3);
        
        for (int i = 0; i < missionButtonsAndConfirmButton.Length - 1; i++)
        {
            int copy = i;
            missionButtonsAndConfirmButton[copy].name = missionButtonsAndConfirmButton[copy].transform.GetChild(0).GetComponent<TMP_Text>().text = randomPaths[copy].planetFrom.name + "-" + randomPaths[copy].planetTo.name;
            missionButtonsAndConfirmButton[copy].onClick.AddListener(() => pathsPanel.HighlightPlanet(randomPaths[copy]));
        }

        missionButtonsAndConfirmButton[^1].onClick.AddListener(AddMissions);
    }

    // Update is called once per frame
    void Update()
    {
        ChangeWidth();
    }

    void AddMissions()
    {
        pathsPanel.PathsChoosed.AddRange(pathsPanel.pathsFromClickedMissionsCards.Except(pathsPanel.PathsChoosed, new PathComparer()).ToList());
        ChangeState();
        pathsPanel.ChangeState();
        button.enabled = true;
    }
}
