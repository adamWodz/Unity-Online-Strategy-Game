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

    public void StartGame()
    {
        Map.mapData = availableMapsData[0];
        Debug.Log(availableMapsData.Count);
        string name = "Scenes/Main Game";
        var status = NetworkManager.SceneManager.LoadScene(name,LoadSceneMode.Single);
    }
}
