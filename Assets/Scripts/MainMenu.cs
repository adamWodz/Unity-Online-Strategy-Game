using Assets.GameplayControl;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : NetworkBehaviour
{
    private void Start()
    {
        Debug.Log("Start GAME");
        
        string name = PlayerPrefs.GetString("username");
        if (name.Length == 0)
            PlayerPrefs.SetString("username", "Gracz");
        PlayerGameData.Name = PlayerPrefs.GetString("username");
    }

    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }
}
