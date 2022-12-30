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
        string name = "Scenes/Main Game";
        var status = NetworkManager.SceneManager.LoadScene(name, LoadSceneMode.Single);
    }

    public void ChooseMap1()
    {
        SetMapDataClientRpc();
    }

    public void ChooseMap2()
    {
        SetMapDataClientRpc();
    }

    public void ChooseMap3()
    {
        SetMapDataClientRpc();
    }

    public void ChooseMap4()
    {
        SetMapDataClientRpc();
    }

    [ClientRpc]
    public void SetMapDataClientRpc()
    {
        //Debug.Log(Map.mapData);
        //Debug.Log(availableMapsData);
        Map.mapData = availableMapsData[0];
    }
}
