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
using UnityEngine.UIElements;

public class EndGame : MonoBehaviour
{
    public void ExitGame()
    {
        NetworkManager.Singleton.Shutdown();
        
        string name = "Scenes/Menu";
        SceneManager.LoadScene(name, LoadSceneMode.Single);
    }
}
