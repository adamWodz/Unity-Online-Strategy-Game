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

    public void StartGame()
    {
        // indeksy scen znajduj¹ siê w 'File->Build Settings'
        //SceneManager.LoadScene(1);
        string name = "Scenes/Main Game";
        var status = NetworkManager.SceneManager.LoadScene(name,LoadSceneMode.Single);
    }
}
