using Assets.GameplayControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class EndGame : MonoBehaviour
{
    GameObject mapViewButton;
    public List<GameObject> otherThanToggle;
    bool mapView = false;
    List<GameObject> mapObjects;

    private void Start()
    {
        mapView = false;
        mapViewButton = gameObject.transform.Find("MapViewButton").transform.Find("ButtonLabel").gameObject;
        mapObjects = GameObject.Find("GameManager").GetComponent<GameManager>().spawnedObjects;
    }

    public void MapViewButtonClicked()
    {
        if(!mapView)
        {
            foreach(GameObject obj in otherThanToggle)
                obj.SetActive(false);

            foreach (GameObject obj in mapObjects)
                if (obj != null)
                    obj.SetActive(true);
            mapViewButton.GetComponent<TextMeshProUGUI>().text = "Zamknij widok mapy";
            mapView = true;
        }

        else
        {
            foreach (GameObject obj in otherThanToggle)
                obj.SetActive(true);

            foreach (GameObject obj in mapObjects)
                if (obj != null)
                    obj.SetActive(false);
            mapViewButton.GetComponent<TextMeshProUGUI>().text = "Widok mapy";
            mapView = false;
        }
    }

    public void ExitGame()
    {
        NetworkManager.Singleton.Shutdown();
        
        if(Map.mapData.missions != null)
            foreach(Mission mission in Map.mapData.missions)
            {
                mission.isDone = false;
            }

        if (Map.mapData.missions != null)
            foreach (Path path in Map.mapData.paths)
            {
                path.isBuilt = false;
                path.builtById = -1;
            }

        PlayerGameData.Reset();
        Server.Reset();
        Communication.Reset();

        //Application.Quit();

        AuthenticationService.Instance.SignOut();

        string name = "Scenes/Menu";
        SceneManager.LoadScene(name, LoadSceneMode.Single);


    }
}
