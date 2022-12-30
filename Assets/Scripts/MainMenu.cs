using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : NetworkBehaviour
{
    public List<MapData> availableMapsData;

    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }

    public void StartGame1()
    {
        //Map.mapData = availableMapsData[0];
        //Map.mapData = availableMapsData[1];
        SetMapDataClientRpc();
        Debug.Log("StartGame1");
        string name = "Scenes/Main Game";
        var status = NetworkManager.SceneManager.LoadScene(name, LoadSceneMode.Single);
    }

    [ClientRpc]
    public void SetMapDataClientRpc()
    {
        //Debug.Log(Map.mapData);
        //Debug.Log(availableMapsData);
        Map.mapData = availableMapsData[0];
    }
}
