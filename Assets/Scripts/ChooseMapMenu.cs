using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChooseMapMenu : NetworkBehaviour
{
    public List<MapData> availableMapsData;
    int mapDataNumer;

    public void StartGame()
    {
        SetMapDataClientRpc(mapDataNumer);
        string name = "Scenes/Main Game";
        var status = NetworkManager.SceneManager.LoadScene(name, LoadSceneMode.Single);
    }

    private void StartServer()
    {
        //var networkManager = NetworkManager.Singleton;
        //if (!networkManager.IsClient && !networkManager.IsServer)
        //{
        //    networkManager.StartHost();
        //}

    }

    public void ChooseMap0()
    {
        StartServer();
        mapDataNumer = 0;
        //StartServer();
    }

    public void ChooseMap1()
    {
        StartServer();
        mapDataNumer = 1;
    }

    public void ChooseMap2()
    {
        StartServer();
        mapDataNumer = 2;
    }

    public void ChooseMap3()
    {
        StartServer();
        mapDataNumer = 3;
    }

    [ClientRpc]
    public void SetMapDataClientRpc(int mapNumber)
    {
        Debug.Log("availableMapsData");
        Map.mapData = availableMapsData[mapNumber];
        Debug.Log(Map.mapData);
    }
}
