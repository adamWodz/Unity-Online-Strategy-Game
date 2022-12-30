using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : NetworkBehaviour
{

    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }
}
